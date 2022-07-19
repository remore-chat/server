using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Client
{
    public class ServerQueryPacket : IPacket
    {
        public int Id => 32;
        public string RequestId { get; set; }

    }
}
