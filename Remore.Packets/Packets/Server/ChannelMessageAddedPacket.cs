using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Server
{
    public class ChannelMessageAddedPacket : IPacket
    {
        public ChannelMessageAddedPacket()
        {
        }

        public int Id => 25;
        public string MessageId { get; set; }
        public string ChannelId { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }
        public string RequestId { get; set; }

    }
}
