using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Enums;

namespace Remore.Library.Packets.Client
{
    public class CreateChannelPacket : IPacket
    {
        public int Id => 36;
        public string Name { get; set; }
        public int MaxClients { get; set; }
        public int Bitrate { get; set; }
        public int Order { get; set; }
        public ChannelType ChannelType { get; set; }
        public string RequestId { get; set; }

    }
}
