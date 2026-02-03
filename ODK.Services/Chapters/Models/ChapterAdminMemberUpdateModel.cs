using ODK.Core.Chapters;

namespace ODK.Services.Chapters.Models;

public class ChapterAdminMemberUpdateModel
{
    public required string? AdminEmailAddress { get; init; }

    public required bool ReceiveContactEmails { get; init; }

    public required bool ReceiveEventCommentEmails { get; init; }

    public required bool ReceiveNewMemberEmails { get; init; }

    public required ChapterAdminRole Role { get; init; }
}