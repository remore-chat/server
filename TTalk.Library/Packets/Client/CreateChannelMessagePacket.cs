using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Client
{
    public class CreateChannelMessagePacket : IPacket
    {
        public int Id => 27;
        public string ChannelId { get; set; }
        public string Text { get; set; }
    }
}
