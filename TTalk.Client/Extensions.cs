using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTalk.Library.Packets;

namespace TTalk.Client
{
    public static class Extensions
    {
        public static long Send(this TcpClient session, IPacket packet)
        {
            return session.Send(new PacketWriter(packet).Write());
        }

        public static long Send(this UdpClient client, IPacket packet)
        {

            return client.Send(client.Endpoint, new PacketWriter(packet).Write());
        }

        public static T[] Slice<T>(this T[] source, int index, int length)
        {
            T[] slice = new T[length];
            Array.Copy(source, index, slice, 0, length);
            return slice;
        }
    }
}
