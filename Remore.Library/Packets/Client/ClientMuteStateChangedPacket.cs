using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Client
{
    public class ClientMuteStateChangedPacket : IPacket
    {
        public int Id => 31;
        public string RequestId { get; set; }

    }
}
