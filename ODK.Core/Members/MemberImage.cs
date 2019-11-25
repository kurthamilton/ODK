using System;

namespace ODK.Core.Members
{
    public class MemberImage
    {
        public MemberImage(Guid memberId, byte[] imageData, string mimeType)
        {
            ImageData = imageData;
            MemberId = memberId;
            MimeType = mimeType;
        }

        public byte[] ImageData { get; }

        public Guid MemberId { get; }

        public string MimeType { get; set; }
    }
}
