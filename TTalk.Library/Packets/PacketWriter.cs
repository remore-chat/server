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
                else if (prop.PropertyType.IsEnum)
                {
                    var enumUnderliying = Enum.GetUnderlyingType(prop.PropertyType);
                    if (enumUnderliying == typeof(int))
                        writer.Write((int)value);
                    else if (enumUnderliying == typeof(byte))
                        writer.Write((byte)value);
                    else
                        throw new InvalidOperationException("Invalid underlying type of enum (Supported: int, byte)");
                }
                else if (prop.PropertyType.IsGenericType && (prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var collection = (System.Collections.IEnumerable)value;
                    int count = 0;
                    var bytesAll = new List<byte>();
                    foreach (var val in collection)
                    {
                        using var mem = new MemoryStream();
                        new BinaryFormatter().Serialize(mem, val);
                        var bytes = mem.ToArray();
                        bytesAll.AddRange(BitConverter.GetBytes(bytes.Length));
                        bytesAll.AddRange(bytes);
                        count++;
                    }
                    writer.Write(count);
                    writer.Write(bytesAll.ToArray());
                    
                }
                else if (value is float)
                    writer.Write((float)value);
                else if (value is bool)
                    writer.Write((bool)value);
                else if (prop.PropertyType == typeof(string))
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
