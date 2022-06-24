using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;


namespace TTalk.Client.Core
{
    internal class TTalkUdpClient : UdpClient
    {
        public TTalkUdpClient(string tcpId, IPAddress address, int port, string username) : base(address, port)
        {
            TcpId = tcpId;
            Username = username;
        }
        public string TcpId { get;}
        public string Username { get; set; }
        public bool IsConnectedToServer { get; set; }

        public event EventHandler<object> Disconnected;
        public event EventHandler<IPacket> PacketReceived;

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
            IsConnectedToServer = false;
            Disconnected?.Invoke(this, null);
            
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            var packet = IPacket.FromByteArray(buffer);
            if (packet is UdpNotifyConnectedPacket)
                IsConnectedToServer = true;
            else if (packet is UdpHeartbeatPacket)
            {
                IsConnectedToServer = true;
                this.Send(new UdpHeartbeatPacket() {  ClientUsername = Username });
            }
            else
            {
                PacketReceived?.Invoke(this, packet);
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
