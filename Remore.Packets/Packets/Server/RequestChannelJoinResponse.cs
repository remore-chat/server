using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Server
{
    public class RequestChannelJoinResponse : IPacket
    {
        public int Id => 21;
        public bool Allowed { get; set; }

        //Only present if allowed = false; otherwise empty string
        public string Reason { get; set; }
        public string RequestId { get; set; }

    }
}
