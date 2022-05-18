using System;
using System.Collections.Generic;
using System.Text;

namespace TTalk.Library.Packets.Server
{
    public class NotificationPacket : IPacket
    {
        public NotificationPacket()
        {
        }

        public NotificationPacket(string message)
        {
            Message = message;
        }
        public int Id => 3;
        public string Message { get; set; }
        public string RequestId { get; set; }

    }
}
