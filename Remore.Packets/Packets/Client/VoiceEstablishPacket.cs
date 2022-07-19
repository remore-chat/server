using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Client
{
    public class VoiceEstablishPacket : IPacket
    {
        public int Id => 19;
        public string RequestId { get; set; }

    }
}
