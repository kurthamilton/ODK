namespace ODK.E2E.Data;

/// <summary>
/// Direct-to-DB member changes that have no self-service UI. Promoting a member to site admin
/// (<c>Members.SuperAdmin = 1</c>, the <see cref="ODK.Core.Members.Member.SiteAdmin"/> flag) is done
/// here after the account is created and activated through the UI; the flag is read into the login
/// claims, so it must be set before the site admin logs in.
/// </summary>
public static class MemberAdminDataHelper
{
    public static async Task SetSiteAdmin(string emailAddress)
    {
        const string sql = "UPDATE Members SET SuperAdmin = 1 WHERE EmailAddress = @email";

        await using var builder = E2EQueryBuilder
            .Create(sql)
            .AddParameter("@email", emailAddress);

        var affected = await builder.ExecuteNonQuery();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Could not promote '{emailAddress}' to site admin - no matching member. Was the account created and activated first?");
        }
    }
}