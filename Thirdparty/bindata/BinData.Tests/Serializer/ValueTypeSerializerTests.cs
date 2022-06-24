using System.Runtime.CompilerServices;

namespace BinData.Tests.Serializer;

public class ValueTypeSerializerTests
{
    [Fact]
    public void SByteTest()
    {
        const sbyte value = 1;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x01 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ByteTest()
    {
        const byte value = 1;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x01 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Int16Test()
    {
        const short value = 1;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x01, 0x00 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void UInt16Test()
    {
        const ushort value = 1;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x01, 0x00 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Int32Test()
    {
        const int value = 1;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x01, 0x00, 0x00, 0x00 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void UInt32Test()
    {
        const uint value = 1;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x01, 0x00, 0x00, 0x00 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Int64Test()
    {
        const long value = 1;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void UInt64Test()
    {
        const ulong value = 1;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SingleTest()
    {
        const float value = 1.0f;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x00, 0x00, 0x80, 0x3f };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DoubleTest()
    {
        const double value = 1.0d;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xf0, 0x3f };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DecimalTest()
    {
        const decimal value = 1.0m;
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[]
        {
            0x0a, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00
        };

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false, new byte[] { 0x00 })]
    [InlineData(true, new byte[] { 0x01 })]
    public void BooleanTest(bool value, byte[] expected)
    {
        var actual = BinaryConvert.Serialize(value);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CharTest()
    {
        const char value = 'c';
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x63, 0x00 };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DateTimeTest()
    {
        var value = new DateTime(256, 8, 16, 0, 32, 4);
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[] { 0x00, 0x1a, 0x26, 0xaa, 0xc4, 0x95, 0x1e, 0x01 };

        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> GetValueTupleData()
    {
        yield return new object[]
        {
            new ValueTuple<bool>(true), new byte[] { 0x01 }
        };
        yield return new object[]
        {
            new ValueTuple<bool, bool>(true, true), new byte[] { 0x01, 0x01 }
        };
        yield return new object[]
        {
            new ValueTuple<bool, bool, bool>(true, true, true), new byte[] { 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new ValueTuple<bool, bool, bool, bool>(true, true, true, true),
            new byte[] { 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new ValueTuple<bool, bool, bool, bool, bool>(true, true, true, true, true),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new ValueTuple<bool, bool, bool, bool, bool, bool>(true, true, true, true, true, true),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new ValueTuple<bool, bool, bool, bool, bool, bool, bool>(true, true, true, true, true, true, true),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new ValueTuple<bool, bool, bool, bool, bool, bool, bool, ValueTuple<bool>>(true, true, true, true, true, true, true, new ValueTuple<bool>(false)),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00 }
        };
    }

    public static IEnumerable<object[]> GetTupleData()
    {
        yield return new object[]
        {
            new Tuple<bool>(true), new byte[] { 0x01, 0x01 }
        };
        yield return new object[]
        {
            new Tuple<bool, bool>(true, true), new byte[] { 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new Tuple<bool, bool, bool>(true, true, true), new byte[] { 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new Tuple<bool, bool, bool, bool>(true, true, true, true),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new Tuple<bool, bool, bool, bool, bool>(true, true, true, true, true),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new Tuple<bool, bool, bool, bool, bool, bool>(true, true, true, true, true, true),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new Tuple<bool, bool, bool, bool, bool, bool, bool>(true, true, true, true, true, true, true),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 }
        };
        yield return new object[]
        {
            new Tuple<bool, bool, bool, bool, bool, bool, bool, Tuple<bool>>(true, true, true, true, true, true, true, new Tuple<bool>(false)),
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00 }
        };
    }

    [Theory]
    [MemberData(nameof(GetValueTupleData))]
    [MemberData(nameof(GetTupleData))]
    public void TupleTest(ITuple value, byte[] expected)
    {
        var actual = BinaryConvert.Serialize(value);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void StringTest()
    {
        const string value = "Test String 😀";
        var actual = BinaryConvert.Serialize(value);
        var expected = new byte[]
        {
            0x01,
            0x10, 0x00, 0x00, 0x00,
            0x54, 0x65, 0x73, 0x74, 0x20,
            0x53, 0x74, 0x72, 0x69, 0x6e, 0x67, 0x20,
            0xf0, 0x9f, 0x98, 0x80
        };

        Assert.Equal(expected, actual);
    }
}
