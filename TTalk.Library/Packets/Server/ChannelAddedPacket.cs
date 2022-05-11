using System;
using System.Collections.Generic;
using System.Text;
using TTalk.Library.Enums;

namespace TTalk.Library.Packets.Server
{
    public class ChannelAddedPacket : IPacket
    {
        public int Id => 7;
        public string ChannelId { get; set; }
        public string Name { get; set; }
        public ChannelType ChannelType { get; set; }
        public int Bitrate { get; set; }
        public int Order { get; set; }
        public List<string> Clients { get; set; }

        public ChannelAddedPacket()
        {

        }

        public ChannelAddedPacket(string channelId, string name, List<string> clients, int bitrate, int order, ChannelType type)
        {
            ChannelId = channelId;
            Name = name;
            Clients = clients ?? new List<string>();
            Bitrate = bitrate;
            Order = order;
            ChannelType = type;
        }
    }
}
