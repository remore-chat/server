using BenchmarkDotNet.Attributes;

namespace BinData.Benchmarks;

[MemoryDiagnoser]
public partial class Benchmarks
{
    public int Integer;
    public PocoClass Poco = new PocoClass();
    public string String = new('\0', 512);
    public byte[] Bytes = new byte[512];
    public List<int> Integers = new(Enumerable.Repeat(0, 512));

    [Benchmark]
    public byte[] SerializeInt()
    {
        return BinaryConvert.Serialize(Integer);
    }

    [Benchmark]
    public byte[] SerializeString()
    {
        return BinaryConvert.Serialize(String);
    }

    [Benchmark]
    public byte[] SerializeBytes()
    {
        return BinaryConvert.Serialize(Bytes);
    }

    [Benchmark]
    public byte[] SerializePoco()
    {
        return BinaryConvert.Serialize(Poco);
    }

    [Benchmark]
    public byte[] SerializeList()
    {
        return BinaryConvert.Serialize(Integers);
    }
}
