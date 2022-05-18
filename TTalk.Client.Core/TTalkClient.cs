using System.Net;
using System.Net.Sockets;
using TTalk.Client.Core.EventArgs;
using TTalk.Client.Core.Seb;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;

namespace TTalk.Client.Core
{
    public class TTalkClient
    {
        private const string version = "1.0.0";

        /// <summary>
        /// Initializes new TTalk Client
        /// </summary>
        /// <param name="ip">IP address of ther server</param>
        /// <param name="port">Port of the server</param>
        /// <param name="username">Username that will be used on server</param>
        /// <param name="password">User server password (currently unimplemented)</param>
        public TTalkClient(string ip, int port, string username, string password = null)
        {
            Ip = ip;
            Port = port;
            Version = version;
            Username = username;
            _tcpClient = new TTalkTcpClient(Ip, Port, Username, Version);
            _tcpClient.SocketErrored += OnTcpSocketErrored;
        }

        /// <summary>
        /// Initializes new TTalk bot client (currently unavailable)
        /// </summary>
        /// <param name="ip">IP address of ther server</param>
        /// <param name="port">Port of the server</param>
        /// <param name="botToken">Token of the bot generated on server</param>
        public TTalkClient(string ip, int port, string botToken)
        {
            Ip = ip;
            Port = port;
            BotToken = botToken;
            Version = $"BOT-{version}";
        }


        public string Version { get; }
        public string Username { get; }
        public string Ip { get; }
        public int Port { get; }
        public bool IsBot => BotToken != null;
        public string BotToken { get; }
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        private TTalkTcpClient _tcpClient;
        private TTalkUdpClient _udpClient;
        private SocketError _tcpConnectionError;
        private PendingDictionary<string, IPacket> _pending = new();

        public async Task ConnectAsync()
        {
            await InternalTcpConnectAsync();
            await InternalUdpConnect();
        }

        public async Task SendPacketTCP(IPacket packet)
        {
            if (!_tcpClient.IsConnected && _tcpClient.State != SessionState.Connected)
                throw new Exception("Can't send packets if state is not connected");

            _tcpClient.Send(packet);
        }

        public async Task SendPacketUDP(IUdpPacket packet)
        {
            if (!_udpClient.IsConnectedToServer)
                throw new Exception("Can't send packets if state is not connected");
            _udpClient.Send(packet as IPacket);
        }

        #region Packet sending methods implementation

        public async Task<RequestChannelJoinResponse?> RequestChannelJoinAsync(string channelId)
        {
            var packet = new RequestChannelJoin()
            {
                ChannelId = channelId,
                RequestId = Guid.NewGuid().ToString()
            };
            await SendPacketTCP(packet);
            return await _pending.WaitForValueAsync(packet.RequestId, 5000) as RequestChannelJoinResponse;
        }
        #endregion


        private async Task InternalTcpConnectAsync()
        {
            var tcpSuccess = _tcpClient.Connect();
            if (!tcpSuccess)
            {
                _tcpConnectionError = default;
                throw new SocketException((int)_tcpConnectionError);
            }
            var tcpIdWaiter = new Task(async () =>
            {
                while (_tcpClient.TcpId == null)
                { await Task.Delay(200); }
            });
            await Task.WhenAny(tcpIdWaiter, Task.Delay(5000));
            var tcpId = _tcpClient.TcpId;
            if (tcpId == null)
                throw new Exception("Failed to connect to server. Client didn't receive tcp id in 5 seconds after connecting");
            _tcpClient.PacketReceived += OnTcpPacketReceived;
        }

        private void OnTcpPacketReceived(object? sender, EventArgs.PacketReceivedEventArgs e)
        {
            if (_pending.Add(e.Packet.RequestId, e.Packet))
                return;

            PacketReceived?.Invoke(this, e);
        }

        private async Task InternalUdpConnect()
        {
            _udpClient = new TTalkUdpClient(_tcpClient.TcpId, Username, IPAddress.Parse(Ip), Port);
            _udpClient.Connect();
            var connectionWaiter = new Task(async () =>
            {
                while (!_udpClient.IsConnectedToServer)
                    await Task.Delay(200);
            });
            await Task.WhenAny(connectionWaiter, Task.Delay(5000));
            if (!_udpClient.IsConnectedToServer)
                throw new Exception("Udp client failed to connect");
        }


        private void OnTcpSocketErrored(object? sender, SocketError e)
        {
            _tcpConnectionError = e;
        }

    }
}