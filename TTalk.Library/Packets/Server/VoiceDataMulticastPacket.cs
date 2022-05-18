using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Server
{
    public class VoiceDataMulticastPacket : IPacket
    {
        public int Id => 15;
        public string Username { get; set; }
        public byte[] VoiceData { get; set; }
        public string RequestId { get; set; }

    }
}
