using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Remore.Library.Packets;
using Remore.Library.Packets.Client;
using Remore.Library.Packets.Server;


namespace Remore.Client.Core
{
    internal class RemoreTcpClient : TcpClient
    {

        private long lastHeartbeatReceived = 0;
        private Timer _heartbeatTimer;

        public RemoreTcpClient(string address, int port, string username, string? privilegeKey = null) : base(address, port)
        {
            TcpId = null;
            Username = username;
            PrivilegeKey = privilegeKey;
        }


        public event EventHandler<IPacket> PacketReceived;
        public event EventHandler<object> Ready;
        public event EventHandler<object> Disconnected;
        public event EventHandler<System.Net.Sockets.SocketError> SocketErrored;

        public string PrivilegeKey { get; private set; }
        public SessionState State { get; set; }
        public string TcpId { get; private set; }
        public string Username { get; }

        protected override async void OnConnected()
        {
            await Task.Delay(100);
            this.Send(new VersionExchangePacket(Constants.ClientVersion));
            ReceiveAsync();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            using var reader = new ByteReaderWriter(buffer);
            int lengthOfPacket = reader.ReadInt();
            int id = reader.ReadInt(false);
            if (lengthOfPacket == size - 4)
            {
                HandlePacket(buffer);
                return;
            }
            else
            {
                int _offset = 0;
                do
                {
                    var packet = buffer.Slice(_offset, lengthOfPacket + 4);
                    HandlePacket(packet);
                    _offset += lengthOfPacket + 4;
                    reader.Position = _offset;
                    lengthOfPacket = reader.ReadInt();
                    id = reader.ReadInt(false);

                } while (_offset + lengthOfPacket < size);
            }
            ReceiveAsync();
        }

        private void HandlePacket(byte[] buffer)
        {
            var packet = IPacket.FromByteArray(buffer);
            if (packet is StateChangedPacket stateChanged)
            {
                var state = (SessionState)stateChanged.NewState;
                State = state;
                if (state == SessionState.Authenticating)
                {
                    this.Send(new AuthenticationDataPacket(Username, PrivilegeKey ?? ""));
                }
                else if (state == SessionState.Connected)
                {
                    TcpId = stateChanged.ClientId;
                    lastHeartbeatReceived = DateTimeOffset.Now.ToUnixTimeSeconds();
                    _heartbeatTimer = new Timer((_) =>
                    {
                        if (_stop)
                            return;
                        if (DateTimeOffset.Now.ToUnixTimeSeconds() - lastHeartbeatReceived > 10)
                        {
                            PacketReceived?.Invoke(this, new DisconnectPacket() { Reason = "TIMED_OUT" });
                            this.DisconnectAndStop();
                        }
                    }, 
                    null, 0, 10*1000);
                    Ready?.Invoke(this, null);
                }
            }
            else if (packet is DisconnectPacket disconnect)
            {
                PacketReceived?.Invoke(this, disconnect);
                return;
            }
            else if (State == SessionState.Connected)
            {
                if (packet is TcpHeartbeatPacket)
                {
                    this.Send(new TcpHeartbeatPacket());
                    lastHeartbeatReceived = DateTimeOffset.Now.ToUnixTimeSeconds();
                }
                else
                {
                    PacketReceived?.Invoke(this, packet);
                }
            }
        }

        protected override void OnDisconnected()
        {
            Disconnected?.Invoke(this, null);
        }

        public void DisconnectAndStop()
        {
            _stop = true;
            DisconnectAsync();
            while (IsConnected)
                Thread.Yield();
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            SocketErrored?.Invoke(this, error);
        }


        private bool _stop;

    }
}
