namespace ODK.Services.Emails.Parameters;

/// <summary>
/// What each parameter means, for the table an admin editing a template reads. Keyed by name because each
/// parameter class keeps its own names private and publishes only the list.
/// </summary>
/// <remarks>
/// Written for someone deciding whether to put a parameter in their wording, so each says what will appear
/// rather than where the value comes from. EmailParameterDescriptionTests holds this to covering every
/// parameter any email offers, so adding one to a parameters class fails until it is described here.
/// </remarks>
public static class EmailParameterDescriptions
{
    private static readonly IReadOnlyDictionary<string, string> Descriptions =
        new Dictionary<string, string>(EmailParameterComparer.Default)
        {
            ["account.urls.activate"] = "Link the recipient follows to activate their account",
            ["account.urls.confirmEmailAddressUpdate"] =
                "Link the recipient follows to confirm their new email address",
            ["account.urls.groups"] = "Link to the recipient's own groups",
            ["account.urls.login"] = "Link to the login page",
            ["account.urls.passwordReset"] = "Link the recipient follows to choose a new password",
            ["account.urls.siteSubscription"] = "Link to the recipient's subscription page",
            ["account.urls.unsubscribe"] = "Link the recipient follows to stop receiving these emails",
            ["admin.urls.member"] = "Link to the member's profile in the group's admin area",
            ["body"] = "The email being sent, rendered into this layout",
            ["comment.text"] = "The comment that was posted",
            ["conversation.message"] = "The message that was sent",
            ["conversation.subject"] = "The subject the conversation was started under",
            ["conversation.url"] = "Link to the conversation",
            ["event.date"] = "The event's date, written out in full",
            ["event.id"] = "The event's identifier, for building your own links",
            ["event.location"] = "The name of the event's venue",
            ["event.name"] = "The event's name",
            ["event.rsvpUrl"] = "Link the recipient follows to reply yes to the event",
            ["event.time"] = "The event's start time",
            ["event.url"] = "Link to the event's page",
            ["group.name"] = "Your group's name",
            ["group.url"] = "Link to your group's home page",
            ["group.urls.events"] = "Link to your group's events page",
            ["group.urls.join"] = "Link the recipient follows to join your group",
            ["group.urls.refuseInvite"] =
                "Link the recipient follows to decline their invite and have the details you imported "
                + "about them deleted",
            ["invites.count"] = "How many invites are waiting to be sent",
            ["invites.url"] = "Link to the invited members page, where the invites are sent from",
            ["member.firstName"] = "The recipient's first name",
            ["member.joined"] = "The date the member joined your group",
            ["member.leftReason"] =
                "The reason the member gave for leaving, or a dash where they gave none",
            ["member.name"] = "The member's full name",
            ["member.properties"] = "The answers the member gave when joining, as a list",
            ["member.removedReason"] =
                "The reason given for removing the member, or a dash where none was given",
            ["message.from"] = "The name and email address the message came from",
            ["message.reply"] = "The reply being sent",
            ["message.text"] = "The message that was sent",
            ["message.url"] = "Link to the message in the group's admin area",
            ["newGroup.name"] = "The name of the group that was created",
            ["payment.amount"] = "The amount paid, with its currency symbol",
            ["payment.reference"] = "The payment's reference, for matching it against your records",
            ["platform.url"] = "Link to the platform's home page",
            ["siteadmin.urls.groups"] = "Link to the groups list in the site admin area",
            ["siteadmin.urls.topics"] = "Link to the topic approval page in the site admin area",
            ["subscription.amount"] = "The amount paid, with its currency symbol",
            ["subscription.disabledDate"] = "The date the membership stops working",
            ["subscription.end"] = "The date the subscription being paid for runs to",
            ["subscription.expiryDate"] = "The date the membership expires",
            ["theme.body.background"] = "The theme's body background colour",
            ["theme.body.color"] = "The theme's body text colour",
            ["theme.header.background"] = "The theme's header background colour",
            ["theme.header.color"] = "The theme's header text colour",
            ["title"] = "The wording emails use to refer to your group",
            ["topics"] = "The topics this email is about, as a table"
        };

    /// <summary>
    /// What <paramref name="name"/> puts in an email, or null where nothing describes it - which the
    /// coverage test exists to prevent, so a caller can render the row without one rather than failing.
    /// </summary>
    public static string? For(string name) => Descriptions.TryGetValue(name, out var description)
        ? description
        : null;
}
