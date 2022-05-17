using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;
using Microsoft.VisualBasic.Devices;
using NAudio.CoreAudioApi;
using NAudio.Wave;

using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.Helpers;
using TTalk.WinUI.Services;
using Windows.ApplicationModel;
namespace TTalk.WinUI.ViewModels
{
    public class SettingsViewModel : ObservableRecipient
    {
        private readonly IThemeSelectorService _themeSelectorService;
        private readonly LocalizationService localizationService;
        private ElementTheme _elementTheme;

        public const string InputDeviceSettingsKey = "InputDeviceSettingsKey";
        public const string OutputDeviceSettingsKey = "OutputDeviceSettingsKey";
        public const string UseVoiceActivityDetectionSettingsKey = "UseVoiceActivityDetectionSettingsKey";
        public const string VoiceActivityDetectionThresholdSettingsKey = "VoiceActivityDetectionThresholdSettingsKey";
        public const string UsernameSettingsKey = "UsernameSettingsKey";
        public const string LanguageSettingsKey = "LanguageSettingsKey";
        public const string FavoritesSettingsKey = "FavoritesTabSettingsKey";
        public const string EnableRNNoiseSuppressionSettingsKey = "EnableRNNoiseSuppressionSettingsKey";
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
                    Action<double> execute = (value) =>
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                        {
                            await SettingsService.SaveSettingAsync<double>(VoiceActivityDetectionThresholdSettingsKey, value);
                        });
                    };
                    var debounceWrapper = execute.Debounce(1500);
                    debounceWrapper(value);
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
                    Action<string> execute = async (username) =>
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                        {
                            await SettingsService.SaveSettingAsync<string>(UsernameSettingsKey, username);
                        });
                    };
                    var debounceWrapper = execute.Debounce(500);
                    debounceWrapper(value);

                }
            }
        }


        private ObservableCollection<string> languages;

        public ObservableCollection<string> Languages
        {
            get { return languages; }
            set { SetProperty(ref languages, value); }
        }

        private int selectedLanguage;

        public int SelectedLanguage
        {
            get { return selectedLanguage; }
            set
            {
                if (SetProperty(ref selectedLanguage, value))
                {
                    if (localizationService.UpdateLanguage(localizationService.Languages[value]))
                    {
                        App.GetService<INavigationService>().NavigateTo(typeof(MainViewModel).FullName, null, true);
                        App.GetService<INavigationService>().NavigateTo(typeof(SettingsViewModel).FullName, null, true);
                    }
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
                    Action<bool> execute = async (enableRNNoiseSuppression) =>
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                        {
                            await SettingsService.SaveSettingAsync<bool>(EnableRNNoiseSuppressionSettingsKey, enableRNNoiseSuppression);
                        });
                    };
                    var debounceWrapper = execute.Debounce(500);
                    debounceWrapper(value);
                }
            }
        }


        public MainViewModel Main { get; }
        public ILocalSettingsService SettingsService { get; }

        public SettingsViewModel(IThemeSelectorService themeSelectorService, LocalizationService localizationService, MainViewModel mainViewModel, ILocalSettingsService settingsService)
        {
            _themeSelectorService = themeSelectorService;
            this.localizationService = localizationService;
            Main = mainViewModel;
            SettingsService = settingsService;
            _elementTheme = _themeSelectorService.Theme;
            VersionDescription = GetVersionDescription();

            InputDevices = new();
            OutputDevices = new();

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
                            if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                            {
                                App.MainWindow.DispatcherQueue.TryEnqueue(() => OutputDevices.Add(device.FriendlyName));
                                break;
                            }
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
                            if (device.FriendlyName.StartsWith(deviceInfo.ProductName))
                            {
                                App.MainWindow.DispatcherQueue.TryEnqueue(() => InputDevices.Add(device.FriendlyName));
                                break;
                            }
                        }

                    }
                }
                catch (Exception)
                {
                    ;
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
