using TTalk.Library.Packets;

namespace TTalk.Client.ClientCode.EventArgs
{
    public class PacketReceivedEventArgs
    {
        public IPacket Packet { get; set; }
    }
}