using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Packets.Client
{
    public class RequestFilePacket : IPacket
    {
        public int Id => 48;
        public string FileId { get; set; }
        public string RequestId { get; set; }
    }
}
