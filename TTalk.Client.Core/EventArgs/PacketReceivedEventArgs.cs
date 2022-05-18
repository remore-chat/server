using TTalk.Library.Packets;

namespace TTalk.Client.Core.EventArgs
{
    public class PacketReceivedEventArgs
    {
        public IPacket Packet { get; set; }
    }
}