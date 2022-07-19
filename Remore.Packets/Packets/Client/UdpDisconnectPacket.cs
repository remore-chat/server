using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Attributes;

namespace Remore.Library.Packets.Client
{
    [SourceGeneratorIgnorePacket]
    public class UdpDisconnectPacket : IPacket, IUdpPacket
    {

        public int Id => 22;
        public string ClientUsername { get; set; }
        public string RequestId { get; set; }
    }
}
