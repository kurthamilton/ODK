using ODK.Core.Images;
using ODK.Services.Exceptions;
using ODK.Services.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ODK.Services.Integrations.Imaging;

public class ImageService : IImageService
{
    private readonly ImageServiceSettings _settings;

    public ImageService(ImageServiceSettings settings)
    {
        _settings = settings;
    }

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
        try
        {
            using var image = LoadImage(data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? MimeType(byte[] data)
    {
        try
        {
            return Image.DetectFormat(data).DefaultMimeType;
        }
        catch
        {
            return null;
        }
    }

    public byte[] Pad(byte[] data, int width, int height)
    {
        return ProcessImage(data, image => PadImage(image, width, height));
    }

    public byte[] Process(byte[] data, ImageProcessingOptions options)
    {
        return ProcessImage(data, image =>
        {
            var processed = false;

            var imageFormat = Image.DetectFormat(data);

            /* A requested mime type only chooses the format the single encode at the end writes. Converting
               up front would mean encoding to bytes and immediately decoding them again, so the operations
               below run on the image that was decoded once, whatever format it is going to be saved as. */
            if (!string.IsNullOrEmpty(options.MimeType))
            {
                var targetFormat = TryFindFormat(options.MimeType);
                if (targetFormat != null)
                {
                    imageFormat = targetFormat;
                    processed = true;
                }
            }

            if (options.AspectRatio != null)
            {
                processed = PadImage(image, options.AspectRatio.Value) || processed;
            }

            if (options.MaxWidth != null)
            {
                processed = ReduceImage(image, options.MaxWidth.Value, image.Size.Height) || processed;
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
        var info = TryIdentify(data);
        return new ImageSize(info?.Width ?? 0, info?.Height ?? 0);
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

    private static bool ReduceImage(Image image, int maxWidth, int maxHeight)
    {
        if (image.Width <= maxWidth && image.Height <= maxHeight)
        {
            return false;
        }

        RescaleImage(image, maxWidth, maxHeight);
        return true;
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

    // The formats an image may be converted to. A mime type outside this set leaves the format unchanged.
    private static IImageFormat? TryFindFormat(string mimeType) => mimeType switch
    {
        "image/jpeg" => JpegFormat.Instance,
        "image/png" => PngFormat.Instance,
        "image/webp" => WebpFormat.Instance,
        _ => null
    };

    private static ImageInfo? TryIdentify(byte[] data)
    {
        try
        {
            return Image.Identify(data);
        }
        catch
        {
            return null;
        }
    }

    /* Every decode goes through here, so a new entry point cannot bypass the size cap. The cap counts pixels
       rather than encoded bytes because pixels are what allocates: a decoded frame costs several bytes per
       pixel however small the file carrying it compresses to, so a few megabytes of input can ask for
       hundreds of megabytes of output. */
    private Image LoadImage(byte[] data)
    {
        var info = Image.Identify(data);

        var pixels = (long)info.Width * info.Height;
        if (pixels > _settings.MaxPixels)
        {
            throw new OdkServiceException(
                $"Image is too large to process: {info.Width}x{info.Height} exceeds {_settings.MaxPixels} pixels");
        }

        return Image.Load(data);
    }

    private byte[] ProcessImage(byte[] data, Action<Image> action)
    {
        return ProcessImage(data, image =>
        {
            var imageInfo = Image.DetectFormat(data);
            action(image);
            return ImageToBytes(image, imageInfo);
        });
    }

    private byte[] ProcessImage(byte[] data, Func<Image, byte[]> action)
    {
        using var image = LoadImage(data);
        return action(image);
    }
}
