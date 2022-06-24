using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets
{
    public interface IUdpPacket
    {
        public string ClientUsername { get; set; }
    }
}
