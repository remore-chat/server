using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Server
{
    public class ChannelUpdatedPacket : IPacket
    {
        public int Id => 23;
        public string ChannelId { get; set; }
        public string Name { get; set; }
        public int Bitrate { get; set; }

    }
}
