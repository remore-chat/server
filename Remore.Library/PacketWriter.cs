using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BinData;
using Remore.Library.Packets;

namespace Remore.Library.Packets
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
            var packetData = BinaryConvert.Serialize(_packet);

            Span<byte> buf = stackalloc byte[8];
            BinaryPrimitives.WriteInt32LittleEndian(buf, packetData.Length + 4);
            BinaryPrimitives.WriteInt32LittleEndian(buf[4..], _packet.Id);

            var allData = new byte[packetData.Length + 8];
            buf.CopyTo(allData);  
            packetData.CopyTo(allData, 8);

            return allData;

            // var writer = new ByteReaderWriter(_packet.Id);
            // var properties = _packet.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            // foreach (var prop in properties)
            // {
            //     if (prop.Name == "Id")
            //         continue;
            //     var value = prop.GetValue(_packet);
            //     if (value is int)
            //         writer.Write((int)value);
            //     else if (value is long)
            //         writer.Write((long)value);
            //     else if (value is short)
            //         writer.Write((short)value);
            //     else if (value is byte)
            //         writer.Write((byte)value);
            //     else if (value is byte[])
            //     {
            //         //NL - No Length
            //         if (!prop.Name.StartsWith("NL"))
            //             writer.Write(((byte[])value).Length);
            //         writer.Write((byte[])value);
            //     }
            //     else if (value is List<byte> list)
            //     {
            //         writer.Write(list.Count());
            //         writer.Write(list.ToArray());
            //     }
            //     else if (prop.PropertyType.IsEnum)
            //     {
            //         var enumUnderliying = Enum.GetUnderlyingType(prop.PropertyType);
            //         if (enumUnderliying == typeof(int))
            //             writer.Write((int)value);
            //         else if (enumUnderliying == typeof(byte))
            //             writer.Write((byte)value);
            //         else
            //             throw new InvalidOperationException("Invalid underlying type of enum (Supported: int, byte)");
            //     }
            //     else if (prop.PropertyType.IsGenericType && (prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
            //     {
            //         var collection = (System.Collections.IEnumerable)value;
            //         int count = 0;
            //         using var listWriter = new ByteReaderWriter();
            //         foreach (var val in collection)
            //         {
            //             var json = JsonConvert.SerializeObject(val);
            //             listWriter.Write(json);
            //             count++;
            //         }
            //         listWriter.InsertInt(count);
            //         writer.Write(listWriter.ToArray());
            //
            //     }
            //     else if (value is float)
            //         writer.Write((float)value);
            //     else if (value is bool)
            //         writer.Write((bool)value);
            //     else if (prop.PropertyType == typeof(string))
            //         writer.Write((string)value ?? "");
            //     else
            //         throw new InvalidOperationException($"Unknown property type {value.GetType().FullName}");
            // }
            // int length = writer.WriteLength();
            // var bytesToSend = writer.ToArray();
            // writer.Dispose();
            // return bytesToSend!;
        }
    }
}
