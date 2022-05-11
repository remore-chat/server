using System;
using System.Collections.Generic;
using System.Text;
using TTalk.Library.Models;

namespace TTalk.Library.Packets.Server
{
    public class ChannelMessagesResponse : IPacket
    {
        public int Id => 29;
        public string ChannelId { get; set; }
        public List<ChannelMessage> Messages { get; set; }
    }
}
