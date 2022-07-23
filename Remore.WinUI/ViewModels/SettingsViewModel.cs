using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebounceThrottle;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic.Devices;
using NAudio.CoreAudioApi;
using NAudio.Wave;

using Remore.WinUI.Contracts.Services;
using Remore.WinUI.Helpers;
using Remore.WinUI.KeyBindings;
using Remore.WinUI.Services;
using Windows.ApplicationModel;
using WinUIEx;

namespace Remore.WinUI.ViewModels
{
    public class SettingsViewModel : ObservableRecipient
    {
        private readonly IThemeSelectorService _themeSelectorService;
        private readonly KeyBindingsService _keyBindingsService;
        private readonly LocalizationService _localizationService;
        private ElementTheme _elementTheme;
        private ILogger<SettingsViewModel> _logger;
        private ThrottleDispatcher _throttleDispatcher;
        public const string InputDeviceSettingsKey = "InputDeviceSettingsKey";
        public const string OutputDeviceSettingsKey = "OutputDeviceSettingsKey";
        public const string UseVoiceActivityDetectionSettingsKey = "UseVoiceActivityDetectionSettingsKey";
        public const string VoiceActivityDetectionThresholdSettingsKey = "VoiceActivityDetectionThresholdSettingsKey";
        public const string UsernameSettingsKey = "UsernameSettingsKey";
        public const string LanguageSettingsKey = "LanguageSettingsKey";
        public const string FavoritesSettingsKey = "FavoritesTabSettingsKey";
        public const string EnableRNNoiseSuppressionSettingsKey = "EnableRNNoiseSuppressionSettingsKey";
        public const string KeyBindingsListSettingsKey = "KeyBindingsListSettingsKey";
        public const string ClientVersion = "1.0.0";

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { SetProperty(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { SetProperty(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            if (ElementTheme != param)
                            {
                                ElementTheme = param;
                                await _themeSelectorService.SetThemeAsync(param);
                            }
                        });
                }

                return _switchThemeCommand;
            }
        }

        private ObservableCollection<string> outputDevices;
        public ObservableCollection<string> OutputDevices
        {
            get { return outputDevices; }
            set { this.SetProperty(ref outputDevices, value); }
        }

        public List<string> Actions { get; }

        private ObservableCollection<string> inputDevices;
        public ObservableCollection<string> InputDevices
        {
            get { return inputDevices; }
            set { this.SetProperty(ref inputDevices, value); }
        }

        private int outputDevice;

