using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Client
{
    public class ClientMuteStateChangedPacket : IPacket
    {
        public int Id => 31;
    }
}
