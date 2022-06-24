using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Server
{
    public class ChannelUserConnected : IPacket
    {
        public int Id => 8;
        public string ChannelId { get; set; }
        public string Username { get; set; }
        public string RequestId { get; set; }

    }
}
