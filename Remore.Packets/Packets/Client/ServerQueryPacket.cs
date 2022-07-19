using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Attributes;

namespace Remore.Library.Packets.Client
{
    [SourceGeneratorIgnorePacket]
    public class ServerQueryPacket : IPacket
    {
        public int Id => 32;
        public string RequestId { get; set; }

    }
}
