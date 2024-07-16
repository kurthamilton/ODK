namespace ODK.Services.Imaging;

public interface IImageService
{
    byte[] Crop(byte[] data, int width, int height);

    bool IsImage(byte[] data);

    byte[] Reduce(byte[] data, int maxWidth, int maxHeight);

    byte[] Resize(byte[] data, int width, int height);

    byte[] Rotate(byte[] data, int degrees);
}
