using NetCoreServer;

public class Channel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<UdpSession> ConnectedClients { get; set; }
    public int MaxClients { get; set; }
    public int Bitrate { get; set; }
}