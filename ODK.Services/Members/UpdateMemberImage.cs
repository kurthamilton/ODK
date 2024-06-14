namespace ODK.Services.Members;

public class UpdateMemberImage
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();

    public string MimeType { get; set; } = "";
}
