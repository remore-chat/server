using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BinData;

[SkipLocalsInit]
internal static class BinaryStreamWriter
{
    private static ReadOnlySpan<byte> ZeroIntegerBytes => new byte[] { 0, 0, 0, 0 };

    public static unsafe void WriteEnum<TEnum>(TEnum value, Stream stream) where TEnum : unmanaged, Enum
    {
        if (sizeof(TEnum) == 1)
        {
            stream.WriteByte(Unsafe.As<TEnum, byte>(ref value));
            return;
        }

        Span<byte> buffer = stackalloc byte[sizeof(TEnum)];
        if (sizeof(TEnum) == 2)
        {
            short temp = Unsafe.As<TEnum, short>(ref value);
            BinaryPrimitives.WriteInt16LittleEndian(buffer, temp);
        }
        else if (sizeof(TEnum) == 4)
        {
            int temp = Unsafe.As<TEnum, int>(ref value);
            BinaryPrimitives.WriteInt32LittleEndian(buffer, temp);
        }
        else if (sizeof(TEnum) == 8)
        {
            long temp = Unsafe.As<TEnum, long>(ref value);
            BinaryPrimitives.WriteInt64LittleEndian(buffer, temp);
        }
        stream.Write(buffer);
    }

    public static unsafe void WritePrimitive<T>(T value, Stream stream) where T : unmanaged
    {
        if (sizeof(T) == sizeof(byte))
        {
            stream.WriteByte(Unsafe.As<T, byte>(ref value));
            return;
        }

        Span<byte> buffer = stackalloc byte[sizeof(T)];
        if (sizeof(T) == sizeof(ushort))
        {
            ushort temp = Unsafe.As<T, ushort>(ref value);
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, temp);
        }
        else if (sizeof(T) == sizeof(uint))
        {
            uint temp = Unsafe.As<T, uint>(ref value);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, temp);
        }
        else if (sizeof(T) == sizeof(ulong))
        {
            ulong temp = Unsafe.As<T, ulong>(ref value);
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, temp);
        }
        else if (sizeof(T) == sizeof(decimal))
        {
            decimal temp = Unsafe.As<T, decimal>(ref value);
            var bits = decimal.GetBits(temp);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(12, 4), bits[3]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[..4], bits[0]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(4, 4), bits[1]);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(8, 4), bits[2]);
        }
        stream.Write(buffer);
    }

    public static void WriteString(string value, Stream stream)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, bytes.Length);

        stream.Write(buffer);
        stream.Write(bytes);
    }

    public static unsafe void WriteStructure<T>(T value, Stream stream) where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[sizeof(T)];
        MemoryMarshal.Write(buffer, ref value);

        if (!BitConverter.IsLittleEndian)
        {
            buffer.Reverse();
        }

        stream.Write(buffer);
    }
}
