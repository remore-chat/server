using NetCoreServer;
using System.Net.Sockets;
using System.Text;
using TTalk.Library.Models;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;
using TTalk.Library.Packets.Server;
using TTalk.Server;

public class ServerSession : TcpSession
{
    public TTalkServer Server { get; }
    public TcpServer TCP => Server.TCP;

    public SessionState State { get; set; }
    public string PrivilegeKey { get; set; }
    public string Username { get; set; }
    public Channel CurrentChannel { get; set; }

    public ServerSession(TTalkServer server) : base(server.TCP)
    {
        Server = server;
        State = SessionState.VersionExchange;
    }

    protected override void OnConnected()
    {
        Logger.LogInfo($"TCP session with Id {Id} connected!");
        Server.Clients.Add(this);
    }


    protected override void OnDisconnected()
    {
        Logger.LogInfo($"TCP session with Id {Id} disconnected!");
        DisconnectCurrentFromChannel();
        var connectedClient = CurrentChannel?.ConnectedClients.FirstOrDefault(x => x.Username == Username);
        if (connectedClient != null)
            Server.UDP.Clients.Remove(connectedClient);
        Server.Clients.Remove(this);
    }

    protected override async void OnReceived(byte[] buffer, long offset, long size)
    {
        var packet = IPacket.FromByteArray(buffer);
        if (State == SessionState.VersionExchange)
        {
            if (packet is ServerQueryPacket queryPacket)
            {
                Logger.LogInfo($"Query request from {Id}");
                Server.Clients.Remove(this);
                this.Send(new ServerQueryResponsePacket()
                {
                    ServerName = Server.Name,
                    ServerVersion = Server.Version,
                    ClientsConnected = Server.Clients.Count,
                    MaxClients = Server.MaxClients
                });
                Thread.Sleep(200);
                this.Disconnect();
                Logger.LogInfo($"Query client disconnected {Id}");
                return;
            }
            if (packet is not VersionExchangePacket vs)
            {
                Logger.LogWarn($"Invalid packet received at state {State} from client {Id}");
                this.Disconnect();
                return;
            }
            if (vs.Version != Server.Version)
            {

                this.Send(new DisconnectPacket($"Client version {vs.Version} doesn't match server version {Server.Version}"));
                this.Disconnect();
            }
            else
            {
                this.Send(new StateChangedPacket(SessionState.Authenticating));
                State = SessionState.Authenticating;
            }
        }
        else if (State == SessionState.Authenticating)
        {
            if (packet is not AuthenticationDataPacket auth)
            {
                Logger.LogWarn($"Invalid packet received at state {State} from client {Id}");
                this.Send(new DisconnectPacket($"Invalid packet received at state {State} from client {Id}"));
                this.Disconnect();
                return;
            }
            if (string.IsNullOrEmpty(auth.Username) || auth.Username.Length > 16)
            {
                Logger.LogWarn($"Client {Id} send invalid username {auth.Username}");
                this.Send(new DisconnectPacket($"Invalid username"));
                this.Disconnect();
                return;
            }
            if (auth.ServerPrivilegeKey != "" && auth.ServerPrivilegeKey != Server.Configuration.PrivilegeKey?.Key)
            {
                Logger.LogInfo($"Client {Id} tried to connect with invalid privilege key");
                this.Send(new DisconnectPacket($"Invalid privilege key"));
                this.Disconnect();
                return;
            }
            if (auth.ServerPrivilegeKey == Server.Configuration.PrivilegeKey?.Key)
            {
                PrivilegeKey = auth.ServerPrivilegeKey;
                Logger.LogInfo($"Client {Id} connected with server privilege key");
            }

            State = SessionState.Connected;
            this.Send(new StateChangedPacket(SessionState.Connected, this.Id.ToString()));
            if (Server.Clients.Count >= Server.MaxClients)
            {
                this.Send(new DisconnectPacket() { Reason = "Maximum amount of connected clients reached" });
                this.Disconnect();
                return;
            }
            Logger.LogInfo($"Client {Id} connected with username {auth.Username}");
            Username = auth.Username;
            TCP.Multicast(new ClientConnectedPacket(auth.Username));
            foreach (var channel in Server.Channels)
            {
                this.Send(new ChannelAddedPacket(channel.Id, channel.Name, channel.ConnectedClients.Select(x => x.Username).ToList(), channel.Bitrate, channel.Order, channel.ChannelType, channel.MaxClients));
                await Task.Delay(10);
            }
            if (PrivilegeKey == Server.Configuration.PrivilegeKey?.Key)
                this.Send(new ClientPermissionsUpdatedPacket() { HasAllPermissions = true });
            this.Send(new ServerToClientNegotatiationFinished());

        }
        else if (State == SessionState.Connected)
        {

            if (packet is VersionExchangePacket || packet is AuthenticationDataPacket)
            {
                Logger.LogWarn($"Invalid packet received at state {State} from client {Id}");
                this.Disconnect();
            }
            else if (packet is CreateChannelMessagePacket channelMessage)
            {
                var id = channelMessage.ChannelId;
                var text = channelMessage.Text;
                var channel = Server.Channels.FirstOrDefault(x => x.Id == id);
                if (channel == null)
                    return;
                if (channel.ChannelType != TTalk.Library.Enums.ChannelType.Text)
                    return;
                if (text.Length > 2000 || string.IsNullOrEmpty(text))
                    return;
                var message = new ChannelMessage()
                {
                    Id = Guid.NewGuid().ToString(),
                    ChannelId = id,
                    Username = this.Username,
                    Message = text
                };
                channel.Messages.Add(message);
                TCP.Multicast(new ChannelMessageAddedPacket()
                {
                    ChannelId = channel.Id,
                    MessageId = message.Id,
                    SenderName = message.Username,
                    Text = message.Message
                });
            }
            else if (packet is RequestChannelMessagesPacket messagesPacket)
            {
                var id = messagesPacket.ChannelId;
                var page = messagesPacket.Page;
                var channel = Server.Channels.FirstOrDefault(x => x.Id == id);
                if (channel == null)
                    return;
                if (channel.ChannelType != TTalk.Library.Enums.ChannelType.Text)
                    return;
                if (messagesPacket.Page < 0)
                    return;
                var messages = channel.Messages.Skip(page * 20).Take(20).ToList();
                this.Send(new ChannelMessagesResponse()
                {
                    ChannelId = channel.Id,
                    Messages = messages
                });
            }
            else if (packet is LeaveChannelPacket)
            {
                if (this.CurrentChannel != null)
                {
                    DisconnectCurrentFromChannel();
                }
            }
            else if (packet is VoiceEstablishPacket voiceEstablish)
            {
                Logger.LogInfo("Got voice establish packet");
                if (this.CurrentChannel != null)
                    this.Send(new VoiceEstablishResponsePacket() { Allowed = true });
                else
                    this.Send(new VoiceEstablishResponsePacket() { Allowed = false });
            }
            else if (packet is RequestChannelJoin channelJoin)
            {
                var channel = Server.Channels.FirstOrDefault(x => x.Id == channelJoin.ChannelId);
                if (channel == null)
                {
                    this.Send(new RequestChannelJoinResponse() { Allowed = false, Reason = "Channel not found" });
                }
                else if (channel.Id == CurrentChannel?.Id)
                {
                    this.Send(new RequestChannelJoinResponse() { Allowed = false, Reason = "Already joined" });
                }
                else if (channel.ChannelType == TTalk.Library.Enums.ChannelType.Text)
                {
                    this.Send(new RequestChannelJoinResponse() { Allowed = false, Reason = "This is text channel" });
                }
                else
                {
                    if (channel.ConnectedClients.Count >= channel.MaxClients)
                    {
                        this.Send(new RequestChannelJoinResponse() { Allowed = false, Reason = "Maximum limit of connected clients reached" });
                    }
                    else
                    {
                        var udpClient = Server.UDP.Clients.FirstOrDefault(x => x.TcpSession.Id == this.Id);
                        if (udpClient == null)
                        {
                            this.Send(new RequestChannelJoinResponse() { Allowed = false, Reason = "Your client isn't connected to UDP server" });
                        }
                        else
                        {
                            if (CurrentChannel != null)
                            {
                                CurrentChannel.ConnectedClients.Remove(udpClient);
                                this.TCP.Multicast(new ChannelUserDisconnected() { ChannelId = CurrentChannel.Id, Username = this.Username });
                                CurrentChannel = null;
                            }
                            channel.ConnectedClients.Add(udpClient);
                            CurrentChannel = channel;
                            this.Send(new RequestChannelJoinResponse() { Allowed = true, Reason = "" });
                            TCP.Multicast(new ChannelUserConnected() { ChannelId = channel.Id, Username = this.Username });
                        }
                    }
                }
            }
        }
    }

