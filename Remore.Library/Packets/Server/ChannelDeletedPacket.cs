using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remore.Library.Packets.Server
{
    public class ChannelDeletedPacket : IPacket
    {
        public int Id => 38;
        public string ChannelId { get; set; }
        public string RequestId { get; set; }
    }
}
