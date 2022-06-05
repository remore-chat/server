using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Packets.Client
{
    public class UpdateServerInfoPacket : IPacket
    {
        public int Id => 40;
        public string Name { get; set; }
        public int MaxClients { get; set; }
        public string RequestId { get; set; }
    }
}
