using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebounceThrottle;
using FragLabs.Audio.Codecs;
using Microsoft.UI.Xaml.Controls;
using NAudio.Wave;
using NWaves.Filters;
using Remore.Library.Packets.Client;
using Remore.Library.Packets.Server;
using Remore.WinUI.Contracts.Services;
using Remore.WinUI.Helpers;
using Remore.WinUI.KeyBindings;
using Remore.WinUI.Models;
using Remore.WinUI.AudioProcessing;
using Remore.WinUI.Services;
using Windows.ApplicationModel.Resources.Core;
using Windows.Media.SpeechSynthesis;
using Remore.WinUI.Views;
using Microsoft.Extensions.Logging;
using DnsClient;
using Remore.Client.Core;
using Remore.Library.Packets;
using Remore.Client.Core.Exceptions;
using Fluent.Icons;

namespace Remore.WinUI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel(ILocalSettingsService settingsService, ILogger<MainViewModel> logger, SoundService sounds, KeyBindingsService bindingsService, ILookupClient lookupClient)
        {
            Process.GetCurrentProcess().Exited += OnExited;
            Channels = new();


            _segmentFrames = 960;
            _microphoneQueueSlim = new(0);
            _audioQueueSlim = new(0);
            _audioQueue = new();

            _logger = logger;
            _logger.LogInformation("\n\n");
            _logger.LogInformation(new string('=', 100));
            _throttleDispatcher = new DebounceThrottle.ThrottleDispatcher(1000);
            SettingsService = settingsService;
            SettingsService.SettingsUpdated += OnSettingsUpdated;

            BindingsService = bindingsService;
            BindingsService.KeyBindingExecuted += OnKeybindingExecuted;
            Sounds = sounds;
            LookupClient = lookupClient;

            Task.Run(SendAudio);
            Task.Run(PlayAudio);
            ShowConnectDialog = new RelayCommand(CreateConnectDialog(settingsService));
            DisconnectCommand = new RelayCommand(() =>
            {
                Disconnect();
            });
            LeaveChannel = new RelayCommand(() =>
            {
                _ = _client.SendPacketTCP(new LeaveChannelPacket());
            });
            ToggleMute = new RelayCommand(async () =>
            {
                _throttleDispatcher.Throttle(() =>
                {
                    if (CurrentChannelClient != null)
                    {
                        CurrentChannelClient.IsMuted = !CurrentChannelClient.IsMuted;
                    }
                });
            });
            OpenServerSettingsViewCommand = new RelayCommand(() =>
            {
                OpenServerSettings();
            });
            CreateChannelDialogCommand = new RelayCommand(CreateChannelDialog);
            DeleteChannelDialogCommand = new RelayCommand<string>(DeleteChannelDialog);
            ShowUpdatePriviligeKeyDialog = new RelayCommand(async () =>
            {
                var address = $"{ip}:{port}";
                var textBox = new TextBox() { PlaceholderText = "Main_PrivilegeKey.PlaceholderText".GetLocalized(), MinWidth = 400 };
                var res = await new ContentDialog()
                {
                    Title = "Main_PriviligeKey.Title".GetLocalized(),
                    Content = textBox,
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    CloseButtonText = "Main_PrivilegeKey_Close.Text".GetLocalized(),
                    PrimaryButtonText = "Main_PrivilegeKey_Confirm.Text".GetLocalized(),
                }.ShowAsync(ContentDialogPlacement.InPlace);
                if (res == ContentDialogResult.Primary)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await SettingsService.SaveSettingAsync<string>($"{address}PrivilegeKey", textBox.Text);
                        var res = await new ContentDialog()
                        {
                            Title = "!",
                            Content = "Main_PrivilegeKey_AfterConfirmNotification.Text".GetLocalized(),
                            XamlRoot = App.MainWindow.Content.XamlRoot,
                            CloseButtonText = "Main_PrivilegeKey_Close.Text".GetLocalized(),
                        }.ShowAsync(ContentDialogPlacement.InPlace);
                    });
                }
            });
            _denoiser = new Denoiser();
            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                IsConnected = true;
                await ReadSettings();
                IsConnected = false;
                StartAudioPlayback();
            });
        }

        private void OpenServerSettings()
        {

            var navService = App.GetService<INavigationService>();

            var viewModel = new ServerSettingsViewModel();
            viewModel.Init((name, maxClients) =>
                {
                    _ = _client.SendPacketTCP(new UpdateServerInfoPacket()
                    {
                        Name = name,
                        MaxClients = maxClients
                    });
                }, ServerName, ServerMaxClients);
            navService.NavigateTo(typeof(ServerSettingsViewModel).FullName, viewModel);

        }

        private async void DeleteChannelDialog(string obj)
        {
            var channel = Channels.FirstOrDefault(x => x.Id == obj);
            var result = await new ContentDialog()
            {
                Title = "Main_DeleteChannelDialog_Title".GetLocalized(),
                Content = string.Format("Main_DeleteChannelDialog_Content".GetLocalized(), channel.Name),
                XamlRoot = App.MainWindow.Content.XamlRoot,
                PrimaryButtonText = "Main_DeleteChannelDialog_Confirm".GetLocalized(),
                CloseButtonText = "Main_PrivilegeKey_Close.Text".GetLocalized()
            }.ShowAsync(ContentDialogPlacement.InPlace);
            if (result == ContentDialogResult.Primary)
            {
                await _client.SendPacketTCP(new DeleteChannelPacket() { ChannelId = channel.Id });
            }
        }

        private async void CreateChannelDialog()
        {
            var stack = new StackPanel()
            {
                Spacing = 8,
            };
            var radioButtons = new RadioButtons();
            radioButtons.Items.Add(new RadioButton() { Tag = "Text", Content = "Main_CreateChannelDialog_TextChannel".GetLocalized() });
            radioButtons.Items.Add(new RadioButton() { Tag = "Voice", Content = "Main_CreateChannelDialog_VoiceChannel".GetLocalized() });
            radioButtons.SelectedIndex = 0;
            stack.Children.Add(radioButtons);
            var channelNameInput = new TextBox()
            {
                PlaceholderText = "Main_CreateChannelDialog_ChannelNameInputPlaceholder".GetLocalized(),
                MaxLength = 32
            };
            stack.Children.Add(channelNameInput);
            var bitrateSlider = new Slider()
            {
                Minimum = 8,
                StepFrequency = 1,
                Maximum = 480,
                Visibility = Microsoft.UI.Xaml.Visibility.Collapsed,
                Header = "Main_CreateChannelDialog_Bitrate".GetLocalized(),
            };
            stack.Children.Add(bitrateSlider);
            radioButtons.SelectionChanged += (s, e) =>
            {
                var item = e.AddedItems.FirstOrDefault();
                if (item != null)
                {
                    bitrateSlider.Visibility = (item as RadioButton).Tag.ToString() == "Voice" ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
                }
            };
            var dialog = new ContentDialog()
            {
                Title = "Main_CreateChannelDialog_Title".GetLocalized(),
                Content = stack,
                PrimaryButtonText = "Main_CreateChannelDialog_ConfirmText".GetLocalized(),
                CloseButtonText = "Main_PrivilegeKey_Close.Text".GetLocalized(),
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            channelNameInput.TextChanged += (s, e) =>
            {
                dialog.IsPrimaryButtonEnabled = channelNameInput.Text.Length > 0;
            };
            var result = await dialog.ShowAsync(ContentDialogPlacement.InPlace);
            if (result == ContentDialogResult.Primary)
            {
                _ = _client.SendPacketTCP(new CreateChannelPacket()
                {
                    Name = channelNameInput.Text,
                    Bitrate = (int)bitrateSlider.Value * 1000,
                    ChannelType = radioButtons.SelectedIndex == 0 ? Library.Enums.ChannelType.Text : Library.Enums.ChannelType.Voice,
                    MaxClients = 999999,
                });
            }
        }
        private void OnKeybindingExecuted(object sender, KeyBindingExecutedEventArgs e)
        {
            var keyBinding = e.KeyBinding;
            switch (keyBinding.Action)
            {
                case KeyBindingAction.ToggleMute:
                    {
                        ToggleMute.Execute(null);
                        break;
                    }
                case KeyBindingAction.ToggleDeafen:
                    break;
                case KeyBindingAction.Unassigned:
                    break;
            };


        }

        private async void OnSettingsUpdated(object sender, object e)
        {
            await ReadSettings();
        }

        private async Task ReadSettings()
        {
            Username = await SettingsService.ReadSettingAsync<string>(SettingsViewModel.UsernameSettingsKey);
            UseVoiceActivityDetection = await SettingsService.ReadSettingAsync<bool>(SettingsViewModel.UseVoiceActivityDetectionSettingsKey);
            VoiceActivityDetectionThreshold = await SettingsService.ReadSettingAsync<double>(SettingsViewModel.VoiceActivityDetectionThresholdSettingsKey);
            InputDevice = await SettingsService.ReadSettingAsync<int>(SettingsViewModel.InputDeviceSettingsKey);
            OutputDevice = await SettingsService.ReadSettingAsync<int>(SettingsViewModel.OutputDeviceSettingsKey);
            EnableRNNoiseSuppression = await SettingsService.ReadSettingAsync<bool>(SettingsViewModel.EnableRNNoiseSuppressionSettingsKey);
        }

        private RemoreClient _client;
        private CancellationTokenSource _cts;
        private bool? voiceAllowed = null;

        #region Reactive Properties

        [ObservableProperty]
        private bool isMessagesNotLoading;

        [ObservableProperty]
        private string serverName;

        [ObservableProperty]
        private int serverMaxClients;

        private string address;

        public string Address
        {
            get { return address; }
            set { this.SetProperty(ref address, value); }
        }

        private string messageContent;

        public string MessageContent
        {
            get { return messageContent; }
            set { this.SetProperty(ref messageContent, value); }
        }

        private ObservableCollection<Channel> channels;

        public ObservableCollection<Channel> Channels
        {
            get { return channels; }
            set { this.SetProperty(ref channels, value); }
        }

        private int _segmentFrames;
        private bool isConnected;
        private string ip;
        private int port;

        public bool IsConnected
        {
            get { return isConnected; }
            set { this.SetProperty(ref isConnected, value); }
        }

        private Channel currentTextChannel;

        public Channel CurrentTextChannel
        {
            get { return currentTextChannel; }
            set { this.SetProperty(ref currentTextChannel, value); }
        }


        private bool isNegotiationFinished;

        public bool IsNegotiationFinished
        {
            get { return isNegotiationFinished; }
            set { SetProperty(ref isNegotiationFinished, value); }
        }

        private bool canEditServerSettings;

        public bool CanEditServerSettings
        {
            get { return canEditServerSettings; }
            set { SetProperty(ref canEditServerSettings, value); }
        }


        private ChannelClient currentChannelClient;

        public ChannelClient CurrentChannelClient
        {
            get { return currentChannelClient; }
            set { SetProperty(ref currentChannelClient, value); }
        }


        public Channel CurrentChannel { get; set; }
        private bool isNotConnectingToChannel;

        public bool IsNotConnectingToChannel
        {
            get { return isNotConnectingToChannel; }
            set { this.SetProperty(ref isNotConnectingToChannel, value); }
        }

        public ICommand ShowConnectDialog { get; }
        public RelayCommand DisconnectCommand { get; }
        public RelayCommand LeaveChannel { get; }
        public RelayCommand ToggleMute { get; }
        public RelayCommand OpenServerSettingsViewCommand { get; }
        public RelayCommand CreateChannelDialogCommand { get; }
        public RelayCommand<string> DeleteChannelDialogCommand { get; }
        public RelayCommand ShowUpdatePriviligeKeyDialog { get; }

        private Denoiser _denoiser;

        public ILookupClient LookupClient { get; }
        public ILocalSettingsService SettingsService { get; }
        public SoundService Sounds { get; }
        public KeyBindingsService BindingsService { get; }
        public string Username { get; private set; }
        public bool UseVoiceActivityDetection { get; private set; }
        public double VoiceActivityDetectionThreshold { get; private set; }
        public bool EnableRNNoiseSuppression { get; private set; }
        public int InputDevice { get; private set; }
        public int OutputDevice { get; private set; }
        public ListView MessagesListBox { get; internal set; }

        private Channel _channel;
        private int _bytesPerSegment;


        #endregion
        #region Audio 
        private WaveIn _waveIn;
        private WaveOut _waveOut;
        private BufferedWaveProvider _playBuffer;
        private OpusEncoder _encoder;
        private OpusDecoder _decoder;

        private Queue<byte[]> _microphoneAudioQueue;
        private Queue<byte[]> _audioQueue;
        private ILogger<MainViewModel> _logger;
        private ThrottleDispatcher _throttleDispatcher;
        private ThrottleDispatcher _throttleDispatcherForSpeech;
        private SemaphoreSlim _microphoneQueueSlim;
        private SemaphoreSlim _audioQueueSlim;

        public void StartAudioPlayback()
        {
            _logger.LogInformation("Starting audio playback");
            _decoder = OpusDecoder.Create(48000, 1);
            _decoder.ForwardErrorCorrection = true;

            _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _waveOut.PlaybackStopped += OnWaveOutPlaybackStopped;
            _waveOut.DeviceNumber = OutputDevice;
            _playBuffer = new BufferedWaveProvider(new NAudio.Wave.WaveFormat(48000, 16, 1));
            _waveOut.Init(_playBuffer);
            _waveOut.Play();
            _logger.LogInformation("Audio playback initialized");
        }

        private void OnWaveOutPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            StopAudioPlayback();
            StartAudioPlayback();
        }

        public void StopAudioPlayback()
        {
            _waveOut?.Stop();
            _playBuffer = null;
            _waveOut = null;
            _decoder = null;
            _logger.LogInformation("Audio playback stopped");

        }

        public void StartEncoding(int bitRate)
        {
            _logger.LogInformation("Initializing OPUS encoder");

            _encoder = OpusEncoder.Create(48000, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
            _encoder.Bitrate = bitRate;
            _bytesPerSegment = _encoder.FrameByteCount(_segmentFrames);

            _waveIn = new WaveIn(WaveCallbackInfo.FunctionCallback());
            _waveIn.BufferMilliseconds = 40;
            _waveIn.DeviceNumber = InputDevice;
            _waveIn.DataAvailable += OnWaveInDataAvailable;
            _waveIn.WaveFormat = new NAudio.Wave.WaveFormat(48000, 16, 1);
            _microphoneAudioQueue = new();

            _waveIn.StartRecording();
            _logger.LogInformation("Recording started");
        }

        public void StopEncoding()
        {

            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.DataAvailable -= OnWaveInDataAvailable;
                _waveIn.Dispose();
            }
            _waveIn = null;
            _encoder?.Dispose();
            _encoder = null;
            _logger.LogInformation("Encoding stopped");
        }

        private async Task HandleVoiceData(VoiceDataMulticastPacket voiceDataMulticast)
        {
            var channelClient = CurrentChannel?.ConnectedClients?.FirstOrDefault(x => x.Username == voiceDataMulticast.Username);
            if (channelClient == null)
                return;
            var chunk = _decoder.Decode(voiceDataMulticast.VoiceData, voiceDataMulticast.VoiceData.Length, out var length);
            chunk = chunk.Slice(0, length);
            channelClient.IsSpeaking = true;

            channelClient.LastTimeVoiceDataReceived = DateTimeOffset.Now.ToUnixTimeSeconds();

            _audioQueue.Enqueue(chunk);
            _audioQueueSlim.Release();
        }
        private async Task StartAudioStreaming()
        {

            var channelClient = CurrentChannel.ConnectedClients.FirstOrDefault(x => x.Username == Username);
            _ = _client.SendPacketTCP(new VoiceEstablishPacket());
            while (voiceAllowed == null)
                await Task.Yield();
            if (voiceAllowed == false)
                return;
            StartEncoding(CurrentChannel.Bitrate);
            IsNotConnectingToChannel = true;
            _logger.LogInformation("Audio streaming started");
        }

        private async Task SendAudio()
        {
            while (true)
            {
                try
                {
                    _microphoneQueueSlim.Wait();
                    var chunk = _microphoneAudioQueue.Dequeue();
                    await _client.SendPacketUDP(new VoiceDataPacket() { ClientUsername = Username, VoiceData = chunk });
                }
                catch (Exception)
                {

                }
            }
        }


        private long lastTimeReceivedAudio = 0;
        private int delay = 200;
        private Timer _offsetListenerTimer;

        private async Task PlayAudio()
        {

            while (true)
            {
                try
                {
                    _audioQueueSlim.Wait();
                    var chunk = _audioQueue.Dequeue();
                    if (_playBuffer == null)
                        continue;
                    if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTimeReceivedAudio > delay)
                        await Task.Delay(delay);
                    lastTimeReceivedAudio = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    _playBuffer.AddSamples(chunk, 0, chunk.Length);
                }
                catch (Exception)
                {

                }
            }
        }

        private bool ProcessData(byte[] buffer, int bytesRecorded, double? threshold = null)
        {
            if (threshold == null)
                threshold = VoiceActivityDetectionThreshold;
            bool result = false;
            bool Tr = false;
            double Sum2 = 0;
            int Count = bytesRecorded / 2;
            for (int index = 0; index < bytesRecorded; index += 2)
            {
                double Tmp = (short)((buffer[index + 1] << 8) | buffer[index + 0]);
                Tmp /= 32768.0;
                Sum2 += Tmp * Tmp;
                if (Tmp > threshold)
                    Tr = true;
            }
            Sum2 /= Count;
            if (Tr || Sum2 > threshold)
            { result = true; }
            else
            { result = false; }
            return result;
        }

        private void OnWaveInDataAvailable(object? sender, WaveInEventArgs a)
        {
            if (App.MainWindow.DispatcherQueue == null)
                return;
            if (_encoder == null)
                return;
            if (CurrentChannelClient == null)
                return;

            if (CurrentChannelClient?.IsMuted ?? false)
            {
                CurrentChannelClient.IsSpeaking = false;
                return;
            }

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (CurrentChannelClient == null)
                    return;
                CurrentChannelClient.IsSpeaking = true;
            });

            var chunks = AudioProcessor.ProcessAudio(a.Buffer, a.BytesRecorded, _bytesPerSegment, _encoder, EnableRNNoiseSuppression);
            if (chunks == null)
            {
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    CurrentChannelClient.IsSpeaking = false;
                });
            }
            foreach (var chunk in chunks)
            {
                _microphoneAudioQueue.Enqueue(chunk);
                _microphoneQueueSlim.Release();
            }

        }
        #endregion
        #region Networking
        private void Connect()
        {
            _logger.LogInformation($"Connecting to {Address}");

            if (_client?.IsConnected ?? false)
                _client.Disconnect();
            if (_cts != null)
                _cts.Cancel();
            _cts = new();
            Task.Run(async () =>
            {
                ip = address.Split(":")[0];
                if (!IPAddress.TryParse(ip, out _))
                {
                    _logger.LogInformation("IP parse failed. Using DNS to find server's IP");
                    ip = Dns.GetHostAddresses(ip)[0].ToString();

                }
                port = Convert.ToInt32(address.Split(":")[1]);
                try
                {
                    _client = new RemoreClient(ip, port, Username, null, null);
                    _client.PacketReceived += OnPacketReceived;
                    _client.UDPPacketReceived += OnUdpPacketReceived;
                    try
                    {
                        await _client.ConnectAsync();
                        App.MainWindow.DispatcherQueue.TryEnqueue(() => IsConnected = true);
                    }
                    catch (ConnectionFailedException ex)
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(() => IsConnected = false);
                        _logger.LogError($"Connection to {address} failed");
                        App.ResetMainViewModel();
                        App.GetService<INavigationService>().NavigateTo(typeof(SettingsViewModel).FullName, null, true);
                        App.GetService<INavigationService>().NavigateTo(typeof(MainViewModel).FullName, null, true);
                        return;
                    }
                    _logger.LogInformation($"Connected to {Address}");
                }
                catch (TaskCanceledException)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() => IsConnected = false);
                }
                catch (Exception ex)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() => IsConnected = false);
                }
            });

        }

        private async Task LoadMoreMessages()
        {
            var queue = App.MainWindow.DispatcherQueue;

            if (!IsMessagesNotLoading)
                return;
            if (CurrentTextChannel == null)
                return;
            var channel = CurrentTextChannel;
            if (channel.LastParsedPage == -1)
                return;
            IsMessagesNotLoading = false;
            channel.LastParsedPage++;
            var messages = await _client.RequestChannelMessages(channel.Id, channel.LastParsedPage);
            if (messages.Messages.Count == 0)
            {
                channel.LastParsedPage = -1;
            }

            if (channel.Messages == null)
                channel.Messages = new();
            foreach (var message in messages.Messages)
            {
                channel.Messages.Insert(0, message);
            }
            IsMessagesNotLoading = true;


        }

        private void OnUdpPacketReceived(object sender, IPacket e)
        {
            if (e is VoiceDataMulticastPacket voice)
                HandleVoiceData(voice);
        }

        public async Task JoinChannel(Channel channel)
        {
            try
            {
                if (CurrentChannel?.Id == channel.Id)
                    return;
                if (channel.ChannelType == Library.Enums.ChannelType.Text)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        CurrentTextChannel = channel;
                        channel.IsSelected = true;
                    });
                    if (channel.Messages == null || channel.Messages.Count == 0)
                    {
                        channel.LastParsedPage = 1;
                        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            IsMessagesNotLoading = false;
                        });
                        var messages = await _client.RequestChannelMessages(channel.Id, 1);
                        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                        {
                            if (channel.Messages == null)
                                channel.Messages = new();
                            foreach (var message in messages.Messages)
                            {
                                channel.Messages.Insert(0, message);
                            }
                            await Task.Delay(1500);
                            //Wait for messages to render
                            //Scroll to infinity (basically)
                            MessagesListBox.GetScrollViewer().ScrollToVerticalOffset(999999);
                            IsMessagesNotLoading = true;
                        });
                        if (_offsetListenerTimer != null)
                            _offsetListenerTimer.Dispose();
                        _offsetListenerTimer = new Timer(async (state) =>
                        {

                            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                            {

                                var scroll = MessagesListBox.GetScrollViewer();
                                try
                                {
                                    if (scroll.VerticalOffset < 10)
                                    {
                                        await LoadMoreMessages();
                                    }
                                }
                                catch
                                {

                                }
                            });

                        }, null, 1000, 40);
                    }
                    return;
                }
                IsNotConnectingToChannel = false;
                _channel = channel;
                var response = await _client.RequestChannelJoinAsync(channel.Id);
                if (response == null || !response.Allowed)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        await new ContentDialog()
                        {
                            Title = "Failed to connect to channel",
                            Content = new TextBlock() { Text = response.Reason },
                            CloseButtonText = "Close",
                            XamlRoot = App.MainWindow.Content.XamlRoot,
                        }.ShowAsync(ContentDialogPlacement.InPlace);
                    });
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }



        private void OnSocketErrored(object? sender, SocketError e)
        {

        }

        public void Disconnect()
        {
            try
            {
                _cts?.Cancel();
                _client.Disconnect();
                StopEncoding();
                StopAudioPlayback();
                App.ResetMainViewModel();
                App.GetService<INavigationService>().NavigateTo(typeof(SettingsViewModel).FullName, null, true);
                App.GetService<INavigationService>().NavigateTo(typeof(MainViewModel).FullName, null, true);
            }
            catch
            {
                ;
            }


        }

        private void OnPacketReceived(object? sender, IPacket e)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                var packet = e;

                if (packet is ClientConnectedPacket client)
                {

                }
                else if (packet is DisconnectPacket disconnect)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        var address = $"{ip}:{port}";
                        Disconnect();
                        if (disconnect.Reason == "Invalid privilege key")
                        {
                            await SettingsService.SaveSettingAsync<string>($"{address}PrivilegeKey", "");
                            var newViewModel = App.GetService<MainViewModel>();
                            newViewModel.Address = Address;
                            newViewModel.Connect();
                            return;
                        }
                        await new ContentDialog()
                        {
                            Title = "You were disconnected from the server",
                            Content = $"Reason: {disconnect.Reason}",
                            XamlRoot = App.MainWindow.Content.XamlRoot,
                            CloseButtonText = "Close"
                        }.ShowAsync(ContentDialogPlacement.InPlace);

                    });
                }
                else if (packet is ChannelAddedPacket addedChannel)
                {
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        Channels.Add(new Channel()
                        {
                            Id = addedChannel.ChannelId,
                            Name = addedChannel.Name,
                            Bitrate = addedChannel.Bitrate,
                            ConnectedClients = new(addedChannel.Clients.Select(x => new ChannelClient(x)).ToList()),
                            Parent = this,
                            ClientsCount = addedChannel.Clients.Count,
                            MaxClients = addedChannel.MaxClients,
                            Order = addedChannel.Order,
                            ChannelType = addedChannel.ChannelType
                        });
                    });


                }
                else if (packet is ChannelMessageAddedPacket channelMessage)
                {
                    var channel = Channels.FirstOrDefault(x => x.Id == channelMessage.ChannelId);
                    if (channel == null)
                        return;
                    App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                    {
                        if (channel.Messages == null)
                            channel.Messages = new();
                        channel.Messages.Add(new()
                        {
                            ChannelId = channelMessage.ChannelId,
                            Id = channelMessage.MessageId,
                            Message = channelMessage.Text,
                            Username = channelMessage.SenderName
                        });
                        if (channelMessage.SenderName == Username)
                        {
                            await Task.Delay(100);
                            var scrollViewer = MessagesListBox.GetScrollViewer();
                            scrollViewer.ScrollToVerticalOffset(9999999);
                        }
                    });
                }
                else if (packet is ChannelUserConnected userConnected)
                {

                    var channel = Channels.FirstOrDefault(x => x.Id == userConnected.ChannelId);
                    if (channel == null)
                        return;
                    var chClient = new ChannelClient(userConnected.Username);
                    channel.ConnectedClients.Add(chClient);
                    channel.ClientsCount++;
                    if (userConnected.Username == Username)
                    {
                        CurrentChannelClient = chClient;
                        CurrentChannel = channel;
                        CurrentChannel.IsSelected = true;
                        IsNotConnectingToChannel = false;
                        Task.Run(() => StartAudioStreaming());
                    }
                }
                else if (packet is ChannelUserDisconnected userDisconnected)
                {

                    var channel = Channels.FirstOrDefault(x => x.Id == userDisconnected.ChannelId);
                    if (channel == null)
                        return;
                    channel.ClientsCount--;
                    if (userDisconnected.Username == Username)
                    {
                        StopEncoding();
                        CurrentChannel.IsSelected = false;
                        CurrentChannel = null;
                        CurrentChannelClient = null;
                    }
                    var channelClient = channel.ConnectedClients.FirstOrDefault(x => x.Username == userDisconnected.Username);
                    channel.ConnectedClients.Remove(channelClient);
                }
                else if (packet is ServerInfoUpdatedPacket infoUpdate)
                {
                    ServerName = infoUpdate.Name;
                    ServerMaxClients = infoUpdate.MaxClients;
                }
                else if (packet is VoiceEstablishResponsePacket voiceEstablishResponse)
                {
                    voiceAllowed = voiceEstablishResponse.Allowed;
                }
                else if (packet is ClientPermissionsUpdatedPacket permissions)
                {
                    if (permissions.HasAllPermissions)
                    {
                        CanEditServerSettings = true;
                    }
                    else
                    {
                        //TODO: Handle permissions
                    }
                }
                else if (packet is ServerToClientNegotatiationFinished)
                {
                    //Make it look like we're doing something important xD
                    await Task.Delay(1000);
                    IsNegotiationFinished = true;
                }
                else if (packet is ChannelDeletedPacket deletedChannel)
                {
                    var channel = Channels.FirstOrDefault(x => x.Id == deletedChannel.ChannelId);
                    if (channel == null)
                        return;
                    if (channel.Id == currentTextChannel?.Id)
                    {
                        currentTextChannel.Messages.Clear();
                        CurrentTextChannel = null;
                    }
                    Channels.Remove(channel);
                }
            });
        }
        #endregion



        public void SendMessage(object param)
        {
            var message = param.ToString();
            if (string.IsNullOrEmpty(message))
                return;
            if (CurrentTextChannel != null)
            {
                _ = _client.SendPacketTCP(new CreateChannelMessagePacket()
                {
                    ChannelId = CurrentTextChannel.Id,
                    Text = message
                });
            }
        }
        private void OnExited(object? sender, EventArgs e)
        {
            _cts?.Cancel();
            _client.Disconnect();
        }


        //Ugly code, don't read
        private Action CreateConnectDialog(ILocalSettingsService settingsService)
        {
            return async () =>
            {
                if (string.IsNullOrEmpty(Username) || Username.Length < 3)
                {
                    await new ContentDialog()
                    {
                        Title = "Main_ConnectToServer_InvalidNicknameTitle".GetLocalized(),
                        Content = "Main_ConnectToServer_InvalidNicknameContent".GetLocalized(),
                        XamlRoot = App.MainWindow.Content.XamlRoot,
                        CloseButtonText = "Main_ConnectToServer_CloseButton".GetLocalized(),
                    }.ShowAsync(ContentDialogPlacement.InPlace);
                    return;
                }
                var parentStack = new StackPanel();
                var stack = new StackPanel()
                {
                    Padding = new(12)
                };
                stack.Children.Add(new TextBlock() { Text = "Main_ConnectToServer_Description".GetLocalized(), TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
                var textBox = new TextBox() { PlaceholderText = "Main_ConnectToServer_AddressInputPlaceholder".GetLocalized(), Name = "AddressInput", Margin = new(0, 12, 0, 0) };
                stack.Children.Add(textBox);
                var addToFavorites = new CheckBox() { Content = new TextBlock() { Text = "Main_ConnectToServer_AddServerToFavoritesAfterConnect".GetLocalized() } };
                stack.Children.Add(addToFavorites);
                var tabView = new TabView()
                {
                    IsAddTabButtonVisible = false,
                    CloseButtonOverlayMode = TabViewCloseButtonOverlayMode.OnPointerOver,
                };

                var connectToServerViaIpItem = new TabViewItem()
                {
                    Header = "Main_ConnectToServer_ConnectWithAddress".GetLocalized(),
                    Content = stack,
                    IsClosable = false
                };
                var connectToServerViaFavoritesStack = new StackPanel()
                {
                    Padding = new(12)
                };
                var list = await SettingsService.ReadSettingAsync<List<string>>(SettingsViewModel.FavoritesSettingsKey);
                if (list == null)
                    list = new();
                var listView = new ListView() { SelectionMode = ListViewSelectionMode.None, MaxHeight = 560 };
                foreach (var address in list)
                {
                    string _ip;
                    int _port;
                    if (address.Contains(':'))
                    {
                        _ip = address.Split(":")[0];
                        _port = Convert.ToInt32(address.Split(":")[1]);
                    }
                    else
                    {
                        (_ip, _port) = GetEndpointForHostname(address);
                    }
                    var holder = new Grid();
                    holder.ColumnDefinitions.Add(new() { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
                    holder.ColumnDefinitions.Add(new() { Width = Microsoft.UI.Xaml.GridLength.Auto });
                    var progress = new ProgressRing() { IsIndeterminate = true, Width = 16, Height = 16, IsActive = true };
                    var _stackPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center
                    };
                    var textBlock = new TextBlock()
                    {
                        Margin = new(12, 0, 0, 0),
                        Text = string.Format("Main_ConnectToServerFavorites_ConnectingToServer".GetLocalized(), address)
                    };
                    var removeButton = new Button()
                    {
                        Content = new FluentSymbolIcon(FluentSymbol.Delete16)
                        {
                            Width = 16,
                            Height = 16
                        },
                        Margin = new(12, 0, 0, 0)
                    };
                    removeButton.Click += async (s, e) =>
                    {
                        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            listView.Items.Remove(holder); 
                        });
                        list.Remove(address);
                        await settingsService.SaveSettingAsync(SettingsViewModel.FavoritesSettingsKey, list);

                    };
                    _stackPanel.Children.Add(progress);
                    _stackPanel.Children.Add(textBlock);
                    var button = new Button()
                    {
                        Content = _stackPanel,
                        HorizontalContentAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                        IsEnabled = false
                    };
                    button.Click += (s, e) =>
                    {
                        Address = address;
                        Connect();
                        (parentStack.Parent as ContentDialog).Hide();
                    };
                    Grid.SetColumn(button, 0);
                    Grid.SetColumn(removeButton, 1);
                    holder.Children.Add(button);
                    holder.Children.Add(removeButton);

                    _ = Task.Run(async () =>
                    {
                        var query = await new RemoreQueryClient(_ip, _port).GetServerInfo();
                        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            progress.IsActive = false;
                            progress.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                            if (query == null)
                            {
                                textBlock.Text = string.Format($"Main_ConnectToServerFavorites_FailedToConnect".GetLocalized(), address);
                                return;
                            }
                            if (query.ServerVersion == SettingsViewModel.ClientVersion)
                            {
                                textBlock.Text = $"{query.ServerName} - {query.ServerVersion} ({query.ClientsConnected}/{query.MaxClients})";
                                button.IsEnabled = true;
                            }
                            else
                            {
                                textBlock.TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center;
                                textBlock.Text = string.Format("Main_ConnectToServer_VersionDontMatch".GetLocalized(), query.ServerName, SettingsViewModel.ClientVersion, query.ServerVersion);
                            }
                        });
                    });
                    listView.Items.Add(holder);
                }
                connectToServerViaFavoritesStack.Children.Add(listView);
                var connectToServerViaFavorites = new TabViewItem()
                {
                    Header = "Main_ConnectToServer_ConnectFromFavoritesList".GetLocalized(),
                    Content = connectToServerViaFavoritesStack,
                    IsClosable = false
                };
                tabView.TabItems.Add(connectToServerViaIpItem);
                tabView.TabItems.Add(connectToServerViaFavorites);
                parentStack.Children.Add(tabView);
                var result = await new ContentDialog()
                {
                    Title = "Main_ConnectToServer_Title".GetLocalized(),
                    Content = parentStack,
                    XamlRoot = App.MainWindow.Content.XamlRoot,
                    CloseButtonText = "Main_ConnectToServer_CloseButton".GetLocalized(),
                    PrimaryButtonText = "Main_ConnectToServer_ConnectButton".GetLocalized(),
                }.ShowAsync(ContentDialogPlacement.InPlace);
                if (textBox.Text.Contains(':') || string.IsNullOrEmpty(textBox.Text))
                    Address = textBox.Text;
                else
                {
                    var ep = GetEndpointForHostname(textBox.Text);
                    Address = ep.Address + ':' + ep.Port.ToString();
                }
                if (result == ContentDialogResult.Primary)
                {
                    Connect();
                    if (addToFavorites.IsChecked ?? false)
                    {
                        var addresses = await SettingsService.ReadSettingAsync<List<string>>(SettingsViewModel.FavoritesSettingsKey);
                        if (addresses == null)
                            addresses = new();
                        if (!addresses.Contains(textBox.Text))
                            addresses.Add(textBox.Text);
                        await settingsService.SaveSettingAsync(SettingsViewModel.FavoritesSettingsKey, addresses);
                    }
                }
            };
        }

        private (string Address, int Port) GetEndpointForHostname(string hostname)
        {
            var result = LookupClient.ResolveService(hostname, "Remore", ProtocolType.Tcp);
            var entry = result.FirstOrDefault();
            if (entry is null)
                return new("0.0.0.0", 0);

            if (entry.AddressList.Any())
                return new(entry.AddressList.First().ToString(), entry.Port);

            var entryAddressAnswers = LookupClient.Query(entry.HostName, QueryType.A).Answers;
            if (entryAddressAnswers.Any())
            {
                var aTarget = entryAddressAnswers.ARecords().First();
                return new(aTarget.Address.ToString(), entry.Port);
            }

            entryAddressAnswers = LookupClient.Query(entry.HostName, QueryType.AAAA).Answers;
            if (entryAddressAnswers.Any())
            {
                var aaaaTarget = entryAddressAnswers.AaaaRecords().First();
                return new(aaaaTarget.Address.ToString(), entry.Port);
            }

            return new("0.0.0.0", 0);
        }
    }
}
