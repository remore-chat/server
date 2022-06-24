using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Client
{
    public class UdpAuthenticationPacket : IPacket, IUdpPacket
    {
        public int Id => 13;

        public string ClientUsername { get; set; }
        public string TcpId { get; set; }
        public string RequestId { get; set; }

    }
}