        public int OutputDevice
        {
            get { return outputDevice; }
            set
            {
                if (this.SetProperty(ref outputDevice, value))
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await SettingsService.SaveSettingAsync(OutputDeviceSettingsKey, value);
                        if (Main.CurrentChannel == null) return;
                        var channelId = Main.CurrentChannel.Id.ToString();
                        Main.StopAudioPlayback();
                        Main.StopAudioPlayback();
                    });

            }
        }

        private int inputDevice;

        public int InputDevice
        {
            get { return inputDevice; }
            set
            {
                if (this.SetProperty(ref inputDevice, value))
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await SettingsService.SaveSettingAsync(InputDeviceSettingsKey, value);
                        if (Main.CurrentChannel == null) return;
                        var channelId = Main.CurrentChannel.Id.ToString();
                        Main.StopEncoding();
                        Main.StartEncoding(Main.CurrentChannel.Bitrate);

                    });

            }
        }

        private bool isNicknameErrored;

        public bool IsNicknameErrored
        {
            get { return isNicknameErrored; }
            set { SetProperty(ref isNicknameErrored, value); }
        }


        private bool useVoiceActivityDetection;

        public bool UseVoiceActivityDetection
        {
            get { return useVoiceActivityDetection; }
            set
            {
                if (SetProperty(ref useVoiceActivityDetection, value))
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await SettingsService.SaveSettingAsync<bool>(UseVoiceActivityDetectionSettingsKey, value);
                    });
                }
            }
        }
        private double voiceActivityDetectionThreshold;

        public double VoiceActivityDetectionThreshold
        {
            get { return voiceActivityDetectionThreshold; }
            set
            {
                if (SetProperty(ref voiceActivityDetectionThreshold, value))
                {
                    _throttleDispatcher.Throttle(() =>
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                        {
                            await SettingsService.SaveSettingAsync<double>(VoiceActivityDetectionThresholdSettingsKey, value);
                        });
                    });
                }
            }
        }

        private string username;

        public string Username
        {
            get { return username; }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
                {
                    IsNicknameErrored = true;
                    return;
                }
                IsNicknameErrored = false;
                if (SetProperty(ref username, value))
                {
                    _throttleDispatcher.Throttle(() =>
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                        {
                            await SettingsService.SaveSettingAsync<string>(UsernameSettingsKey, value);
                        });
                    });


                }
            }
        }


        private ObservableCollection<string> languages;

        public ObservableCollection<string> Languages
        {
            get { return languages; }
            set { SetProperty(ref languages, value); }
        }

        private ObservableCollection<KeyBinding> keyBindings;

        public ObservableCollection<KeyBinding> KeyBindings
        {
            get { return keyBindings; }
            set { SetProperty(ref keyBindings, value); }
        }

        public RelayCommand AddKeyBinding { get; }
        public RelayCommand<KeyBinding> RemoveKeyBinding { get; }

        private int selectedLanguage;

        public int SelectedLanguage
        {
            get { return selectedLanguage; }
            set
            {
                if (SetProperty(ref selectedLanguage, value))
                {
                    if (_localizationService.UpdateLanguage(_localizationService.Languages[value]))
                    {
                        App.GetService<INavigationService>().NavigateTo(typeof(MainViewModel).FullName, null, true);
                        App.GetService<INavigationService>().NavigateTo(typeof(SettingsViewModel).FullName, null, true);
                    }
                }
            }
        }
        private int selectedBackdrop;

        public int SelectedBackdrop
        {
            get { return selectedBackdrop; }
            set
            {
                if (SetProperty(ref selectedBackdrop, value))
                {
                    if (value is 0)
                        App.MainWindow.Backdrop = new MicaSystemBackdrop();
                    else if (value is 1)
                        App.MainWindow.Backdrop = new AcrylicSystemBackdrop();
                    else
                        App.MainWindow.Backdrop = null;
                }

            }
        }



        private bool isDevicesLoaded;

        public bool IsDevicesLoaded
        {
            get { return isDevicesLoaded; }
            set { SetProperty(ref isDevicesLoaded, value); }
        }

        private bool enableRNNoiseSuppression;

        public bool EnableRNNoiseSuppression
        {
            get { return enableRNNoiseSuppression; }
            set
            {
                if (SetProperty(ref enableRNNoiseSuppression, value))
                {
                    _throttleDispatcher.Throttle(() =>
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                        {
                            await SettingsService.SaveSettingAsync<bool>(EnableRNNoiseSuppressionSettingsKey, enableRNNoiseSuppression);
                        });
                    });
                }
            }
        }


        public MainViewModel Main { get; }
        public ILocalSettingsService SettingsService { get; }

        public SettingsViewModel(IThemeSelectorService themeSelectorService, ILogger<SettingsViewModel> logger, KeyBindingsService keyBindingsService, LocalizationService localizationService, MainViewModel mainViewModel, ILocalSettingsService settingsService)
        {
            _themeSelectorService = themeSelectorService;
            _keyBindingsService = keyBindingsService;
            _localizationService = localizationService;
            _elementTheme = _themeSelectorService.Theme;
            _logger = logger;
            _throttleDispatcher = new DebounceThrottle.ThrottleDispatcher(500);

            VersionDescription = GetVersionDescription();
            SettingsService = settingsService;
            Main = mainViewModel;
            InputDevices = new();
            OutputDevices = new();
            Actions = typeof(KeyBindingAction).GetEnumNames().Select(x => Regex.Replace(x.ToString(), "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}")).ToList();
            KeyBindings = new(_keyBindingsService.KeyBindings.ToList());

            if (App.MainWindow.Backdrop is MicaSystemBackdrop)
                selectedBackdrop = 0;
            else if (App.MainWindow.Backdrop is AcrylicSystemBackdrop)
                selectedBackdrop = 1;
            else
                selectedBackdrop = 2;


            AddKeyBinding = new RelayCommand(async () =>
            {
                var content = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };
                var textBox = new TextBox()
                {
                    MinWidth = 150,
                    MaxWidth = 150,
                    PlaceholderText = "Settings_KeyBindings_KeyInputPlaceholder".GetLocalized()
                };
                PInvoke.User32.VirtualKey pinvokeKey = PInvoke.User32.VirtualKey.VK_NO_KEY;
                textBox.KeyDown += (s, e) =>
                {
                    if (e.Key == Windows.System.VirtualKey.LeftButton ||
                        e.Key == Windows.System.VirtualKey.RightButton ||
                        e.Key == Windows.System.VirtualKey.MiddleButton)
                        return;
                    textBox.Text = e.Key.ToString();
                    pinvokeKey = (PInvoke.User32.VirtualKey)(int)e.Key;
                    textBox.IsEnabled = false;
                    textBox.IsEnabled = true;
                };
                var comboBox = new ComboBox()
                {
                    MinWidth = 150,
                    MaxWidth = 150,
                    PlaceholderText = "Settings_KeyBindings_ActionInputPlaceholder".GetLocalized(),
                    Margin = new(24, 0, 0, 0)
                };
                var actions = Actions.ToList();
                actions.Remove(KeyBindingAction.Unassigned.ToString());
                comboBox.ItemsSource = actions;
                content.Children.Add(textBox);
                content.Children.Add(comboBox);
                _keyBindingsService.IsKeyBindingsEnabled = false;
                var contentDialogResult = await new ContentDialog()
                {
                    Title = "Settings_KeyBindings_DialogTitle".GetLocalized(),
                    Content = content,
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    CloseButtonText = "Main_ConnectToServer_CloseButton".GetLocalized(),
                    PrimaryButtonText = "Settings_KeyBindings_AddButton".GetLocalized()
                }.ShowAsync(ContentDialogPlacement.InPlace);
                if (contentDialogResult == ContentDialogResult.Primary)
                {
                    if (pinvokeKey == PInvoke.User32.VirtualKey.VK_NO_KEY)
                    {
                        _keyBindingsService.IsKeyBindingsEnabled = true;
                        return;
                    }
                    var keyBinding = new KeyBinding()
                    {
                        Action = Enum.Parse<KeyBindingAction>(comboBox.SelectedItem.ToString().Replace(" ", ""), true),
                        Key = pinvokeKey
                    };

                    if (!_keyBindingsService.RegisterKeyBinding(keyBinding))
                    {
                        await new ContentDialog()
                        {
                            Title = "Settings_KeyBindings_DialogTitle".GetLocalized(),
                            Content = "Settings_KeyBindingsFailed".GetLocalized(),
                            XamlRoot = App.MainWindow.Content.XamlRoot,
                            CloseButtonText = "Main_ConnectToServer_CloseButton".GetLocalized(),
                        }.ShowAsync(ContentDialogPlacement.InPlace);
                    }
                    else
                    {
                        KeyBindings.Add(keyBinding);
                    }
                }
                _keyBindingsService.IsKeyBindingsEnabled = true;
            });
            RemoveKeyBinding = new RelayCommand<KeyBinding>((binding) =>
            {
                _keyBindingsService.RemoveKeyBinding(binding.Key);
                KeyBindings.Remove(binding);
            });
            Task.Run(() =>
            {
                // Protection from unfriendly devices that throw exception when you try to access their name :((
                try
                {


                    var enumerator = new MMDeviceEnumerator();
                    int waveOutDevices = WaveOut.DeviceCount;
                    for (int waveOutDevice = 0; waveOutDevice < waveOutDevices; waveOutDevice++)
                    {
                        WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveOutDevice);
                        foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All))
                        {
                            try
                            {
                                if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                                {
                                    App.MainWindow.DispatcherQueue.TryEnqueue(() => OutputDevices.Add(device.FriendlyName));
                                    break;
                                }
                            }
                            catch { }
                        }
                    }

                    if (WaveOut.DeviceCount > 0)
                        App.MainWindow.DispatcherQueue.TryEnqueue(async () => OutputDevice = await SettingsService.ReadSettingAsync<int>(OutputDeviceSettingsKey));

                    int waveInDevices = WaveIn.DeviceCount;
                    for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
                    {
                        WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                        foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All))
                        {
                            try
                            {
                                if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                                {
                                    App.MainWindow.DispatcherQueue.TryEnqueue(() => InputDevices.Add(device.FriendlyName));
                                    break;
                                }
                            }
                            catch { }
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                }
                if (WaveIn.DeviceCount > 0)
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () => InputDevice = await SettingsService.ReadSettingAsync<int>(InputDeviceSettingsKey));
                App.MainWindow.DispatcherQueue.TryEnqueue(() => IsDevicesLoaded = true);
            });

            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                Username = await SettingsService.ReadSettingAsync<string>(UsernameSettingsKey);
                UseVoiceActivityDetection = await SettingsService.ReadSettingAsync<bool>(UseVoiceActivityDetectionSettingsKey);
                VoiceActivityDetectionThreshold = await SettingsService.ReadSettingAsync<double>(VoiceActivityDetectionThresholdSettingsKey);
                EnableRNNoiseSuppression = await SettingsService.ReadSettingAsync<bool>(EnableRNNoiseSuppressionSettingsKey);
            });

            Languages = new(localizationService.Languages.Select(x => x.NativeName.Split("(")[0].Trim().Capitalize()));
            selectedLanguage = localizationService.Languages.ToList().IndexOf(localizationService.CurrentLanguage);
        }



        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            return $"{appName} {ClientVersion}";
        }
    }
}
