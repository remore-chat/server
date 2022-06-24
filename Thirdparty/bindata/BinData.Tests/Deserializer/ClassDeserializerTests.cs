using System.Diagnostics.CodeAnalysis;

namespace BinData.Tests.Deserializer;

public class ClassDeserializerTests
{
    [Fact]
    public void ObjectPocoClassTest()
    {
        var value = new byte[]
        {
            0x01,
            0xa4, 0x01, 0x00, 0x00,
            0x01,
            0x2a, 0x00
        };

        var actual = BinaryConvert.Deserialize(value, typeof(PocoClass));

        var expected = new PocoClass(42)
        {
            Int = 420,
            Bool = true,
            LongIgnore = 25565
        };

        Assert.IsType<PocoClass>(actual);
        var typed = (PocoClass)actual!;

        Assert.Equal(expected.Int, typed.Int);
        Assert.Equal(expected.Bool, typed.Bool);
        Assert.Equal(expected.ShortExplicit, typed.ShortExplicit);
    }

    [Fact]
    public void ObjectComplexClassTest()
    {
        var value = new byte[]
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

        var actual = BinaryConvert.Deserialize(value, typeof(ComplexClass));

        var expected = new ComplexClass
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

        Assert.IsType<ComplexClass>(actual);
        var typed = (ComplexClass)actual!;

        Assert.Equal(expected.NormalInt, typed.NormalInt);
        Assert.Null(typed.NullableInt);
        Assert.Equal(expected.NormalString, typed.NormalString);
        Assert.Equal(expected.EmptyString, typed.EmptyString);
        Assert.Null(typed.NullableString);
        Assert.Null(typed.NullableInnerClass);
        Assert.Equal(expected.ByteList, typed.ByteList);
        Assert.Equal(expected.ByteArray, typed.ByteArray);
        Assert.Equal(expected.Enum, typed.Enum);

        Assert.IsType<PocoClass>(typed.InnerClass);

        Assert.Equal(expected.InnerClass.Int, typed.InnerClass.Int);
        Assert.Equal(expected.InnerClass.Bool, typed.InnerClass.Bool);
        Assert.Equal(expected.InnerClass.ShortExplicit, typed.InnerClass.ShortExplicit);
    }

    [Fact]
    public void TypedPocoClassTest()
    {
        var value = new byte[]
        {
            0x01,
            0xa4, 0x01, 0x00, 0x00,
            0x01,
            0x2a, 0x00
        };

        var actual = BinaryConvert.Deserialize<PocoClass>(value);

        var expected = new PocoClass(42)
        {
            Int = 420,
            Bool = true,
            LongIgnore = 25565
        };

        Assert.NotNull(actual);
        Assert.Equal(expected.Int, actual!.Int);
        Assert.Equal(expected.Bool, actual.Bool);
        Assert.Equal(expected.ShortExplicit, actual.ShortExplicit);
    }

    [Fact]
    public void TypedComplexClassTest()
    {
        var value = new byte[]
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

        var actual = BinaryConvert.Deserialize<ComplexClass>(value);

        var expected = new ComplexClass
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

        Assert.IsType<ComplexClass>(actual);
        var typed = (ComplexClass)actual!;

        Assert.Equal(expected.NormalInt, typed.NormalInt);
        Assert.Null(typed.NullableInt);
        Assert.Equal(expected.NormalString, typed.NormalString);
        Assert.Equal(expected.EmptyString, typed.EmptyString);
        Assert.Null(typed.NullableString);
        Assert.Null(typed.NullableInnerClass);
        Assert.Equal(expected.ByteList, typed.ByteList);
        Assert.Equal(expected.ByteArray, typed.ByteArray);
        Assert.Equal(expected.Enum, typed.Enum);

        Assert.IsType<PocoClass>(typed.InnerClass);

        Assert.Equal(expected.InnerClass.Int, typed.InnerClass.Int);
        Assert.Equal(expected.InnerClass.Bool, typed.InnerClass.Bool);
        Assert.Equal(expected.InnerClass.ShortExplicit, typed.InnerClass.ShortExplicit);
    }

    [Fact]
    public void LinkedClassTest()
    {
        var value = new byte[]
        {
            0x01,
            0x01,
            0x01,
            0x00
        };

        var actual = BinaryConvert.Deserialize<LinkedClass>(value);

        var expected = new LinkedClass
        {
            Link = new LinkedClass
            {
                Link = new LinkedClass
                {
                    Link = null
                }
            }
        };

        Assert.Equal(expected, actual, LinkedClass.Comparer);
    }
}

public class LinkedClass
{
    public static IEqualityComparer<LinkedClass?> Comparer => new LinkedClassComparer();

    public LinkedClass? Link { get; init; }

    private class LinkedClassComparer : IEqualityComparer<LinkedClass?>
    {
        public bool Equals(LinkedClass? x, LinkedClass? y)
        {
            return (x is null) == (y is null);
        }

        public int GetHashCode([DisallowNull] LinkedClass obj)
        {
            return default;
        }
    }
}

public class PocoClass
{
    public int Int { get; set; }
    public bool Bool { get; set; }

    [NotSerialized]
    public long LongIgnore { get; set; }

    [Serialized]
    internal short ShortExplicit;

    public PocoClass()
    {
        ShortExplicit = 0;
    }

    public PocoClass(short shortExplicit)
    {
        ShortExplicit = shortExplicit;
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
