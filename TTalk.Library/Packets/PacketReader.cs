using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
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
        public int Id { get; set; }

        public IPacket Read()
        {

            var reader = new ByteReaderWriter(_data);
            //Ignore length of packet
            int _length = reader.ReadInt();
            var id = reader.ReadInt();
            if (!_packets.TryGetValue(id, out var type))
                throw new Exception($"Unknown packet id received 0x{id}");

            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var instance = Activator.CreateInstance(type);
            foreach (var prop in properties)
            {
                if (prop.Name == "Id")
                    continue;
                if (prop.Name[0] == '_')
                    continue;
                if (prop.PropertyType == typeof(int))
                {
                    if (prop.Name.StartsWith("NL"))
                        continue;
                    prop.SetValue(instance, reader.ReadInt());
                }
                else if (prop.PropertyType == typeof(long))
                    prop.SetValue(instance, reader.ReadLong());
                else if (prop.PropertyType == typeof(short))
                    prop.SetValue(instance, reader.ReadShort());
                else if (prop.PropertyType == typeof(byte))
                    prop.SetValue(instance, reader.ReadByte());
                else if (prop.PropertyType == typeof(byte[]))
                {
                    var length = reader.ReadInt();
                    prop.SetValue(instance, reader.ReadBytes(length));
                }
                else if (prop.PropertyType == typeof(List<string>))
                {
                    var length = reader.ReadInt();
                    var bytes = reader.ReadBytes(length);
                    using var mem = new MemoryStream(bytes, false);
                    prop.SetValue(instance, new BinaryFormatter().Deserialize(mem));
                }
                else if (prop.PropertyType == typeof(float))
                    prop.SetValue(instance, reader.ReadFloat());
                else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(instance, reader.ReadBool());
                else if (prop.PropertyType == typeof(string))
                    prop.SetValue(instance, reader.ReadString());

                else
                    throw new InvalidOperationException($"Unknown property type {prop.PropertyType.GetType().FullName}");
            }
            reader.Dispose();            
            return instance as IPacket;
        }
    }
}
