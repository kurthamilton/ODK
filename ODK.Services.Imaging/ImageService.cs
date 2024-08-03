using ODK.Core.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace ODK.Services.Imaging;

public class ImageService : IImageService
{
    public byte[] Crop(byte[] data, int width, int height) => Crop(data, width, height, 0, 0);

    public byte[] Crop(byte[] data, int width, int height, int x, int y)
    {
        return ProcessImage(data, image =>
        {
            image.Mutate(context =>
            {
                var crop = new Rectangle(
                    x,
                    y,
                    width,
                    height);

                try
                {
                    context
                        .AutoOrient()
                        .Crop(crop);
                }
                catch
                {
                    // let the crop fail if the target size is smaller than the original
                }
            });
        });
    }

    public bool IsImage(byte[] data)
    {
        var image = TryLoadImage(data);
        return image != null;
    }

    public string? MimeType(byte[] data)
    {
        try
        {
            var imageInfo = Image.DetectFormat(data);
            return imageInfo.DefaultMimeType;
        }
        catch
        {
            return null;
        }
    }

    public byte[] Pad(byte[] data, int width, int height)
    {
        return ProcessImage(data, image =>
        {
            image.Mutate(context =>
            {
                try
                {
                    context
                        .AutoOrient()
                        .Pad(width, height, Color.Transparent);
                }
                catch
                {
                    // do nothing
                }
            });
        });
    }

    public byte[] Reduce(byte[] data, int maxWidth, int maxHeight)
    {
        return ProcessImage(data, image =>
        {
            if (image.Width <= maxWidth && image.Height <= maxHeight)
            {
                return;
            }

            RescaleImage(image, maxWidth, maxHeight);
        });
    }

    public byte[] Resize(byte[] data, int width, int height)
    {
        return ProcessImage(data, image =>
        {
            RescaleImage(image, width, height);
        });
    }

    public byte[] Rotate(byte[] data, int degrees)
    {
        return ProcessImage(data, image =>
        {
            image.Mutate(context =>
            {
                context
                    .AutoOrient()
                    .Rotate(degrees);
            });
        });
    }

    public ImageSize Size(byte[] data)
    {
        var image = TryLoadImage(data);
        return new ImageSize(image?.Width ?? 0, image?.Height ?? 0);
    }

    private static Size GetRescaledSize(Size current, Size maxSize, Func<double, double, double> chooseRatio)
    {
        double widthRatio = (double)maxSize.Width / current.Width;
        double heightRatio = (double)maxSize.Height / current.Height;

        double ratio = chooseRatio(widthRatio, heightRatio);

        return new Size((int)Math.Floor(current.Width * ratio), (int)Math.Floor(current.Height * ratio));
    }

    private static byte[] ImageToBytes(Image image, IImageFormat format)
    {
        using MemoryStream ms = new MemoryStream();
        image.Save(ms, format);
        return ms.ToArray();
    }

    private static byte[] ProcessImage(byte[] data, Action<Image> action)
    {            
        var imageInfo = Image.DetectFormat(data);
        using var image = Image.Load(data);
        action(image);
        return ImageToBytes(image, imageInfo);
    }

    private static void RescaleImage(Image image, int maxWidth, int maxHeight)
    {
        Size rescaled = GetRescaledSize(image.Size, new Size(maxWidth, maxHeight), Math.Min);
        image.Mutate(context =>
        {
            context
                .AutoOrient()
                .Resize(rescaled);
        });
    }

    private static Image? TryLoadImage(byte[] data)
    {
        try
        {
            var imageInfo = Image.DetectFormat(data);
            using var image = Image.Load(data);
            return image;
        }        
        catch
        {
            return null;
        }
    }
}
