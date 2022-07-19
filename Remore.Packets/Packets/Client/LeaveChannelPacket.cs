using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Client
{
    public class LeaveChannelPacket : IPacket
    {
        public int Id => 34;
        public string RequestId { get; set; }

    }
}
