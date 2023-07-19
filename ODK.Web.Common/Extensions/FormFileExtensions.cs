using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ODK.Web.Common.Extensions
{
    public static class FormFileExtensions
    {
        public static async Task<byte[]> ToByteArrayAsync(this IFormFile file)
        {
            if (file == null)
            {
                return null;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                return stream.ToArray();
            }
        }
    }
}
