namespace ODK.Core.Members;

/// <summary>
/// What a sign-up was for, where it was started by somebody trying to do something in particular rather
/// than to join the site. Carried on the activation token so it survives the sign-up leaving the browser
/// for an inbox, and spent with it.
/// </summary>
/// <remarks>
/// Numbered explicitly: the value is persisted and read back after an arbitrary delay, so the number is
/// the contract. <see cref="None"/> is never stored - a sign-up that states nothing leaves the column
/// null, the way a token not scoped to a group leaves its chapter null.
/// </remarks>
public enum SignUpIntentType
{
    None = 0,

    /// <summary>
    /// The member signed up in order to create a group. They are sent to the create-group form once they
    /// have activated and signed in, rather than to the site they have not asked to look around.
    /// </summary>
    CreateGroup = 1
}
