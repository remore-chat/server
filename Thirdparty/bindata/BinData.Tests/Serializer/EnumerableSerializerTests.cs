namespace BinData.Tests.Serializer;

public class EnumerableSerializerTests
{
    [Fact]
    public void IntListTest()
    {
        var value = new List<int>
        {
            1, 2, 4, 8, 16
        };

        var actual = BinaryConvert.Serialize(value);

        var expected = new byte[]
        {
            0x01,
            0x05, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x00, 0x00,
            0x08, 0x00, 0x00, 0x00,
            0x10, 0x00, 0x00, 0x00
        };

        Assert.Equal(expected, actual);
    }
}
