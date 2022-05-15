using NetCoreServer;
using System.Net;
using System.Net.Sockets;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;
using TTalk.Server;
using TTalk.Server.Services;

public class TTalkServer
{

    public IPAddress Ip { get; set; }
    public int Port { get; set; }
    public string Version => "1.0.0";
    public List<Channel> Channels { get; set; }
    public List<ServerSession> Clients { get; set; }
    public TCPServer TCP { get; }
    public UDPServer UDP { get; set; }

    public string Name => Configuration.Name;
    public int MaxClients => Configuration.MaxClients;
    public ServerConfiguration Configuration { get; set; }

    private ConfigurationService _configurationService;
    public void Start()
    {
        Task.Run(async () =>
        {
            Configuration = await _configurationService.GetServerConfigurationAsync();
        }).GetAwaiter().GetResult();
        TCP.Start();
        UDP.Start();
    }


    public void Stop()
    {
        TCP.Multicast(new DisconnectPacket() { Reason = "Server stopped." });
        TCP.Stop();
        UDP.Stop();
    }

    public TTalkServer(IPAddress ip, int port)
    {
        Ip = ip;
        Port = port;
        TCP = new(this, ip, port);
        UDP = new(this, IPAddress.Any, port);
        _configurationService = ServiceContainer.GetService<ConfigurationService>();
        Channels = new()
        {
            new Channel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Main",
                ConnectedClients = new(),
                MaxClients = 32,
                Bitrate = 500000,
                ChannelType = TTalk.Library.Enums.ChannelType.Voice,
                Order = 0
            },
            new Channel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Second",
                ConnectedClients = new(),
                MaxClients = 32,
                Bitrate = 64000,
                ChannelType = TTalk.Library.Enums.ChannelType.Voice,
                Order = 1
            },
            new Channel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Third",
                ConnectedClients = new(),
                MaxClients = 12,
                Bitrate = 12000,
                ChannelType = TTalk.Library.Enums.ChannelType.Voice,
                Order = 2
            },
            new Channel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "TextChannel",
                ConnectedClients = new(),
                MaxClients = 0,
                Bitrate = 0,
                ChannelType = TTalk.Library.Enums.ChannelType.Text,
                Order = 3,
                Messages = new()
            },
            new Channel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Text channel 2",
                ConnectedClients = new(),
                MaxClients = 0,
                Bitrate = 0,
                ChannelType = TTalk.Library.Enums.ChannelType.Text,
                Order = 4,
                Messages = new()
            }
        };
        Clients = new();
    }

    public class UDPServer : UdpServer
    {
        public UDPServer(TTalkServer server, IPAddress address, int port) : base(address, port)
        {
            Server = server;
            Clients = new();
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var client in Clients.ToList())
                    {
                        if (DateTimeOffset.Now.ToUnixTimeSeconds() - client.HeartbeatReceivedAt > 10)
                        {
                            Clients.Remove(client);
                            var channel = Server.Channels.FirstOrDefault(x => x.ConnectedClients.Any(x => x == client));
                            if (channel != null)
                                channel.ConnectedClients.Remove(client);
                            continue;
                        }
                        this.Send(client.EndPoint, new PacketWriter(new UdpHeartbeatPacket() { ClientUsername = client.Username }).Write());
                    }
                    await Task.Delay(10 * 1000);
                }
            });
        }

        public List<UdpSession> Clients { get; }
        public TTalkServer Server { get; }

        protected override void OnStarted()
        {
            ReceiveAsync();
        }

        protected override async void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            var packet = IPacket.FromByteArray(buffer);
            var client = Clients.FirstOrDefault(x => x.EndPoint.ToString() == endpoint.ToString());
            if (client == null)
            {
                if (packet is UdpAuthenticationPacket udpAuthentication)
                {
                    var tcpSession = Server.Clients.FirstOrDefault(x => x.Id.ToString() == udpAuthentication.TcpId);
                    if (tcpSession != null && tcpSession.Username == udpAuthentication.ClientUsername)
                    {

                        var session = new UdpSession()
                        {
                            EndPoint = endpoint,
                            HeartbeatReceivedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            Server = this,
                            TcpSession = tcpSession,
                            Username = udpAuthentication.ClientUsername,
                        };
                        Clients.Add(session);
                        Logger.LogInfo($"New UDP client {tcpSession.Id} ({udpAuthentication.ClientUsername}) connected");
                        this.Send(endpoint, new UdpNotifyConnectedPacket() { ClientUsername = udpAuthentication.ClientUsername });
                    }
                }
            }
            else if (packet is UdpHeartbeatPacket heartbeat)
            {
                if (ValidateClient(endpoint, heartbeat.ClientUsername, out var session))
                {
                    //Logger.LogInfo($"Heartbeat {session.TcpSession.Id} ({session.Username})");
                    session.HeartbeatReceivedAt = DateTimeOffset.Now.ToUnixTimeSeconds();

                }
            }
            else if (packet is VoiceDataPacket voiceData)
            {
                if (ValidateClient(endpoint, voiceData.ClientUsername, out var session))
                {
                    var tcp = session.TcpSession;
                    if (tcp.CurrentChannel == null)
                    {
                        //Logger.LogWarn("Got invalid voice data packet");
                    }
                    else
                        Parallel.ForEach(tcp.CurrentChannel.ConnectedClients.ToList(), (vClient) =>
                        {
                            //Do not send voice data back to sender
                            if (vClient.Username == session.Username)
                                return;
                            var actualSent = this.Send(vClient.EndPoint, new VoiceDataMulticastPacket() { Username = session.Username, VoiceData = voiceData.VoiceData });
                        });
                }
            }
            else if (packet is UdpDisconnectPacket disconnectPacket)
            {
                if (ValidateClient(endpoint, disconnectPacket.ClientUsername, out var session))
                {
                    var currentChannel = session.TcpSession.CurrentChannel;
                    if (currentChannel != null)
                    {
                        if (currentChannel.ConnectedClients.Remove(session))
                            this.Server.TCP.Multicast(new ChannelUserDisconnected() { ChannelId = currentChannel.Id, Username = session.Username });
                    }
                    session.TcpSession.CurrentChannel = null;

                    Clients.Remove(session);
                }
            }
            ReceiveAsync();
        }


        public bool ValidateClient(EndPoint endpoint, string username, out UdpSession client)
        {
            client = Clients.FirstOrDefault(x => x.EndPoint.ToString() == endpoint.ToString() && x.Username == username);
            return client != null;
        }

    }

    public class TCPServer : TcpServer
    {

        public TCPServer(TTalkServer server, IPAddress address, int port) : base(address, port)
        {
            Server = server;
        }

        public TTalkServer Server { get; }

        protected override TcpSession CreateSession()
        {
            return new ServerSession(Server);
        }

        protected override void OnError(SocketError error)
        {
            Logger.LogError($"TCP server caught an error with code {error}");
        }
    }
}