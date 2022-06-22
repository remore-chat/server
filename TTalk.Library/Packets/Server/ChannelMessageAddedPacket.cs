using System;
using System.Collections.Generic;
using System.Text;
using TTalk.Library.Models;

namespace TTalk.Library.Packets.Server
{
    public class ChannelMessageAddedPacket : IPacket
    {
        public ChannelMessageAddedPacket()
        {
        }

        public int Id => 25;
        public ChannelMessage Message { get; set; }
        public string RequestId { get; set; }
    }
}
