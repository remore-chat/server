namespace BinData.Tests.Serializer;

public class ClassSerializerTests
{
    [Fact]
    public void PocoClassTest()
    {
        var value = new PocoClass(42)
        {
            Int = 420,
            Bool = true,
            LongIgnore = 25565
        };

        var actual = BinaryConvert.Serialize(value);

        var expected = new byte[]
        {
            0x01,
            0xa4, 0x01, 0x00, 0x00,
            0x01,
            0x2a, 0x00
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ComplexClassTest()
    {
        var value = new ComplexClass
        {
            NormalInt = 10,
            NullableInt = null,
            NormalString = "test",
            EmptyString = "",
            NullableString = null,
            InnerClass = new PocoClass(10)
            {
                Int = 10,
                Bool = true,
                LongIgnore = 420
            },
            NullableInnerClass = null,
            ByteList = { 0x01, 0x02 },
            ByteArray = new byte[] { 0x03, 0x04 },
            Enum = ClassTestEnum.Value1
        };

        var actual = BinaryConvert.Serialize(value);

        var expected = new byte[]
        {
            0x01,
            0x0a, 0x00, 0x00, 0x00,
            0x00,
            0x01, 0x04, 0x00, 0x00, 0x00, 0x74, 0x65, 0x73, 0x74,
            0x01, 0x00, 0x00, 0x00, 0x00,
            0x00,
            0x01,
                0x0a, 0x00, 0x00, 0x00,
                0x01,
                0x0a, 0x00,
            0x00,
            0x01, 0x02, 0x00, 0x00, 0x00, 0x01, 0x02,
            0x01, 0x02, 0x00, 0x00, 0x00, 0x03, 0x04,
            0x01, 0x00, 0x00, 0x00
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LinkedClassTest()
    {
        var value = new LinkedClass
        {
            Link = new LinkedClass
            {
                Link = new LinkedClass
                {
                    Link = null
                }
            }
        };

        var actual = BinaryConvert.Serialize(value);

        var expected = new byte[]
        {
            0x01,
            0x01,
            0x01,
            0x00
        };

        Assert.Equal(expected, actual);
    }
}

public class LinkedClass
{
    public LinkedClass? Link { get; set; }
}

public class PocoClass
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

public class ComplexClass
{
    public int NormalInt { get; set; }
    public int? NullableInt { get; set; }
    public string NormalString { get; set; }
    public string EmptyString { get; set; }
    public string? NullableString { get; set; }
    public PocoClass InnerClass { get; set; }
    public PocoClass? NullableInnerClass { get; set; }
    public List<byte> ByteList { get; set; } = new();
    public byte[] ByteArray { get; set; } = Array.Empty<byte>();
    public ClassTestEnum Enum { get; set; }
}

public enum ClassTestEnum
{
    Value0,
    Value1
}
