using ODK.Core.Images;
using ODK.Services.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace ODK.Services.Integrations.Imaging;

public class ImageService : IImageService
{
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

    public byte[] CropSquare(byte[] data)
    {
        var size = Size(data);
        if (size.Height == size.Width)
        {
            return data;
        }

        var (x, y) = (0, 0);
        int length;

        if (size.Height > size.Width)
        {
            y = (int)Math.Round((size.Height / 2.0) - (size.Width / 2.0), 0);
            length = size.Width;
        }
        else
        {
            x = (int)Math.Round((size.Width / 2.0) - (size.Height / 2.0), 0);
            length = size.Height;
        }

        return Crop(data, length, length, x, y);
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

    public byte[] Process(byte[] data, ImageProcessingOptions options)
    {
        return ProcessImage(data, image =>
        {
            var processed = false;

            var imageFormat = Image.DetectFormat(data);

            if (!string.IsNullOrEmpty(options.MimeType))
            {
                var converted = ConvertImage(image, options.MimeType);
                if (converted != null)
                {
                    image = Image.Load(converted);
                    imageFormat = Image.DetectFormat(converted);
                    processed = true;
                }
            }

            if (options.AspectRatio != null)
            {
                processed = PadImage(image, options.AspectRatio.Value) || processed;
            }

            if (options.MaxWidth != null)
            {
                ReduceImage(image, options.MaxWidth.Value, image.Size.Height);
            }

            return processed
                ? ImageToBytes(image, imageFormat)
                : data;
        });
    }

    public byte[] Reduce(byte[] data, int maxWidth, int maxHeight)
    {
        return ProcessImage(data, image =>
        {
            ReduceImage(image, maxWidth, maxHeight);
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

    private static byte[]? ConvertImage(Image image, string mimeType)
    {
        using var ms = new MemoryStream();
        switch (mimeType)
        {
            case "image/jpeg":
                image.SaveAsJpeg(ms);
                break;

            case "image/png":
                image.SaveAsPng(ms);
                break;

            case "image/webp":
                image.SaveAsWebp(ms);
                break;

            default:
                return null;
        }

        return ms.ToArray();
    }

    private static Size GetRescaledSize(Size current, Size maxSize, Func<double, double, double> chooseRatio)
    {
        double widthRatio = (double)maxSize.Width / current.Width;
        double heightRatio = (double)maxSize.Height / current.Height;

        double ratio = chooseRatio(widthRatio, heightRatio);

        return new Size(
            width: (int)Math.Floor(current.Width * ratio),
            height: (int)Math.Floor(current.Height * ratio));
    }

    private static byte[] ImageToBytes(Image image, IImageFormat format)
    {
        using MemoryStream ms = new MemoryStream();
        image.Save(ms, format);
        return ms.ToArray();
    }

    private static bool PadImage(Image image, decimal aspectRatio)
    {
        var size = image.Size;
        var currentAspectRatio = size.Width * 1.0M / size.Height;
        var width = currentAspectRatio >= aspectRatio
            ? size.Width
            : (int)Math.Ceiling(size.Height * aspectRatio);
        var height = currentAspectRatio <= aspectRatio
            ? size.Height
            : (int)Math.Ceiling(size.Width / aspectRatio);

        if (size.Width == width && size.Height == height)
        {
            return false;
        }

        PadImage(image, width, height);
        return true;
    }

    private static void PadImage(Image image, int width, int height)
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
    }

    private static byte[] ProcessImage(byte[] data, Action<Image> action)
    {
        return ProcessImage(data, image =>
        {
            var imageInfo = Image.DetectFormat(data);
            action(image);
            return ImageToBytes(image, imageInfo);
        });
    }

    private static byte[] ProcessImage(byte[] data, Func<Image, byte[]> action)
    {
        using var image = Image.Load(data);
        return action(image);
    }

    private static void ReduceImage(Image image, int maxWidth, int maxHeight)
    {
        if (image.Width <= maxWidth && image.Height <= maxHeight)
        {
            return;
        }

        RescaleImage(image, maxWidth, maxHeight);
    }

    private static void RescaleImage(Image image, int maxWidth, int maxHeight)
    {
        var rescaled = GetRescaledSize(image.Size, new Size(maxWidth, maxHeight), Math.Min);
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