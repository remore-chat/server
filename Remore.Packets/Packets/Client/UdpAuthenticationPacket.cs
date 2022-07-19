using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Attributes;

namespace Remore.Library.Packets.Client
{
    [SourceGeneratorIgnorePacket]
    public class UdpAuthenticationPacket : IPacket, IUdpPacket
    {
        public int Id => 13;

        public string ClientUsername { get; set; }
        public string TcpId { get; set; }
        public string RequestId { get; set; }

    }
}
