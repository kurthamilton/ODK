using ODK.Core.Countries;
using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberLocationViewModel
{
    public required IReadOnlyCollection<DistanceUnit> DistanceUnits { get; init; }

    public required MemberLocation? MemberLocation { get; init; }

    public required MemberPreferences? MemberPreferences { get; init; }
}