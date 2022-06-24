namespace BinData.Tests.Deserializer;

public class EnumerableDeserializerTests
{
    [Fact]
    public void ObjectIntListTest()
    {
        var value = new byte[]
        {
            0x01, 0x05, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00
        };

        var actual = BinaryConvert.Deserialize(value, typeof(List<int>));

        var expected = new List<int>
        {
            1, 2, 4, 8, 16
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ObjectIntArrayTest()
    {
        var value = new byte[]
        {
            0x01, 0x05, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00
        };

        var actual = BinaryConvert.Deserialize(value, typeof(int[]));

        var expected = new[]
        {
            1, 2, 4, 8, 16
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ObjectIntIEnumerableTest()
    {
        var value = new byte[]
        {
            0x01, 0x05, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00
        };

        var actual = BinaryConvert.Deserialize(value, typeof(IEnumerable<int>));

        IEnumerable<int> expected = new List<int>
        {
            1, 2, 4, 8, 16
        }.AsEnumerable();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedIntListTest()
    {
        var value = new byte[]
        {
            0x01, 0x05, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00
        };

        var actual = BinaryConvert.Deserialize<List<int>>(value);

        var expected = new List<int>
        {
            1, 2, 4, 8, 16
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedIntArrayTest()
    {
        var value = new byte[]
        {
            0x01, 0x05, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00
        };

        var actual = BinaryConvert.Deserialize<int[]>(value);

        var expected = new[]
        {
            1, 2, 4, 8, 16
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TypedIntIEnumerableTest()
    {
        var value = new byte[]
        {
            0x01, 0x05, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00
        };

        var actual = BinaryConvert.Deserialize<IEnumerable<int>>(value);

        IEnumerable<int> expected = new List<int>
        {
            1, 2, 4, 8, 16
        }.AsEnumerable();

        Assert.Equal(expected, actual);
    }
}
