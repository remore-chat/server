using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BinData;

[SkipLocalsInit]
internal static class BinaryStreamReader
{
    public static unsafe TEnum ReadEnum<TEnum>(Stream stream) where TEnum : unmanaged, Enum
    {
        if (sizeof(TEnum) == 1)
        {
            int temp = stream.ReadByte();
            if (temp == -1)
                ThrowHelper.ThrowEndOfStreamException();
            return Unsafe.As<int, TEnum>(ref temp);
        }

        Span<byte> buffer = stackalloc byte[sizeof(TEnum)];
        if (stream.Read(buffer) < sizeof(TEnum))
            ThrowHelper.ThrowEndOfStreamException();

        if (sizeof(TEnum) == 2)
        {
            short temp = BinaryPrimitives.ReadInt16LittleEndian(buffer);
            return Unsafe.As<short, TEnum>(ref temp);
        }
        else if (sizeof(TEnum) == 4)
        {
            int temp = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            return Unsafe.As<int, TEnum>(ref temp);
        }
        else
        {
            long temp = BinaryPrimitives.ReadInt64LittleEndian(buffer);
            return Unsafe.As<long, TEnum>(ref temp);
        }
    }

    public static unsafe T ReadPrimitive<T>(Stream stream) where T : unmanaged
    {
        if (sizeof(T) == 1)
        {
            int temp = stream.ReadByte();
            if (temp == -1)
                ThrowHelper.ThrowEndOfStreamException();
            byte value = (byte)temp;
            return Unsafe.As<byte, T>(ref value);
        }

        Span<byte> buffer = stackalloc byte[sizeof(T)];
        if (stream.Read(buffer) < sizeof(T))
            ThrowHelper.ThrowEndOfStreamException();

        if (sizeof(T) == 2)
        {
            short temp = BinaryPrimitives.ReadInt16LittleEndian(buffer);
            return Unsafe.As<short, T>(ref temp);
        }
        else if (sizeof(T) == 4)
        {
            int temp = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            return Unsafe.As<int, T>(ref temp);
        }
        else if (sizeof(T) == 8)
        {
            long temp = BinaryPrimitives.ReadInt64LittleEndian(buffer);
            return Unsafe.As<long, T>(ref temp);
        }
        else
        {
            if (!BitConverter.IsLittleEndian)
            {
                buffer[12..16].Reverse();
                buffer[8..12].Reverse();
                buffer[4..8].Reverse();
                buffer[..4].Reverse();
            }
            decimal temp = new(MemoryMarshal.CreateSpan(ref Unsafe.As<byte, int>(ref MemoryMarshal.GetReference(buffer)), 4));
            return Unsafe.As<decimal, T>(ref temp);
        }
    }

    public static int ReadInt(Stream stream)
    {
        return ReadPrimitive<int>(stream);
    }

    public static string ReadString(Stream stream)
    {
        return Encoding.UTF8.GetString(ReadBytes(stream));
    }

    public static byte[] ReadBytes(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        if (stream.Read(buffer) < sizeof(int))
            ThrowHelper.ThrowEndOfStreamException();

        int length = BinaryPrimitives.ReadInt32LittleEndian(buffer);

        var bytes = new byte[length];
        if (stream.Read(bytes) < length)
            ThrowHelper.ThrowEndOfStreamException();

        return bytes;
    }

    public static unsafe T ReadStructure<T>(Stream stream) where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[sizeof(T)];
        if (stream.Read(buffer) < sizeof(T))
            ThrowHelper.ThrowEndOfStreamException();

        if (!BitConverter.IsLittleEndian)
        {
            buffer.Reverse();
        }

        return MemoryMarshal.Read<T>(buffer);
    }
}
