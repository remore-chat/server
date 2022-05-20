using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Packets.Client
{
    public class DeleteChannelPacket : IPacket
    {
        public int Id => 37;
        public string ChannelId { get; set; }
        public string RequestId { get; set; }
    }
}
