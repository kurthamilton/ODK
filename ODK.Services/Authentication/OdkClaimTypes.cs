namespace ODK.Services.Authentication;

public static class OdkClaimTypes
{
    /// <summary>
    /// The members signed in on the same auth cookie, in the order they signed in, so a site admin can
    /// switch between them without re-authenticating. Written only when more than one is signed in;
    /// the member a request acts as is always the one in <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>.
    /// </summary>
    public const string SignedInMemberIds = "odk:signed-in-member-ids";
}
