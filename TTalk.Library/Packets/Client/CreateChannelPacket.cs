using System;
using System.Collections.Generic;
using System.Text;
using TTalk.Library.Enums;

namespace TTalk.Library.Packets.Client
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