    private void ConnectUserToChannel(Channel channel)
    {
        var user = channel.ConnectedClients.FirstOrDefault(x => x.Username == this.Username);
        if (user != null)
            return;
        channel.ConnectedClients.Add(user);
        CurrentChannel = channel;
        TCP.Multicast(new ChannelUserConnected() { ChannelId = channel.Id, Username = this.Username });
    }



    public override bool Disconnect()
    {
        var connectedClient = CurrentChannel?.ConnectedClients.FirstOrDefault(x => x.Username == Username);
        if (connectedClient != null)
            Server.UDP.Clients.Remove(connectedClient);
        DisconnectCurrentFromChannel();
        return base.Disconnect();
    }
    private void DisconnectCurrentFromChannel()
    {
        var connectedClient = CurrentChannel?.ConnectedClients.FirstOrDefault(x => x.Username == Username);
        if (connectedClient == null)
            return;
        //Server.UDP.Clients.Remove(connectedClient);
        CurrentChannel.ConnectedClients.Remove(connectedClient);
        TCP.Multicast(new ChannelUserDisconnected() { ChannelId = CurrentChannel.Id, Username = this.Username });
        this.CurrentChannel = null;
    }

    protected override void OnError(SocketError error)
    {
        Logger.LogError($"TCP session caught an error with code {error}");
    }
}