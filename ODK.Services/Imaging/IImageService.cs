namespace ODK.Services.Imaging
{
    public interface IImageService
    {
        byte[] Reduce(byte[] data, int maxWidth, int maxHeight);

        byte[] Resize(byte[] data, int width, int height);
    }
}
