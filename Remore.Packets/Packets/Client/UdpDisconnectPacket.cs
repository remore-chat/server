using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Client
{
    public class UdpDisconnectPacket : IPacket, IUdpPacket
    {

        public int Id => 22;
        public string ClientUsername { get; set; }
        public string RequestId { get; set; }
    }
}
