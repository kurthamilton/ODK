using ODK.Core.Images;

namespace ODK.Services.Imaging;

public interface IImageService
{
    byte[] Crop(byte[] data, int width, int height);

    byte[] Crop(byte[] data, int width, int height, int x, int y);

    bool IsImage(byte[] data);

    string? MimeType(byte[] data);

    byte[] Pad(byte[] data, int width, int height);

    byte[] Process(byte[] data, ImageProcessingOptions options);

    byte[] Reduce(byte[] data, int maxWidth, int maxHeight);

    byte[] Resize(byte[] data, int width, int height);

    byte[] Rotate(byte[] data, int degrees);

    ImageSize Size(byte[] data);
}
