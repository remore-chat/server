using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using TTalk.Library.Packets;

namespace TTalk.Library.Packets
{
    public class PacketWriter
    {
        private IPacket _packet;
        public PacketWriter(IPacket packet)
        {
            _packet = packet;
        }

        public byte[] Write()
        {
            var writer = new ByteReaderWriter(_packet.Id);
            var properties = _packet.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.Name == "Id")
                    continue;
                var value = prop.GetValue(_packet);
                if (value is int)
                    writer.Write((int)value);
                else if (value is long)
                    writer.Write((long)value);
                else if (value is short)
                    writer.Write((short)value);
                else if (value is byte)
                    writer.Write((byte)value);
                else if (value is byte[])
                {
                    //NL - No Length
                    if (!prop.Name.StartsWith("NL"))
                        writer.Write(((byte[])value).Length);
                    writer.Write((byte[])value);
                }    
                else if (value is List<byte> list)
                {
                    writer.Write(list.Count());
                    writer.Write(list.ToArray());
                }
                else if (prop.PropertyType == typeof(List<string>))
                {
                    var _list = value as List<string>;
                    using var mem = new MemoryStream();
                    new BinaryFormatter().Serialize(mem, _list);
                    var bytes = mem.ToArray();
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }
                else if (value is float)
                    writer.Write((float)value);
                else if (value is bool)
                    writer.Write((bool)value);
                else if (value is string)
                    writer.Write((string)value ?? "");
                else
                    throw new InvalidOperationException($"Unknown property type {value.GetType().FullName}");
            }
            int length = writer.WriteLength();
            var bytesToSend = writer.ToArray();
            writer.Dispose();
            return bytesToSend!;
        }
    }
}
