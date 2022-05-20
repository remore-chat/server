using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BinData;
using TTalk.Library.Packets;
using TTalk.Library.Packets.Client;

namespace TTalk.Library.Packets
{
    public sealed class PacketReader
    {
        private byte[] _data;
        private static Dictionary<int, Type> _packets;


        public static void Init()
        {
            _packets = new Dictionary<int, Type>();
            _packets = Assembly.GetAssembly(typeof(IPacket)).GetTypes().Where(x => !x.IsInterface && typeof(IPacket).IsAssignableFrom(x))
               .ToDictionary(k => ((IPacket)Activator.CreateInstance(k)).Id, v => v);

        }

        public PacketReader(byte[] data)
        {
            if (data.Length < 4)
                throw new InvalidOperationException("Invalid packet received");
            _data = data;
        }

        public IPacket Read()
        {
            var length = BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(0, 4));
            var id = BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(4, 4));

            if (!_packets.TryGetValue(id, out var type))
                throw new Exception($"Unknown packet id received 0x{id}");

            if (BinaryConvert.Deserialize(_data.AsSpan(8, length).ToArray(), type) is not IPacket instance)
                throw new InvalidOperationException("Packet should not be null");

            return instance;
        }
    }
}
