using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Attributes;

namespace Remore.Library.Packets.Client
{
    [SourceGeneratorIgnorePacket]
    public class RequestChannelMessagesPacket : IPacket
    {
        public int Id => 26;
        public string ChannelId { get; set; }
        public int Page { get; set; }
        public string RequestId { get; set; }

    }
}
