using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Client
{
    public class RequestChannelJoin : IPacket
    {
        public int Id => 20;
        public string ChannelId { get; set; }
        public string RequestId { get; set; }

    }
}
