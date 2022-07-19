using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Client
{
    public class VoiceDataPacket : IPacket, IUdpPacket
    {
        public int Id => 12;
        public string ClientUsername { get; set; }
        public byte[] VoiceData { get; set; }
        public string RequestId { get; set; }

    }
}
