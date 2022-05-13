using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FragLabs.Audio.Codecs;
using NAudio.Wave;
using NWaves.Audio;
using NWaves.Filters;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TTalk.Client.ClientCode;
using TTalk.Client.ClientCode.EventArgs;
using TTalk.Client.Models;
using TTalk.Client.Views;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;

namespace TTalk.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Process.GetCurrentProcess().Exited += OnExited;
            Channels = new();

            _segmentFrames = 960;
            _microphoneQueueSlim = new(0);
            _audioQueueSlim = new(0);
            _audioQueue = new();


            _filter = new WienerFilter(13, 0.1);
            SettingsModel.Init();
            SettingsModel.I.Main = this;
            Task.Run(SendAudio);
            Task.Run(PlayAudio);
            StartAudioPlayback();
        }

        private TTalkClient _client;
        private CancellationTokenSource _cts;
        private bool? voiceAllowed = null;

        public SettingsModel Settings => SettingsModel.I;
        #region Reactive Properties
        private string address;

        public string Address
        {
            get { return address; }
            set { this.RaiseAndSetIfChanged(ref address, value); }
        }

        private string messageContent;

        public string MessageContent
        {
            get { return messageContent; }
            set { this.RaiseAndSetIfChanged(ref messageContent, value); }
        }

        private ObservableCollection<Channel> channels;

        public ObservableCollection<Channel> Channels
        {
            get { return channels; }
            set { this.RaiseAndSetIfChanged(ref channels, value); }
        }

        private bool isConnected;
        private string ip;
        private int port;

        public bool IsConnected
        {
            get { return isConnected; }
            set { this.RaiseAndSetIfChanged(ref isConnected, value); }
        }

        private Channel currentTextChannel;

        public Channel CurrentTextChannel
        {
            get { return currentTextChannel; }
            set { this.RaiseAndSetIfChanged(ref currentTextChannel, value); }
        }


        public ChannelClient CurrentChannelClient { get; private set; }

        public Channel CurrentChannel { get; set; }
        private bool isNotConnectingToChannel;

        public bool IsNotConnectingToChannel
        {
            get { return isNotConnectingToChannel; }
            set { this.RaiseAndSetIfChanged(ref isNotConnectingToChannel, value); }
        }

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
        private WienerFilter _filter;
        private SemaphoreSlim _microphoneQueueSlim;
        private SemaphoreSlim _audioQueueSlim;

        public void StartAudioPlayback()
        {

            _decoder = OpusDecoder.Create(48000, 1);

            _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _waveOut.PlaybackStopped += OnWaveOutPlaybackStopped;
            _waveOut.DeviceNumber = SettingsModel.I.OutputDevice;
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
            _waveOut.Stop();
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
            _waveIn.DeviceNumber = SettingsModel.I.InputDevice;
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

            var channelClient = CurrentChannel.ConnectedClients.FirstOrDefault(x => x.Username == Settings.Username);
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
                    _playBuffer.AddSamples(chunk, 0, chunk.Length);
                    //_udpClient.Send(new VoiceDataPacket() { ClientUsername = Settings.Username, VoiceData = chunk });
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
            double threshold = Settings.Threshold;
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

        private void ToggleMute()
        {
            if (CurrentChannelClient != null)
                CurrentChannelClient.IsMuted = !CurrentChannelClient.IsMuted;
        }

        private void OnWaveInDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (CurrentChannelClient?.IsMuted ?? false)
                return;

            if (Settings.UseVoiceActivityDetection && !ProcessData(e))
            {
                if (CurrentChannelClient != null)
                    CurrentChannelClient.IsSpeaking = false;
                return;
            }
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (_encoder == null)
                    return;


                if (Settings.NoiseReductionEnabled)
                {
                    var sizeInBytes = e.BytesRecorded;
                    var sizeInFloats = sizeInBytes / sizeof(short);  // for Pcm 16bit

                    byte[] _bytes = e.Buffer;
                    float[][] _data = new float[1][];
                    for (var i = 0; i < 1; i++)
                        _data[i] = new float[sizeInFloats];

                    ByteConverter.ToFloats16Bit(_bytes, _data);
                    var data = _filter.ApplyTo(new NWaves.Signals.DiscreteSignal(48000, _data[0]));
                    var buffer = new byte[data.Length * sizeof(short)];
                    ByteConverter.FromFloats16Bit(new float[][] { data.Samples }, buffer);
                    e = new WaveInEventArgs(buffer, buffer.Length);
                }
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
                IsConnected = true;
                try
                {
                    _client = new TTalkClient(ip, port);
                    _client.SocketErrored += OnSocketErrored;
                    _client.PacketReceived += OnPacketReceived;
                    var success = _client.ConnectAsync();
                    while (_client.IsConnecting || _client.TcpId == null)
                        await Task.Yield();
                    UdpConnect(ip, port);
                    while (!_udpClient.IsConnectedToServer)
                        await Task.Delay(100);
                    await Task.Delay(-1, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    ;
                }
                catch (Exception ex)
                {
                    ;
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

            if (CurrentChannel?.Id == channel.Id)
                return;
            if (channel.ÑhannelType == Library.Enums.ChannelType.Text)
            {
                CurrentTextChannel = channel;
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
            }
            catch
            {
                ;
            }

            (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                .MainWindow.DataContext = new MainWindowViewModel();
        }

        private void OnPacketReceived(object? sender, PacketReceivedEventArgs e)
        {
            var packet = e.Packet;
            if (packet is ClientConnectedPacket client)
            {

            }
            else if (packet is DisconnectPacket disconnect)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await MainWindow.ShowDialogHost(new NotificationDialogModel("You was disconnected from this server\nReason:" + disconnect.Reason), "NotificationDialogHost");
                    Disconnect();
                });
            }
            else if (packet is ChannelAddedPacket addedChannel)
            {
                _ = Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Channels.Add(new Channel()
                    {
                        Id = addedChannel.ChannelId,
                        Name = addedChannel.Name,
                        Bitrate = addedChannel.Bitrate,
                        ConnectedClients = new(addedChannel.Clients.Select(x => new ChannelClient(x)).ToList()),
                        Parent = this,
                        Order = addedChannel.Order,
                        ÑhannelType = addedChannel.ChannelType
                    });
                });

            }
            else if (packet is ChannelMessageAddedPacket channelMessage)
            {
                var channel = Channels.FirstOrDefault(x => x.Id == channelMessage.ChannelId);
                if (channel == null)
                    return;
                if (channel.Messages == null)
                    channel.Messages = new();
                channel.Messages.Add(new()
                {
                    ChannelId = channelMessage.ChannelId,
                    Id = channelMessage.MessageId,
                    Message = channelMessage.Text,
                    Username = channelMessage.SenderName
                });
                if (channelMessage.SenderName == Settings.Username)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        MainWindow.ListBox.Scroll.Offset = new Vector(MainWindow.ListBox.Scroll.Offset.X, double.MaxValue);
                    });
                }
            }
            else if (packet is ChannelMessagesResponse channelMessages)
            {
                var channel = Channels.FirstOrDefault(x => x.Id == channelMessages.ChannelId);
                if (channel == null)
                    return;
                if (channel.Messages == null)
                    channel.Messages = new();
                channelMessages.Messages.Reverse();
                foreach (var message in channelMessages.Messages)
                {
                    channel.Messages.Insert(0, message);
                }
            }
            else if (packet is ChannelUserConnected userConnected)
            {

                var channel = Channels.FirstOrDefault(x => x.Id == userConnected.ChannelId);
                if (channel == null)
                    return;
                var chClient = new ChannelClient(userConnected.Username);
                channel.ConnectedClients.Add(chClient);
                if (userConnected.Username == Settings.Username)
                {
                    CurrentChannelClient = chClient;
                    CurrentChannel = channel;
                    IsNotConnectingToChannel = false;
                    Task.Run(() => StartAudioStreaming());
                }
            }
            else if (packet is ChannelUserDisconnected userDisconnected)
            {

                var channel = Channels.FirstOrDefault(x => x.Id == userDisconnected.ChannelId);
                if (channel == null)
                    return;
                if (userDisconnected.Username == Settings.Username)
                {
                    StopEncoding();
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
                    }
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await MainWindow.ShowDialogHost(new NotificationDialogModel(response.Reason), "NotificationDialogHost");
                    });
                }
            }
        }
        private void OnVoiceDataAvailable(object? sender, VoiceDataMulticastPacket e)
        {
            HandleVoiceData(e);
        }

        public void UdpDisconnect()
        {
            if (_udpClient != null && _udpClient.IsConnected)
            {
                _udpClient.Send(new UdpDisconnectPacket() { ClientUsername = Settings.Username });
                _udpClient.DisconnectAndStop();
                _udpClient = null;
            }
        }
        #endregion
        #region UI 
        public async void ShowSettingsDialog()
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var amogus = await MainWindow.ShowDialogHost(SettingsModel.I, "SettingsDialogHost");
                ;
            });
        }
        public async void ShowConnectDialog()
        {
            if (string.IsNullOrEmpty(Settings.Username))
            {
                await MainWindow.ShowDialogHost(new NotificationDialogModel("Please set valid nickname in settings"), "NotificationDialogHost");
                return;
            }
            var address = await MainWindow.ShowDialogHost(new ConnectDialogModel(), "ConnectDialogHost");
            if (string.IsNullOrEmpty(address?.ToString()))
                return;
            Address = address.ToString();
            Connect();
        }
        #endregion

        public void SendMessage(object param)
        {
            var message = MessageContent;
            if (string.IsNullOrEmpty(message))
                return;
            if (CurrentTextChannel != null)
            {

                _client.Send(new CreateChannelMessagePacket()
                {
                    ChannelId = CurrentTextChannel.Id,
                    Text = message
                });
                MessageContent = "";
            }
        }
        private void OnExited(object? sender, EventArgs e)
        {
            _cts?.Cancel();
            _client.DisconnectAndStop();
            UdpDisconnect();
        }
    }
}
