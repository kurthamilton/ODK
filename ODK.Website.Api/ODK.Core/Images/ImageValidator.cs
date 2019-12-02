using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ODK.Core.Images
{
    public static class ImageValidator
    {
        public static IReadOnlyCollection<string> ValidMimeTypes { get; } = new[]
        {
            "image/jpg",
            "image/jpeg",
            "image/pjpeg",
            "image/gif",
            "image/x-png",
            "image/png"
        };

        public static bool IsValidData(byte[] imageData)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    Image.FromStream(ms);
                }
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        public static bool IsValidMimeType(string mimeType)
        {
            return ValidMimeTypes.Contains(mimeType.ToLowerInvariant());
        }        
    }
}
