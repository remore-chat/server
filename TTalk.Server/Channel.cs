using NetCoreServer;
using TTalk.Library.Enums;
using TTalk.Library.Models;

namespace TTalk.Server;
public class Channel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<UdpSession> ConnectedClients { get; set; }
    public int MaxClients { get; set; }
    public int Bitrate { get; set; }
    public int Order { get; set; }
    public ChannelType ChannelType { get; set; }
    public List<ChannelMessage> Messages { get; set; }
}
