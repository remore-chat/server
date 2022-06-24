namespace BinData.Benchmarks;

public sealed class PocoClass
{
    public int Int { get; set; }
    public bool Bool { get; set; }

    [NotSerialized]
    public long LongIgnore { get; set; }

    [Serialized]
    private short _shortExplicit;

    public PocoClass()
    {
        _shortExplicit = 0;
    }

    public PocoClass(short shortExplicit)
    {
        _shortExplicit = shortExplicit;
    }
}
