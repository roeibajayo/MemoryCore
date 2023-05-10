namespace MemoryCore.Utils;
internal static class DateTimeUtils
{
#if NET6_0_OR_GREATER
    public static long Now => Environment.TickCount64;
#else
    public static long Now => Environment.TickCount;
#endif
}