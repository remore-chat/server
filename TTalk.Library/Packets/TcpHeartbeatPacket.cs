using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Packets
{
    public class TcpHeartbeatPacket : IPacket
    {
        public int Id => 39;

        public string RequestId { get; set; }
    }
}
