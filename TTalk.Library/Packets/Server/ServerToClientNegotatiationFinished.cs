using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Server
{
    public class ServerToClientNegotatiationFinished : IPacket
    {
        public int Id => 30;
    }
}
