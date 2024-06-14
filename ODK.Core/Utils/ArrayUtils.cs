namespace ODK.Core.Utils;

public static class ArrayUtils
{
    public static T[][] Segment<T>(this T[] items, int segmentSize)
    {
        List<List<T>> segments = new List<List<T>>();

        if (segmentSize <= 0)
        {
            segmentSize = 1;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (i % segmentSize == 0)
            {
                segments.Add(new List<T>());
            }

            segments.Last().Add(items[i]);
        }

        return segments
            .Select(x => x.ToArray())
            .ToArray();
    }
}
