using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Remore.Library.Packets
{
    public interface IPacket
    {
        int Id { get; }
        string RequestId { get; set; }
    }
}
