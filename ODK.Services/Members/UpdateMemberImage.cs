namespace ODK.Services.Members;

public class UpdateMemberImage
{    
    public required byte[] ImageData { get; set; }

    public required string MimeType { get; set; }
}
