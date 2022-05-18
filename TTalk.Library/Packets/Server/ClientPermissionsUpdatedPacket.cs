using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Server
{
    public class ClientPermissionsUpdatedPacket : IPacket
    {
        public int Id => 35;
        public bool HasAllPermissions { get; set; }
        //TODO: List of permissions
        public string RequestId { get; set; }

    }
}
