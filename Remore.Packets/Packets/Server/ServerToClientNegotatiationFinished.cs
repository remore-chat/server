using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Server
{
    public class ServerToClientNegotatiationFinished : IPacket
    {
        public int Id => 30;
        public string RequestId { get; set; }
        
    }
}
