namespace ODK.Services.Members.Workflows;

/// <remarks>Numbered for the reason given on <see cref="AccountState"/>.</remarks>
public enum AccountTrigger
{
    None = 0,

    /// <summary>An admin imports the address from a file.</summary>
    Import = 1,

    /// <summary>The sign-up form is submitted.</summary>
    SignUp = 2,

    /// <summary>An activation link is followed and a password set.</summary>
    Activate = 3,

    /// <summary>A member who can already sign in asks to join a group.</summary>
    Join = 4,

    /// <summary>An admin approves an application.</summary>
    Approve = 5
}
