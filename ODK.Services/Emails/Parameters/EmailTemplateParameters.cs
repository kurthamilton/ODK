using ODK.Core.Emails;

namespace ODK.Services.Emails.Parameters;

/// <summary>
/// What an admin editing a template is offered: the parameters the email type adds, then the core ones
/// every email gets.
/// </summary>
/// <remarks>
/// Registration is explicit, and an unregistered type throws rather than quietly offering the core list
/// alone - a type whose parameters were never declared would otherwise look like a type that has none.
/// EmailTemplateParametersTests holds it to covering every type.
/// </remarks>
public static class EmailTemplateParameters
{
    private static readonly IReadOnlyDictionary<EmailType, IReadOnlyCollection<string>> TypeNames =
        new Dictionary<EmailType, IReadOnlyCollection<string>>
        {
            [EmailType.ActivateAccount] = ActivateAccountParameters.Names,
            [EmailType.ContactRequest] = ContactRequestParameters.Names,
            [EmailType.DuplicateEmail] = DuplicateEmailParameters.Names,
            [EmailType.EmailAddressUpdate] = EmailAddressUpdateParameters.Names,
            [EmailType.EventComment] = EventCommentParameters.Names,
            // The same values as the admin copy - only the audience and the wording differ.
            [EmailType.EventCommentReply] = EventCommentParameters.Names,
            [EmailType.EventInvite] = EventInviteParameters.Names,
            [EmailType.Layout] = LayoutParameters.Names,
            [EmailType.MemberImportActivation] = MemberImportActivationParameters.Names,
            [EmailType.MemberImportInvite] = MemberImportInviteParameters.Names,
            [EmailType.NewMember] = NewMemberParameters.Names,
            [EmailType.NewMemberAdmin] = NewMemberAdminParameters.Names,
            [EmailType.PasswordReset] = PasswordResetParameters.Names,
            [EmailType.PaymentNotification] = PaymentNotificationParameters.Names,
            [EmailType.SubscriptionConfirmation] = SubscriptionConfirmationParameters.Names,
            [EmailType.SubscriptionExpired] = SubscriptionExpiryParameters.Names,
            [EmailType.SubscriptionExpiring] = SubscriptionExpiryParameters.Names,
            [EmailType.TrialExpired] = SubscriptionExpiryParameters.Names,
            [EmailType.TrialExpiring] = SubscriptionExpiryParameters.Names
        };

    public static IReadOnlyCollection<string> ForGroup(EmailType type)
        => Combine(EmailParameters.GroupNames, type);

    /* The theme colours only appear on the layout. They style the layout's own markup, so on any other
       template they are four buttons that do nothing useful - and a group never sees them at all, even
       on the layout, because the colours belong to the site rather than to one group. They keep
       resolving wherever they are already used; this is only about what is offered. */
    public static IReadOnlyCollection<string> ForSite(EmailType type)
        => Combine(
            type == EmailType.Layout
                ? EmailParameters.Names
                : EmailParameters.Names.Except(EmailParameters.ThemeNames).ToArray(),
            type);

    public static IReadOnlyCollection<string> ForType(EmailType type)
    {
        if (!TypeNames.TryGetValue(type, out var names))
        {
            throw new ArgumentException(
                $"No parameters are registered for {type}. Add them to {nameof(EmailTemplateParameters)}.",
                nameof(type));
        }

        return names;
    }

    private static IReadOnlyCollection<string> Combine(IEnumerable<string> core, EmailType type)
        => [.. ForType(type).Concat(core).Order(StringComparer.Ordinal)];
}
