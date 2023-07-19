using System;

namespace ODK.Core.SocialMedia
{
    public class InstagramImage
    {
        public InstagramImage(Guid instagramPostId, byte[] imageData, string mimeType)
        {
            ImageData = imageData;
            InstagramPostId = instagramPostId;
            MimeType = mimeType;
        }

        public byte[] ImageData { get; }

        public Guid InstagramPostId { get; }

        public string MimeType { get; }
    }
}
