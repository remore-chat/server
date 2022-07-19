using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Server
{
    public class ClientConnectedPacket : IPacket
    {
        public int Id => 6;
        public string ConnectedClientUsername { get; set; }
        public string RequestId { get; set; }


        public ClientConnectedPacket()
        {

        }

        public ClientConnectedPacket(string username)
        {
            ConnectedClientUsername = username;
        }
    }
}
