using ODK.Core.Countries;

namespace ODK.Services.Members;

public class CreateAccountModel
{
    public required string EmailAddress { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required LatLong? Location { get; set; }

    public required string LocationName { get; set; }
}
