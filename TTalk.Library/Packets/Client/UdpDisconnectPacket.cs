using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Client
{
    public class UdpDisconnectPacket : IPacket, IUdpPacket
    {
        public string ClientUsername { get; set; }

        public int Id => 22;
    }
}
