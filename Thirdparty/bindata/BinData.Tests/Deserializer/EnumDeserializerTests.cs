namespace BinData.Tests.Deserializer;

public class EnumDeserializerTests
{
    public enum BasicEnum
    {
        ValueNegative = -1,
        Default = 0,
        ValuePositive = 1
    }

    public enum LongEnum : long
    {
        ValueNegative = -2147483649,
        Default = 0,
        ValuePositive = 2147483648
    }

    [Flags]
    public enum FlagsEnum : byte
    {
        None = 0,
        First = 1 << 0,
        Second = 1 << 1,
        Third = 1 << 2,
        Fourth = 1 << 3,
        Even = Second | Fourth,
        Uneven = First | Third,
        All = Even | Uneven
    }

    [Theory]
    [InlineData(BasicEnum.ValueNegative, new byte[] { 0xff, 0xff, 0xff, 0xff })]
    [InlineData(BasicEnum.Default, new byte[] { 0x00, 0x00, 0x00, 0x00 })]
    [InlineData(BasicEnum.ValuePositive, new byte[] { 0x01, 0x00, 0x00, 0x00 })]
    [InlineData(LongEnum.ValueNegative, new byte[] { 0xff, 0xff, 0xff, 0x7f, 0xff, 0xff, 0xff, 0xff })]
    [InlineData(LongEnum.Default, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]
    [InlineData(LongEnum.ValuePositive, new byte[] { 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 })]
    [InlineData(FlagsEnum.None, new byte[] { 0b0 })]
    [InlineData(FlagsEnum.First, new byte[] { 0b1 })]
    [InlineData(FlagsEnum.Second, new byte[] { 0b10 })]
    [InlineData(FlagsEnum.Third, new byte[] { 0b100 })]
    [InlineData(FlagsEnum.Fourth, new byte[] { 0b1000 })]
    [InlineData(FlagsEnum.Even, new byte[] { 0b1010 })]
    [InlineData(FlagsEnum.Uneven, new byte[] { 0b0101 })]
    [InlineData(FlagsEnum.All, new byte[] { 0b1111 })]
    public void ObjectEnumTest(Enum expected, byte[] value)
    {
        var actual = BinaryConvert.Deserialize(value, expected.GetType());
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(BasicEnum.ValueNegative, new byte[] { 0xff, 0xff, 0xff, 0xff })]
    [InlineData(BasicEnum.Default, new byte[] { 0x00, 0x00, 0x00, 0x00 })]
    [InlineData(BasicEnum.ValuePositive, new byte[] { 0x01, 0x00, 0x00, 0x00 })]
    public void TypedBasicEnumTest(BasicEnum expected, byte[] value)
    {
        var actual = BinaryConvert.Deserialize<BasicEnum>(value);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(LongEnum.ValueNegative, new byte[] { 0xff, 0xff, 0xff, 0x7f, 0xff, 0xff, 0xff, 0xff })]
    [InlineData(LongEnum.Default, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]
    [InlineData(LongEnum.ValuePositive, new byte[] { 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00 })]
    public void LongEnumTest(LongEnum expected, byte[] value)
    {
        var actual = BinaryConvert.Deserialize<LongEnum>(value);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(FlagsEnum.None, new byte[] { 0b0 })]
    [InlineData(FlagsEnum.First, new byte[] { 0b1 })]
    [InlineData(FlagsEnum.Second, new byte[] { 0b10 })]
    [InlineData(FlagsEnum.Third, new byte[] { 0b100 })]
    [InlineData(FlagsEnum.Fourth, new byte[] { 0b1000 })]
    [InlineData(FlagsEnum.Even, new byte[] { 0b1010 })]
    [InlineData(FlagsEnum.Uneven, new byte[] { 0b0101 })]
    [InlineData(FlagsEnum.All, new byte[] { 0b1111 })]
    public void FlagsEnumTest(FlagsEnum expected, byte[] value)
    {
        var actual = BinaryConvert.Deserialize<FlagsEnum>(value);
        Assert.Equal(expected, actual);
    }
}
