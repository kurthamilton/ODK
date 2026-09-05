using FluentAssertions;
using ODK.Services.Exceptions;
using ODK.Services.Imaging;
using ODK.Services.Integrations.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace ODK.Services.Integrations.Tests.Imaging;

[Parallelizable]
public static class ImageServiceTests
{
    // Well clear of every image these tests create, so only the tests that set their own cap hit it.
    private const int MaxPixels = 1_000_000;

    private static readonly IImageFormat[] SupportedFormats =
    [
        GifFormat.Instance,
        JpegFormat.Instance,
        PngFormat.Instance,
        WebpFormat.Instance
    ];

    [Test]
    public static void IsImage_ExceedsMaxPixels_ReturnsFalse()
    {
        /* Arrange - the cap is what stops a small file asking for a huge allocation, and it has to be
           reachable through the validation call, so an oversized upload is refused rather than thrown. */
        var service = CreateService(maxPixels: 100);
        var data = CreateImage(50, 50);

        // Act
        var result = service.IsImage(data);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsImage_NotAnImage_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.IsImage([1, 2, 3]);

        // Assert
        result.Should().BeFalse();
    }

    [TestCaseSource(nameof(SupportedFormats))]
    public static void IsImage_SupportedFormat_ReturnsTrue(IImageFormat format)
    {
        // Arrange
        var service = CreateService();
        var data = CreateImage(50, 50, format);

        // Act
        var result = service.IsImage(data);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void IsImage_UnsupportedFormat_ReturnsFalse()
    {
        /* Arrange - the service registers a decoder per format it accepts, so a format outside that set is
           not recognised as an image at all. Written here through ImageSharp's own default configuration,
           which still carries every decoder it ships. */
        var service = CreateService();
        var data = CreateImage(50, 50, TiffFormat.Instance);

        // Act
        var result = service.IsImage(data);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static void IsImage_WithinMaxPixels_ReturnsTrue()
    {
        // Arrange
        var service = CreateService();
        var data = CreateImage(50, 50);

        // Act
        var result = service.IsImage(data);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public static void Process_ExceedsMaxPixels_Throws()
    {
        // Arrange
        var service = CreateService(maxPixels: 100);
        var data = CreateImage(50, 50);

        // Act
        var act = () => service.Process(data, new ImageProcessingOptions { MaxWidth = 10 });

        // Assert
        act.Should().Throw<OdkServiceException>();
    }

    [Test]
    public static void Process_MaxWidthOnly_ReturnsResizedImage()
    {
        /* Arrange - the resize must reach the returned bytes. Requesting no format conversion is what
           previously left the resized image unencoded and returned the original data instead. */
        var service = CreateService();
        var data = CreateImage(200, 100);

        // Act
        var result = service.Process(data, new ImageProcessingOptions { MaxWidth = 50 });

        // Assert
        var size = service.Size(result);
        size.Width.Should().Be(50);
        size.Height.Should().Be(25);
    }

    [Test]
    public static void Process_MimeType_ReturnsRequestedFormat()
    {
        // Arrange
        var service = CreateService();
        var data = CreateImage(50, 50);

        // Act
        var result = service.Process(data, new ImageProcessingOptions { MimeType = "image/webp" });

        // Assert
        service.MimeType(result).Should().Be(WebpFormat.Instance.DefaultMimeType);
    }

    [Test]
    public static void Process_MimeTypeAndMaxWidth_ConvertsAndResizes()
    {
        // Arrange - the combination both uploads use.
        var service = CreateService();
        var data = CreateImage(400, 200);

        // Act
        var result = service.Process(data, new ImageProcessingOptions
        {
            MaxWidth = 100,
            MimeType = "image/webp"
        });

        // Assert
        service.MimeType(result).Should().Be(WebpFormat.Instance.DefaultMimeType);

        var size = service.Size(result);
        size.Width.Should().Be(100);
        size.Height.Should().Be(50);
    }

    [Test]
    public static void Process_NoOptions_ReturnsOriginalData()
    {
        // Arrange
        var service = CreateService();
        var data = CreateImage(50, 50);

        // Act
        var result = service.Process(data, new ImageProcessingOptions());

        // Assert
        result.Should().BeSameAs(data);
    }

    [Test]
    public static void Process_UnknownMimeType_KeepsSourceFormat()
    {
        // Arrange
        var service = CreateService();
        var data = CreateImage(200, 100);

        // Act
        var result = service.Process(data, new ImageProcessingOptions
        {
            MaxWidth = 50,
            MimeType = "image/tiff"
        });

        // Assert
        service.MimeType(result).Should().Be(PngFormat.Instance.DefaultMimeType);
    }

    [Test]
    public static void Reduce_WithinBounds_ReturnsUnchangedSize()
    {
        // Arrange
        var service = CreateService();
        var data = CreateImage(50, 40);

        // Act
        var result = service.Reduce(data, 100, 100);

        // Assert
        var size = service.Size(result);
        size.Width.Should().Be(50);
        size.Height.Should().Be(40);
    }

    [Test]
    public static void Size_NotAnImage_ReturnsZero()
    {
        // Arrange
        var service = CreateService();

        // Act
        var size = service.Size([1, 2, 3]);

        // Assert
        size.Width.Should().Be(0);
        size.Height.Should().Be(0);
    }

    [Test]
    public static void Size_ReturnsDimensions()
    {
        // Arrange
        var service = CreateService();
        var data = CreateImage(120, 80);

        // Act
        var size = service.Size(data);

        // Assert
        size.Width.Should().Be(120);
        size.Height.Should().Be(80);
    }

    private static byte[] CreateImage(int width, int height, IImageFormat? format = null)
    {
        using var image = new Image<Rgba32>(width, height);
        using var stream = new MemoryStream();
        image.Save(stream, format ?? PngFormat.Instance);
        return stream.ToArray();
    }

    private static ImageService CreateService(int maxPixels = MaxPixels)
        => new ImageService(new ImageServiceSettings { MaxPixels = maxPixels });
}
