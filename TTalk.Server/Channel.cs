using NetCoreServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TTalk.Library.Enums;
using TTalk.Library.Models;

namespace TTalk.Server;
public class Channel
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    [NotMapped]
    public List<UdpSession> ConnectedClients { get; set; }
    public int MaxClients { get; set; }
    public int Bitrate { get; set; }
    public int Order { get; set; }
    public ChannelType ChannelType { get; set; }
    public List<ChannelMessage> Messages { get; set; }

    public Channel()
    {
        Id = Guid.NewGuid().ToString();
        Messages = new();
        ConnectedClients = new();
    }
}
