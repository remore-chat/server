using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Packets
{
    public interface IPacket
    {
        public int Id { get; }

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
