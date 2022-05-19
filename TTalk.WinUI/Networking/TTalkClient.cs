using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTalk.WinUI.Networking.EventArgs;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;
using TTalk.WinUI.ViewModels;
using TTalk.WinUI.Contracts.Services;

namespace TTalk.WinUI.Networking.ClientCode
{
    internal class TTalkClient : TcpClient
    {
        
        public TTalkClient(string address, int port) : base(address, port)
        {
            TcpId = null;
            _settingsService = App.GetService<ILocalSettingsService>();
            _username = _settingsService.ReadSettingAsync<string>(SettingsViewModel.UsernameSettingsKey).GetAwaiter().GetResult();
        }


        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        public event EventHandler<object> OnClientReady;
        public event EventHandler<object> OnClientDisconnected;
        public event EventHandler<System.Net.Sockets.SocketError> SocketErrored;

        public string PrivilegeKey { get; private set; }
        public SessionState State { get; set; }
        public string TcpId { get; private set; }

        private ILocalSettingsService _settingsService;
        private string _username;

        protected override async void OnConnected()
        {
            Initialize();
            ReceiveAsync();
            
        }

        private async Task Initialize()
        {
            await Task.Delay(100);
            PrivilegeKey = (await _settingsService.ReadSettingAsync<string>($"{Address}:{Port}PrivilegeKey")) ?? "";
            State = SessionState.VersionExchange;
            this.Send(new VersionExchangePacket(SettingsViewModel.ClientVersion));
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
                    this.Send(new AuthenticationDataPacket(_username, PrivilegeKey));
                }
                else if (state == SessionState.Connected)
                {
                    OnClientReady?.Invoke(this, null);
                    TcpId = stateChanged.ClientId;
                }
            }
            else if (packet is DisconnectPacket disconnect)
            {
                PacketReceived.Invoke(this, new() { Packet = disconnect });
                return;
            }
            else if (State == SessionState.Connected)
            {
                PacketReceived?.Invoke(this, new() { Packet = packet });
            }
        }

        protected override void OnDisconnected()
        {
            OnClientDisconnected?.Invoke(this, null);
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
