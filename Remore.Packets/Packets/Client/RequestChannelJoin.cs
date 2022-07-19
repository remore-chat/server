using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Attributes;

namespace Remore.Library.Packets.Client
{
    [SourceGeneratorIgnorePacket]
    public class RequestChannelJoin : IPacket
    {
        public int Id => 20;
        public string ChannelId { get; set; }
        public string RequestId { get; set; }

    }
}
