namespace ODK.Core.Images;

public struct ImageSize
{
    public ImageSize()
    {
    }

    public ImageSize(int width, int height)
    {
        Height = height;
        Width = width;
    }

    public decimal AspectRatio => (Width * 1.0M) / Height;

    public int Height { get; set; }

    public int Width { get; set; }
}
