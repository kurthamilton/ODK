namespace ODK.Services;
public class AdminServiceRequest
{
    public AdminServiceRequest(Guid chapterId, Guid currentMemberId)
    {
        ChapterId = chapterId;
        CurrentMemberId = currentMemberId;
    }

    public Guid ChapterId { get; }

    public Guid CurrentMemberId { get; }
}
