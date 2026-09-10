using System.Globalization;
using ODK.Core.Members;
using ODK.Core.Utils;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// Tells group admins that a member has left.
/// </summary>
public sealed class MemberLeftParameters : EmailTypeParameters
{
    private const string JoinedName = "member.joined";

    private const string NameName = "member.name";

    private const string ReasonName = "member.leftReason";

    private readonly CultureInfo _culture;
    private readonly Member _member;
    private readonly TimeZoneInfo _timeZone;

    public MemberLeftParameters(Member member, CultureInfo culture, TimeZoneInfo timeZone)
    {
        _culture = culture;
        _member = member;
        _timeZone = timeZone;
    }

    public static IReadOnlyCollection<string> Names { get; } = [JoinedName, NameName, ReasonName];

    public required DateTime? JoinedUtc { get; init; }

    public required string? Reason { get; init; }

    protected override void AddParameters(IDictionary<string, string> values)
    {
        var joined = JoinedUtc?.ToFriendlyDateString(new FriendlyDateStringOptions
        {
            IncludeDayOfWeek = true,
            TimeZone = _timeZone,
            Culture = _culture
        });

        Add(values, JoinedName, joined ?? NoValue);
        Add(values, NameName, _member.FullName);
        Add(values, ReasonName, !string.IsNullOrEmpty(Reason) ? Reason : NoValue);
    }
}
