using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTalk.Client.Core;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;


namespace TTalk.Client.Core
{
    public class TTalkUdpClient : UdpClient
    {
        public TTalkUdpClient(string tcpId, string username, IPAddress address, int port) : base(address, port)
        {
            TcpId = tcpId;
            Username = username;
        }
        public string TcpId { get;}
        public string Username { get; }
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
                ClientUsername = Username,
                TcpId = TcpId
            });
            ReceiveAsync();
        }

        protected override void OnSent(EndPoint endpoint, long sent)
        {
            ReceiveAsync();
        }
        protected override void OnDisconnected()
        {
            Thread.Sleep(1000);

            // Try to connect again
            if (!_stop)
                Connect();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            var packet = IPacket.FromByteArray(buffer);
            if (packet is UdpNotifyConnectedPacket)
                IsConnectedToServer = true;
            else if (packet is UdpHeartbeatPacket)
            {
                this.Send(new UdpHeartbeatPacket() {  ClientUsername = Username });
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
