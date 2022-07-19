using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.Library.Packets.Client
{
    public class AuthenticationDataPacket : IPacket
    {
        public int Id => 5;
        public string Username { get; set; }
        public string ServerPrivilegeKey { get; set; }
        public string RequestId { get; set; }

        public AuthenticationDataPacket()
        {

        }

        public AuthenticationDataPacket(string username, string serverPrivilegeKey)
        {
            Username = username;
            ServerPrivilegeKey = serverPrivilegeKey;
        }
    }
}
