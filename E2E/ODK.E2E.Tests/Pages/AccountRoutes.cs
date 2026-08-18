namespace ODK.E2E.Tests.Pages;

/// <summary>
/// Relative URLs for a member's own account pages. On Default these are global (<c>/account/...</c>); on
/// DrunkenKnitwits they're chapter-scoped (<c>/{chapterName}/account/...</c>), so a DrunkenKnitwits member
/// must belong to a chapter. The forms themselves are shared across platforms; only the page URLs differ.
/// </summary>
public abstract class AccountRoutes
{
    /// <summary>The change-email request form.</summary>
    public abstract string ChangeEmail { get; }

    /// <summary>The change-password form.</summary>
    public abstract string ChangePassword { get; }

    /// <summary>The forgotten-password request form, which emails a reset link.</summary>
    public abstract string ForgottenPassword { get; }

    /// <summary>The personal-details (name) edit page.</summary>
    public abstract string PersonalDetails { get; }

    public static AccountRoutes Default() => new DefaultAccountRoutes();

    public static AccountRoutes DrunkenKnitwits(string chapterShortName) =>
        new DrunkenKnitwitsAccountRoutes(chapterShortName);

    /// <summary>The link that confirms a pending email change (as the logged-in member).</summary>
    public abstract string EmailChangeConfirm(string token);

    /// <summary>The reset form the emailed link lands on, carrying the token.</summary>
    public abstract string PasswordReset(string token);

    private sealed class DefaultAccountRoutes : AccountRoutes
    {
        public override string ChangeEmail => "/account/emails";

        public override string ChangePassword => "/account/password/change";

        public override string ForgottenPassword => "/account/password/forgotten";

        public override string PersonalDetails => "/account";

        public override string EmailChangeConfirm(string token) =>
            $"/account/email/change/confirm?token={Uri.EscapeDataString(token)}";

        public override string PasswordReset(string token) =>
            $"/account/password/reset?token={Uri.EscapeDataString(token)}";
    }

    private sealed class DrunkenKnitwitsAccountRoutes : AccountRoutes
    {
        private readonly string _shortName;

        public DrunkenKnitwitsAccountRoutes(string chapterShortName)
        {
            _shortName = chapterShortName;
        }

        public override string ChangeEmail => $"/{_shortName}/account/email/change";

        public override string ChangePassword => $"/{_shortName}/account/password/change";

        public override string ForgottenPassword => $"/{_shortName}/account/password/forgotten";

        public override string PersonalDetails => $"/{_shortName}/account";

        public override string EmailChangeConfirm(string token) =>
            $"/{_shortName}/account/email/change/confirm?token={Uri.EscapeDataString(token)}";

        public override string PasswordReset(string token) =>
            $"/{_shortName}/account/password/reset?token={Uri.EscapeDataString(token)}";
    }
}