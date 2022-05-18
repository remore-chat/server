using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Server
{
    public class ChannelUserDisconnected : IPacket
    {
        public int Id => 9;
        public string ChannelId { get; set; }
        public string Username { get; set; }
        public string RequestId { get; set; }

    }
}
