using ODK.Core.Chapters;
using ODK.Core.Emails;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailContentViewModel
{
    public ChapterEmailContentViewModel(
        Chapter chapter,
        ChapterEmail email,
        bool canEdit,
        EmailRecipientType recipientType,
        string title)
    {
        CanEdit = canEdit;
        Chapter = chapter;
        Email = email;
        RecipientType = recipientType;
        Title = title;
    }

    public bool CanEdit { get; }

    public Chapter Chapter { get; }

    public ChapterEmail Email { get; }

    public EmailRecipientType RecipientType { get; }

    /// <summary>
    /// What this email resolves <c>{title}</c> to, itself a template.
    /// </summary>
    public string Title { get; }
}
