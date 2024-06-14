using ODK.Core.Chapters;

namespace ODK.Services.Members;

public class MemberCsvFileHeaders
{
    public const string Email = "Email";
    public const string FirstName = "FirstName";
    public const string LastName = "LastName";
    public const string SubscriptionExpiry = "Subscription Expires (yyyy-mm-dd)";

    public MemberCsvFileHeaders(IEnumerable<ChapterProperty> properties)
    {
        Properties = properties.ToArray();
    }

    public IEnumerable<string> ToFields()
    {
        yield return Email;
        yield return FirstName;
        yield return LastName;
        yield return SubscriptionExpiry;

        foreach (ChapterProperty property in Properties.OrderBy(x => x.DisplayOrder))
        {
            yield return property.Label;
        }
    }

    private IReadOnlyCollection<ChapterProperty> Properties { get; }
}
