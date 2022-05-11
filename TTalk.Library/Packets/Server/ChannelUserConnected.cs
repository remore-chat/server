using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Server
{
    public class ChannelUserConnected : IPacket
    {
        public int Id => 8;
        public string ChannelId { get; set; }
        public string Username { get; set; }
    }
}
