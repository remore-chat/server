using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remore.Library.Packets.Server
{
    public class DisconnectPacket : IPacket
    {
        public DisconnectPacket()
        {
        }

        public DisconnectPacket(string reason)
        {
            Reason = reason;
        }
        public int Id => 2;
        public string Reason { get; set; }
        public string RequestId { get; set; }

    }
}
