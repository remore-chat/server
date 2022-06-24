using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets
{
    public class UdpHeartbeatPacket : IPacket, IUdpPacket
    {
        public int Id => 11;
        public string ClientUsername { get; set; }
        public string RequestId { get; set; }
    }
}
