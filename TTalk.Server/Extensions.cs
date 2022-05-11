using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TTalk.Library.Packets;

namespace TTalk.Server
{
    public static class Extensions
    {
        public static long Send(this TcpSession session, IPacket packet)
        {
            return session.Send(new PacketWriter(packet).Write());
        }

        public static long Send(this UdpServer server, EndPoint endpoint, IPacket packet)
        {
            return server.Send(endpoint, new PacketWriter(packet).Write());
        }
        

        public static bool Multicast(this TcpServer server, IPacket packet)
        {
            
            return server.Multicast(new PacketWriter(packet).Write());
        }
    }
}
