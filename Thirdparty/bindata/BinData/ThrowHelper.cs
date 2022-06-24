namespace BinData;

internal static class ThrowHelper
{
    public static void ThrowEndOfStreamException()
    {
        throw new EndOfStreamException("End of stream reached unexpectedly.");
    }
}
