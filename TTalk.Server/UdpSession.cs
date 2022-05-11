using NetCoreServer;
using System.Net;
using static TTalkServer;

public class UdpSession
{
    public UDPServer Server { get; set; }

    public EndPoint EndPoint { get; set; }
    public string Username { get; set; }
    public long HeartbeatReceivedAt { get; set; }
    public ServerSession TcpSession { get; set; }
}