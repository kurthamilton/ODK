namespace ODK.Services.Members.Workflows.Account;

/// <remarks>Numbered for the reason given on <see cref="AccountState"/>.</remarks>
public enum AccountTrigger
{
    None = 0,

    /// <summary>An admin imports the address from a file, which raises an account nobody has signed up for.</summary>
    Import = 1,

    /// <summary>The sign-up form is submitted.</summary>
    SignUp = 2,

    /// <summary>An activation link is followed and a password set.</summary>
    Activate = 3
}
