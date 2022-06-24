using System.Runtime.CompilerServices;

namespace BinData.Tests.Deserializer;

public class ValueTypeDeserializerTests
{
    public static IEnumerable<object[]> GetObjectValueTypeData()
    {
        yield return new object[] { new byte[] { 0x01 }, (sbyte)1 };
        yield return new object[] { new byte[] { 0x01 }, (byte)1 };
        yield return new object[] { new byte[] { 0x01, 0x00 }, (short)1 };
        yield return new object[] { new byte[] { 0x01, 0x00 }, (ushort)1 };
        yield return new object[] { new byte[] { 0x01, 0x00, 0x00, 0x00 }, 1 };
        yield return new object[] { new byte[] { 0x01, 0x00, 0x00, 0x00 }, (uint)1 };
        yield return new object[] { new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, (long)1 };
        yield return new object[] { new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, (ulong)1 };
        yield return new object[] { new byte[] { 0x00, 0x00, 0x80, 0x3f }, 1.0f };
        yield return new object[] { new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xf0, 0x3f }, 1.0d };
        yield return new object[] { new byte[] { 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00 }, 1.0m };
        yield return new object[] { new byte[] { 0x00 }, false };
        yield return new object[] { new byte[] { 0x01 }, true };
        yield return new object[] { new byte[] { 0x63, 0x00 }, 'c' };
        yield return new object[] { new byte[] { 0x01, 0x10, 0x00, 0x00, 0x00, 0x54, 0x65, 0x73, 0x74, 0x20, 0x53, 0x74, 0x72, 0x69, 0x6e, 0x67, 0x20, 0xf0, 0x9f, 0x98, 0x80 }, "Test String 😀" };
    }

    [Theory]
    [MemberData(nameof(GetObjectValueTypeData))]
    public void ObjectValueTypeDataTest(byte[] value, object expected)
    {
        var actual = BinaryConvert.Deserialize(value, expected.GetType());

        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> GetObjectValueTupleData()
    {
        yield return new object[]
        {
            new ValueTuple<bool>(true), new byte[] { 0x01 }
        };
        yield return new object[]
        {
            new ValueTuple<bool, bool>(true, true), new byte[] { 0x01, 0x01}
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
            new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00}
        };
    }

    public static IEnumerable<object[]> GetObjectTupleData()
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
    [MemberData(nameof(GetObjectValueTupleData))]
    [MemberData(nameof(GetObjectTupleData))]
    public void ObjectTupleDataTest(ITuple expected, byte[] value)
    {
        var actual = BinaryConvert.Deserialize(value, expected.GetType());

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedInt32Test()
    {
        var value = new byte[] { 0x01, 0x00, 0x00, 0x00 };

        var actual = BinaryConvert.Deserialize<int>(value);

        const int expected = 1;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedSingleTest()
    {
        var value = new byte[] { 0x00, 0x00, 0x80, 0x3f };

        var actual = BinaryConvert.Deserialize<float>(value);

        const float expected = 1.0f;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedBooleanTest()
    {
        var value = new byte[] { 0x01 };

        var actual = BinaryConvert.Deserialize<bool>(value);

        const bool expected = true;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedCharTest()
    {
        var value = new byte[] { 0x63, 0x00 };
        var actual = BinaryConvert.Deserialize<char>(value);
        const char expected = 'c';
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedDateTimeTest()
    {
        var value = new byte[] { 0x00, 0x1a, 0x26, 0xaa, 0xc4, 0x95, 0x1e, 0x01 };
        var actual = BinaryConvert.Deserialize<DateTime>(value);
        var expected = new DateTime(256, 8, 16, 0, 32, 4);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedStringTest()
    {
        var value = new byte[]
        {
            0x01, 0x10, 0x00, 0x00, 0x00, 0x54, 0x65, 0x73, 0x74, 0x20, 0x53, 0x74, 0x72, 0x69, 0x6e, 0x67, 0x20, 0xf0,
            0x9f, 0x98, 0x80
        };
        var actual = BinaryConvert.Deserialize<string>(value);
        const string expected = "Test String 😀";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedValueTupleTest()
    {
        var value = new byte[] { 0x01, 0x00 };
        var actual = BinaryConvert.Deserialize<ValueTuple<bool, bool>>(value);
        var expected = new ValueTuple<bool, bool>(true, false);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedTupleTest()
    {
        var value = new byte[] { 0x01, 0x01, 0x00 };
        var actual = BinaryConvert.Deserialize<Tuple<bool, bool>>(value);
        var expected = new Tuple<bool, bool>(true, false);
        Assert.Equal(expected, actual);
    }
}
