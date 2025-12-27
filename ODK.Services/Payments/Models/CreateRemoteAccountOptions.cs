using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;

namespace ODK.Services.Payments.Models;

public class CreateRemoteAccountOptions
{
    public required Chapter Chapter { get; init; }

    public required Currency ChapterCurrency { get; init; }

    public required string ChapterUrl { get; init; }

    public required Country Country { get; init; }

    public required Member Owner { get; init; }
}
