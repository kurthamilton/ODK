using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace ODK.Services.Imaging
{
    public class ImageService : IImageService
    {
        public byte[] Crop(byte[] data, int width, int height)
        {
            return ProcessImage(data, image =>
            {
                Size size = GetRescaledSize(image.Size(), new Size(width, height), Math.Max);
                image.Mutate(x =>
                {
                    try
                    {
                        x.Resize(size);
                    }
                    catch
                    {
                        // let the resize fail if the target size is smaller than the original
                    }

                    Rectangle crop = new Rectangle(
                        Math.Max(size.Width - width, 0) / 2,
                        Math.Max(size.Height - height, 0) / 2,
                        Math.Min(width, size.Width),
                        Math.Min(height, size.Width));

                    try
                    {
                        x.Crop(crop);
                    }
                    catch
                    {
                        // let the crop fail if the target size is smaller than the original
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
                image.Mutate(x => x.Rotate(degrees));
            });
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
            using Image image = Image.Load(data, out IImageFormat format);
            action(image);
            return ImageToBytes(image, format);
        }

        private static void RescaleImage(Image image, int maxWidth, int maxHeight)
        {
            Size rescaled = GetRescaledSize(image.Size(), new Size(maxWidth, maxHeight), Math.Min);
            image.Mutate(x => x.Resize(rescaled));
        }
    }
}
