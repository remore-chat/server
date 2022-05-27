using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Packets.Server
{
    public class ServerInfoUpdatedPacket : IPacket
    {
        public int Id => 41;
        public string Name { get; set; }
        public int MaxClients { get; set; }
        public string RequestId { get; set; }
    }
}
