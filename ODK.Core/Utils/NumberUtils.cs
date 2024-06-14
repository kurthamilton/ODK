namespace ODK.Core.Utils;

public static class NumberUtils
{
    public static int? FirstPositive(params int?[] numbers)
    {
        return numbers.FirstOrDefault(x => x > 0);
    }
}
