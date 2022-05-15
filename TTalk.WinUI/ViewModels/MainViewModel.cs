using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FragLabs.Audio.Codecs;
using Microsoft.UI.Xaml.Controls;
using NAudio.Wave;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;
using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.Helpers;
using TTalk.WinUI.Models;
using TTalk.WinUI.Networking;
using TTalk.WinUI.Networking.ClientCode;
using TTalk.WinUI.Networking.EventArgs;
using TTalk.WinUI.Services;
using Windows.ApplicationModel.Resources.Core;

namespace TTalk.WinUI.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        public MainViewModel(ILocalSettingsService settingsService, SoundService sounds)
        {
            Process.GetCurrentProcess().Exited += OnExited;
            Channels = new();

            _segmentFrames = 960;
            _microphoneQueueSlim = new(0);
            _audioQueueSlim = new(0);
            _audioQueue = new();
            SettingsService = settingsService;
            Sounds = sounds;
            SettingsService.SettingsUpdated += OnSettingsUpdated;

            Task.Run(SendAudio);
            Task.Run(PlayAudio);
            ShowConnectDialog = new RelayCommand(CreateConnectDialog(settingsService));
            DisconnectCommand = new RelayCommand(() =>
            {
                Disconnect();
            });
            LeaveChannel = new RelayCommand(() =>
            {
                this._client.Send(new LeaveChannelPacket());
            });
            ToggleMute = new RelayCommand(async () =>
            {
                if (CurrentChannelClient != null)
                {
                    CurrentChannelClient.IsMuted = !CurrentChannelClient.IsMuted;
                }
            });
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
            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                await ReadSettings();
                StartAudioPlayback();
            });
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
        }

        private TTalkClient _client;
        private CancellationTokenSource _cts;
        private bool? voiceAllowed = null;

        #region Reactive Properties
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
        public RelayCommand ShowUpdatePriviligeKeyDialog { get; }
        public ILocalSettingsService SettingsService { get; }
        public SoundService Sounds { get; }
        public string Username { get; private set; }
        public bool UseVoiceActivityDetection { get; private set; }
        public double VoiceActivityDetectionThreshold { get; private set; }
        public int InputDevice { get; private set; }
        public int OutputDevice { get; private set; }

        private Channel _channel;


        #endregion
        #region Audio 
        private WaveIn _waveIn;
        private WaveOut _waveOut;
        private BufferedWaveProvider _playBuffer;
        private OpusEncoder _encoder;
        private OpusDecoder _decoder;
        private int _segmentFrames;
        private int _bytesPerSegment;
        private byte[] _notEncodedBuffer = new byte[0];
        private TTalkUdpClient _udpClient;

        private Queue<byte[]> _microphoneAudioQueue;
        private Queue<byte[]> _audioQueue;
        private SemaphoreSlim _microphoneQueueSlim;
        private SemaphoreSlim _audioQueueSlim;

        public void StartAudioPlayback()
        {

            _decoder = OpusDecoder.Create(48000, 1);

            _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _waveOut.PlaybackStopped += OnWaveOutPlaybackStopped;
            _waveOut.DeviceNumber = OutputDevice;
            _playBuffer = new BufferedWaveProvider(new NAudio.Wave.WaveFormat(48000, 16, 1));
            _waveOut.Init(_playBuffer);
            _waveOut.Play();
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
        }

        public void StartEncoding(int bitRate)
        {

            _encoder = OpusEncoder.Create(48000, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
            _encoder.Bitrate = bitRate;
            _bytesPerSegment = _encoder.FrameByteCount(_segmentFrames);

            _waveIn = new WaveIn(WaveCallbackInfo.FunctionCallback());
            _waveIn.BufferMilliseconds = 50;
            _waveIn.DeviceNumber = InputDevice;
            _waveIn.DataAvailable += OnWaveInDataAvailable;
            _waveIn.WaveFormat = new NAudio.Wave.WaveFormat(48000, 16, 1);
            _microphoneAudioQueue = new();

            _waveIn.StartRecording();
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
        }

        private async Task HandleVoiceData(VoiceDataMulticastPacket voiceDataMulticast)
        {
            var channelClient = CurrentChannel?.ConnectedClients?.FirstOrDefault(x => x.Username == voiceDataMulticast.Username);
            if (channelClient == null)
                return;
            channelClient.IsSpeaking = true;
            channelClient.LastTimeVoiceDataReceived = DateTimeOffset.Now.ToUnixTimeSeconds();

            _audioQueue.Enqueue(voiceDataMulticast.VoiceData);
            _audioQueueSlim.Release();
        }
        private async Task StartAudioStreaming()
        {

            var channelClient = CurrentChannel.ConnectedClients.FirstOrDefault(x => x.Username == Username);
            _client.Send(new VoiceEstablishPacket());
            while (voiceAllowed == null)
                await Task.Yield();
            if (voiceAllowed == false)
                return;
            StartEncoding(CurrentChannel.Bitrate);
            IsNotConnectingToChannel = true;
        }

        private async Task SendAudio()
        {
            while (true)
            {
                try
                {
                    _microphoneQueueSlim.Wait();
                    var chunk = _microphoneAudioQueue.Dequeue();
                    _udpClient.Send(new VoiceDataPacket() { ClientUsername = Username, VoiceData = chunk });
                }
                catch (Exception)
                {

                }
            }
        }
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
                    _playBuffer.AddSamples(chunk, 0, chunk.Length);
                }
                catch (Exception)
                {

                }
            }
        }

        private bool ProcessData(WaveInEventArgs e)
        {
            double threshold = VoiceActivityDetectionThreshold;
            bool result = false;
            bool Tr = false;
            double Sum2 = 0;
            int Count = e.BytesRecorded / 2;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                double Tmp = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
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


        private void OnWaveInDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (CurrentChannelClient?.IsMuted ?? false)
            {
                CurrentChannelClient.IsSpeaking = false;
                return;
            }

            if (UseVoiceActivityDetection && !ProcessData(e))
            {
                if (CurrentChannelClient != null)
                    CurrentChannelClient.IsSpeaking = false;
                return;
            }
            try
            {
                if (App.MainWindow.DispatcherQueue == null)
                    return;
                App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
                {
                    if (_encoder == null)
                        return;

                    byte[] soundBuffer = new byte[e.BytesRecorded + _notEncodedBuffer.Length];
                    for (int i = 0; i < _notEncodedBuffer.Length; i++)
                        soundBuffer[i] = _notEncodedBuffer[i];
                    for (int i = 0; i < e.BytesRecorded; i++)
                        soundBuffer[i + _notEncodedBuffer.Length] = e.Buffer[i];

                    int byteCap = _bytesPerSegment;
                    int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
                    int segmentsEnd = segmentCount * byteCap;
                    int notEncodedCount = soundBuffer.Length - segmentsEnd;
                    _notEncodedBuffer = new byte[notEncodedCount];
                    for (int i = 0; i < notEncodedCount; i++)
                    {
                        _notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
                    }

                    for (int i = 0; i < segmentCount; i++)
                    {
                        byte[] segment = new byte[byteCap];
                        for (int j = 0; j < segment.Length; j++)
                            segment[j] = soundBuffer[(i * byteCap) + j];
                        int len;
                        byte[] buff = _encoder.Encode(segment, segment.Length, out len);
                        buff = _decoder.Decode(buff, len, out len);
                        _microphoneAudioQueue.Enqueue(buff.Slice(0, len));
                        _microphoneQueueSlim.Release();
                        if (CurrentChannelClient != null)
                            CurrentChannelClient.IsSpeaking = true;
                    }
                });
            }
            catch (Exception)
            {

            }
        }
        #endregion
        #region Networking
        private void Connect()
        {
            if (_client?.IsConnected ?? false)
                _client.DisconnectAndStop();
            if (_cts != null)
                _cts.Cancel();
            _cts = new();
            Task.Run(async () =>
            {
                ip = address.Split(":")[0];
                port = Convert.ToInt32(address.Split(":")[1]);
                try
                {
                    _client = new TTalkClient(ip, port);
                    _client.SocketErrored += OnSocketErrored;
                    _client.PacketReceived += OnPacketReceived;
                    var success = _client.ConnectAsync();
                    while (_client.IsConnecting || _client.TcpId == null)
                        await Task.Yield();
                    App.MainWindow.DispatcherQueue.TryEnqueue(() => IsConnected = true);
                    UdpConnect(ip, port);
                    while (!_udpClient?.IsConnectedToServer ?? false)
                        await Task.Delay(100);
                    await Task.Delay(-1, _cts.Token);
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
        private void UdpConnect(string ip, int port)
        {
            _udpClient = new TTalkUdpClient(_client.TcpId, IPAddress.Parse(ip), port);
            _udpClient.VoiceDataAvailable += OnVoiceDataAvailable;
            _udpClient.Connect();
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
                    if (channel.LastParsedPage != 0)
                    {
                        channel.LastParsedPage++;
                        _client.Send(new RequestChannelMessagesPacket()
                        {
                            ChannelId = channel.Id,
                            Page = 0
                        });
                    }
                    return;
                }
                IsNotConnectingToChannel = false;
                _channel = channel;
                _client.Send(new RequestChannelJoin() { ChannelId = channel.Id });
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
                _client.DisconnectAndStop();
                UdpDisconnect();
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

        private void OnPacketReceived(object? sender, PacketReceivedEventArgs e)
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                var packet = e.Packet;

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
                            Title = "You was disconnected from the server",
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
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
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

                            //MainWindow.ListBox.Scroll.Offset = new Vector(MainWindow.ListBox.Scroll.Offset.X, double.MaxValue);
                        }
                    });
                }
                else if (packet is ChannelMessagesResponse channelMessages)
                {
                    var channel = Channels.FirstOrDefault(x => x.Id == channelMessages.ChannelId);
                    if (channel == null)
                        return;
                    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        if (channel.Messages == null)
                            channel.Messages = new();
                        channelMessages.Messages.Reverse();
                        foreach (var message in channelMessages.Messages)
                        {
                            channel.Messages.Insert(0, message);
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
                else if (packet is VoiceEstablishResponsePacket voiceEstablishResponse)
                {
                    voiceAllowed = voiceEstablishResponse.Allowed;
                }
                else if (packet is RequestChannelJoinResponse response)
                {
                    if (!response.Allowed)
                    {
                        if (response.Reason.StartsWith("Your client isn't connected"))
                        {
                            UdpDisconnect();
                            UdpConnect(ip, port);
                            while (!_udpClient.IsConnectedToServer)
                            {

                            }
                            _client.Send(new RequestChannelJoin() { ChannelId = _channel.Id });
                            return;
                        }
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
            });
        }
        private void OnVoiceDataAvailable(object? sender, VoiceDataMulticastPacket e)
        {
            HandleVoiceData(e);
        }

        public void UdpDisconnect()
        {
            if (_udpClient != null && _udpClient.IsConnected)
            {
                _udpClient.Send(new UdpDisconnectPacket() { ClientUsername = Username });
                _udpClient.DisconnectAndStop();
                _udpClient = null;
            }
        }
        #endregion



        public void SendMessage(object param)
        {
            var message = param.ToString();
            if (string.IsNullOrEmpty(message))
                return;
            if (CurrentTextChannel != null)
            {
                _client.Send(new CreateChannelMessagePacket()
                {
                    ChannelId = CurrentTextChannel.Id,
                    Text = message
                });
            }
        }
        private void OnExited(object? sender, EventArgs e)
        {
            _cts?.Cancel();
            _client.DisconnectAndStop();
            UdpDisconnect();
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
                    var _ip = address.Split(":")[0];
                    var _port = Convert.ToInt32(address.Split(":")[1]);
                    var progress = new ProgressRing() { IsIndeterminate = true, Width = 16, Height = 16, IsActive = true };
                    var _stackPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    var textBlock = new TextBlock()
                    {
                        Margin = new(12, 0, 0, 0),
                        Text = string.Format("Main_ConnectToServerFavorites_ConnectingToServer".GetLocalized(), address)
                    };
                    _stackPanel.Children.Add(progress);
                    _stackPanel.Children.Add(textBlock);
                    var button = new Button()
                    {
                        Content = _stackPanel,
                        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                        IsEnabled = false
                    };
                    button.Click += (s, e) =>
                    {
                        Address = address;
                        Connect();
                        (parentStack.Parent as ContentDialog).Hide();
                    };
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1500);
                        var query = await new TTalkQueryClient(_ip, _port).GetServerInfo();
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
                    listView.Items.Add(button);
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
                Address = textBox.Text;
                if (result == ContentDialogResult.Primary)
                {
                    Connect();
                    if (addToFavorites.IsChecked ?? false)
                    {
                        var addresses = await SettingsService.ReadSettingAsync<List<string>>(SettingsViewModel.FavoritesSettingsKey);
                        if (addresses == null)
                            addresses = new();
                        if (!addresses.Contains(address))
                            addresses.Add(address);
                        await settingsService.SaveSettingAsync(SettingsViewModel.FavoritesSettingsKey, addresses);
                    }
                }
            };
        }
    }
}
