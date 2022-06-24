using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Enums;

namespace Remore.Library.Packets.Server
{
    public class ChannelAddedPacket : IPacket
    {
        public int Id => 7;
        public string ChannelId { get; set; }
        public string Name { get; set; }
        public ChannelType ChannelType { get; set; }
        public int Bitrate { get; set; }
        public int Order { get; set; }
        public int MaxClients { get; set; }
        public List<string> Clients { get; set; }
        public string RequestId { get; set; }


        public ChannelAddedPacket()
        {

        }

        public ChannelAddedPacket(string channelId, string name, List<string> clients, int bitrate, int order, ChannelType type, int maxClients)
        {
            ChannelId = channelId;
            Name = name;
            Clients = clients ?? new List<string>();
            Bitrate = bitrate;
            Order = order;
            ChannelType = type;
            MaxClients = maxClients;
        }
    }
}
