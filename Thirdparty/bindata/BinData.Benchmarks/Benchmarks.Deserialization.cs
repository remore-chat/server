using BenchmarkDotNet.Attributes;

namespace BinData.Benchmarks;

public partial class Benchmarks
{
    public byte[] IntegerBytes = BinaryConvert.Serialize(0);
    public byte[] PocoBytes = BinaryConvert.Serialize(new PocoClass());
    public byte[] StringBytes = BinaryConvert.Serialize(new string('\0', 512));
    public byte[] ArrayBytes = BinaryConvert.Serialize(new byte[512]);
    public byte[] ListBytes = BinaryConvert.Serialize(new List<int>(Enumerable.Repeat(0, 512)));

    [Benchmark]
    public int DeserializeInt()
    {
        return BinaryConvert.Deserialize<int>(IntegerBytes);
    }

    [Benchmark]
    public string? DeserializeString()
    {
        return BinaryConvert.Deserialize<string>(StringBytes);
    }

    [Benchmark]
    public byte[]? DeserializeBytes()
    {
        return BinaryConvert.Deserialize<byte[]>(ArrayBytes);
    }

    [Benchmark]
    public PocoClass? DeserializePoco()
    {
        return BinaryConvert.Deserialize<PocoClass>(PocoBytes);
    }

    [Benchmark]
    public List<int>? DeserializeList()
    {
        return BinaryConvert.Deserialize<List<int>>(ListBytes);
    }
}
