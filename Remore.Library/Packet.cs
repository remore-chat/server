using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remore.Library.Packets;

namespace Remore.Library
{
    public static class Packet
    {
        public static IPacket FromByteArray(byte[] data)
        {
            try
            {
                return new PacketReader(data).Read();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
