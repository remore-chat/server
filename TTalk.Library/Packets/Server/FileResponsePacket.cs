using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Library.Packets.Server
{
    public class FileResponsePacket : IPacket
    {
        public int Id => 47;
        public string RequestId { get; set; }
        public string Error { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
    }
}
