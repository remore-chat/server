using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Server
{
    public class UdpNotifyConnectedPacket : IPacket, IUdpPacket
    {
        public string RequestId { get; set; }
        public string ClientUsername { get; set; }
        public int Id => 16;
    }
}
