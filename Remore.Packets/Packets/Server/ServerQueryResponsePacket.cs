using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Server
{
    public class ServerQueryResponsePacket : IPacket
    {
        public int Id => 33;
        public string ServerName { get; set; }
        public string ServerVersion { get; set; }
        public int ClientsConnected { get; set; }
        public int MaxClients { get; set; }
        public string RequestId { get; set; }

    }
}
