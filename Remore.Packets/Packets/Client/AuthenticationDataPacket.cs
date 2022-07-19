using System;
using System.Collections.Generic;
using System.Text;
using Remore.Library.Attributes;

namespace Remore.Library.Packets.Client
{
    [SourceGeneratorIgnorePacket]
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
