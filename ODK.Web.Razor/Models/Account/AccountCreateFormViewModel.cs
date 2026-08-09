using ODK.Core.Countries;
using ODK.Core.Topics;
using ODK.Services.Users.ViewModels;
using ODK.Web.Razor.Models.Topics;

namespace ODK.Web.Razor.Models.Account;

public class AccountCreateFormViewModel
{
    public required string GoogleClientId { get; init; }

    public required Location? Location { get; init; }

    /// <summary>
    /// The values a rejected submit posted. When set, each wizard page renders these instead of its
    /// defaults, so a member correcting one field doesn't lose the answers on the other three pages.
    /// Null on a first render.
    /// </summary>
    public LocationFormViewModel? PostedLocation { get; init; }

    public OAuthDetailsFormViewModel? PostedOAuth { get; init; }

    public PersonalDetailsFormViewModel? PostedPersonalDetails { get; init; }

    public TopicPickerFormSubmitViewModel? PostedTopics { get; init; }

    public required IReadOnlyCollection<TopicGroup> TopicGroups { get; init; }

    public required IReadOnlyCollection<Topic> Topics { get; init; }
}