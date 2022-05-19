using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets
{
    public class UdpHeartbeatPacket : IPacket, IUdpPacket
    {
        public int Id => 11;
        public string ClientUsername { get; set; }
        public string RequestId { get; set; }
    }
}
