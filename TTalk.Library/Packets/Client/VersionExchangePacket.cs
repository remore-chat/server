using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Packets.Client
{
    public class VersionExchangePacket : IPacket
    {
        public VersionExchangePacket()
        {
        }

        public VersionExchangePacket(string version)
        {
            Version = version;
        }

        public int Id => 1;
        public string Version { get; set; }
        public string RequestId { get; set; }

    }
}
