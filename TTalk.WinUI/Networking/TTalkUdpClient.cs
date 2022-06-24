using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTalk.WinUI.Networking.EventArgs;
using TTalk.WinUI.Models;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;
using TTalk.WinUI.Contracts.Services;
using TTalk.WinUI.ViewModels;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;

namespace TTalk.WinUI.Networking.ClientCode
{
    public class TTalkUdpClient : UdpClient
    {
        public TTalkUdpClient(string tcpId, IPAddress address, int port) : base(address, port)
        {
            TcpId = tcpId;
            _settingsService = App.GetService<ILocalSettingsService>();
            _username = _settingsService.ReadSettingAsync<string>(SettingsViewModel.UsernameSettingsKey).GetAwaiter().GetResult();
            _logger = App.GetService<ILogger<TTalkUdpClient>>();
        }
        public string TcpId { get;}

        private ILocalSettingsService _settingsService;
        private string _username;
        private ILogger<TTalkUdpClient> _logger;

        public EndPoint LastSenderEndpoint { get; set; }
        public bool IsConnectedToServer { get; set; }

        public event EventHandler<VoiceDataMulticastPacket> VoiceDataAvailable;

        public void DisconnectAndStop()
        {
            _stop = true;
            Disconnect();
            while (IsConnected)
                Thread.Yield();
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
        }

        protected override void OnConnected()
        {
            this.Send(new UdpAuthenticationPacket()
            {
                ClientUsername = _username,
                TcpId = TcpId
            });
            ReceiveAsync();
        }

        protected override void OnSent(EndPoint endpoint, long sent)
        {
            
        }
        protected override void OnDisconnected()
        {
            Thread.Sleep(1000);
            _logger.LogInformation(_stop ? $"Udp disconnected" : "UDP connection lost");
            
            if (!_stop)
            {
                _logger.LogInformation("Trying to restore UDP connection");
                Connect();
            }
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            var packet = IPacket.FromByteArray(buffer, out var ex);
            if (ex != null)
                App.GetService<ILogger<TTalkUdpClient>>().LogError($"Failed to read packet with ID {BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(4, 4))}:\n" + ex.ToString());
            if (packet is UdpNotifyConnectedPacket)
                IsConnectedToServer = true;
            else if (packet is UdpHeartbeatPacket)
            {
                this.Send(new UdpHeartbeatPacket() {  ClientUsername = _username });
            }
            else if (packet is VoiceDataMulticastPacket multicast)
            {
                VoiceDataAvailable?.Invoke(this, multicast);
            }

            ReceiveAsync();
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            Console.WriteLine($"UDP client caught an error with code {error}");
        }

        private bool _stop;

    }

}
