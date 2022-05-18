using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Server
{
    public class VoiceEstablishResponsePacket : IPacket
    {
        public int Id => 14;
        public bool Allowed { get; set; }
        public string RequestId { get; set; }

    }
}
