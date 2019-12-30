using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ODK.Utils
{
    public static class ImageDownloader
    {
        public static async Task DownloadImages(string text, string savePath)
        {
            string[] lines = text.Split(Environment.NewLine);
            foreach (string line in lines)
            {
                string[] parts = line.Split("|");
                string name = parts[0];
                string url = parts[1];
                string extension = url.Substring(url.LastIndexOf("."), url.Length - url.LastIndexOf("."));
                string filePath = Path.Combine(savePath, name + extension);

                await Download(url, filePath);
            }
        }

        public static async Task Download(string url, string filePath)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                byte[] data = await response.Content.ReadAsByteArrayAsync();
                using (FileStream stream = File.Create(filePath))
                {
                    stream.Write(data, 0, data.Length);
                }
            }
        }
    }
}
