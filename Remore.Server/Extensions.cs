using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Remore.Library.Packets;

namespace Remore.Server
{
    public static class Extensions
    {
        public static long Send(this TcpSession session, IPacket packet)
        {
            try
            {
                return session.Send(new PacketWriter(packet).Write());

            }
            catch (Exception ex)
            {

                Logger.LogError(ex.Message);
            }
            return 0;
        }

        public static long Send(this UdpServer server, EndPoint endpoint, IPacket packet)
        {
            try
            {
                return server.Send(endpoint, new PacketWriter(packet).Write());
            }
            catch (Exception ex)
            {

                Logger.LogError(ex.Message);
            }
            return 0;
        }


        public static bool Multicast(this TcpServer server, IPacket packet)
        {

            try
            {
                return server.Multicast(new PacketWriter(packet).Write());
            }
            catch (Exception ex)
            {

                Logger.LogError(ex.Message);
            }
            return false;
        }
    }
}
