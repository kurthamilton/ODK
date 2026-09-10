using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsHardCodedEmailsAdd : Migration
    {
        /* The seventeen templates for the emails MemberEmailService used to build in C#. The wording is
           carried over as it was, with the values it interpolated now reaching it as parameters, so a site
           admin can edit any of them and a group can override the ones marked IsGroupEmail.

           Three read differently on purpose, because a stored template cannot do what the code did:

           - Member left and membership removed: the branch on whether a reason was given is one line, and
             the parameter falls back to a dash.
           - Topics approved and rejected: the count no longer inflects the wording, so both read as plural.
           - A conversation reply's "Re: " arrives on {conversation.subject}, which is why the subject opens
             with it.

           One migration rather than one per group of emails: they are inserted together and there is no
           order in which applying some of them and not the rest leaves anything working. */
        private static readonly Email[] Emails =
        [
            new Email
            {
                BodyHtml =
                    """
                    <p>Your group <strong>{group.name}</strong> has been approved and you are ready to go!</p>
                    <p><a href="{group.url}">{group.url}</a></p>
                    """,
                // The site's word that the group passed review, so not the group's to reword.
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Admins,
                Subject = "{title} - Your group has been approved \U0001F680",
                Type = EmailType.GroupApproved
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>Your application to join {group.name} has been approved</p>
                    <p><a href="{group.url}">{group.url}</a></p>
                    """,
                IsGroupEmail = true,
                RecipientType = EmailRecipientType.Members,
                Subject = "{title} - You have been approved by {group.name}",
                Type = EmailType.MemberApproved
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>Hi {member.firstName}</p>
                    <p>Welcome to {title}!</p>
                    <p>Enjoy creating or joining your first group, and please do share.</p>
                    <p><a href="{account.urls.groups}">{account.urls.groups}</a></p>
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Members,
                Subject = "{title} - Welcome!",
                Type = EmailType.SiteWelcome
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>Your subscription has now expired</p>
                    <p><a href="{account.urls.siteSubscription}">{account.urls.siteSubscription}</a></p>
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Members,
                Subject = "{title} - Subscription Expired",
                Type = EmailType.SiteSubscriptionExpired
            },
            new Email
            {
                /* newGroup.name rather than group.name: this is sent as the site, and the core group.name
                   is what an email is addressed from. */
                BodyHtml =
                    """
                    <p>A group has just been created</p>
                    <p>Name: {newGroup.name}</p>
                    <p><a href="{siteadmin.urls.groups}">{siteadmin.urls.groups}</a></p>
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Admins,
                Subject = "{title} - New group",
                Type = EmailType.NewGroupAdmin
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>A spot has opened up for {event.name} on {event.date}.</p>
                    <p>Please update your RSVP if you no longer wish to attend.</p>
                    <p><a href="{event.url}">{event.url}</a></p>
                    """,
                IsGroupEmail = true,
                RecipientType = EmailRecipientType.Members,
                Subject = "{title} - You're in! A spot opened up for {event.name}",
                Type = EmailType.EventWaitlistPromotion
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>{member.name} has left {group.name}</p>
                    <p>They had been a member since {member.joined}</p>
                    <p>Reason: {member.leftReason}</p>
                    """,
                IsGroupEmail = true,
                RecipientType = EmailRecipientType.Admins,
                Subject = "{title} - {member.name} has left {group.name}",
                Type = EmailType.MemberLeftAdmin
            },
            new Email
            {
                /* message.reply carries the admin's own markup, so it is not wrapped in a paragraph;
                   message.text is what someone typed into the contact form, so it is. */
                BodyHtml =
                    """
                    {message.reply}
                    <hr/>
                    <p>Your original message:</p>
                    <p>{message.text}</p>
                    <p><a href="{group.url}">{group.url}</a></p>
                    """,
                IsGroupEmail = true,
                // Written for whoever contacted the group, who need not be a member of it.
                RecipientType = EmailRecipientType.Members,
                Subject = "Re: your message to {title}",
                Type = EmailType.ContactRequestReply
            },
            new Email
            {
                BodyHtml =
                    """
                    {message.reply}
                    <hr/>
                    <p>Your original message:</p>
                    <p>{message.text}</p>
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Members,
                Subject = "Re: your message to {title}",
                Type = EmailType.SiteContactRequestReply
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>The following topics require approval</p>
                    {topics}
                    <p><a href="{siteadmin.urls.topics}">{siteadmin.urls.topics}</a></p>
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Admins,
                Subject = "{title} - New topics",
                Type = EmailType.NewTopicAdmin
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>The following topics have been approved</p>
                    {topics}
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Members,
                Subject = "{title} - Topics approved",
                Type = EmailType.TopicsApproved
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>The following topics have been rejected</p>
                    {topics}
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Members,
                Subject = "{title} - Topics rejected",
                Type = EmailType.TopicsRejected
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>You have a new reply on your conversation:</p>
                    <p>{conversation.message}</p>
                    <p><a href="{conversation.url}">{conversation.url}</a></p>
                    """,
                IsGroupEmail = true,
                RecipientType = EmailRecipientType.Members,
                Subject = "{conversation.subject} - {title}",
                Type = EmailType.ConversationMessage
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>A new reply has been added to a conversation:</p>
                    <p>{conversation.message}</p>
                    <p><a href="{conversation.url}">{conversation.url}</a></p>
                    """,
                IsGroupEmail = true,
                RecipientType = EmailRecipientType.Admins,
                Subject = "{conversation.subject} - {title}",
                Type = EmailType.ConversationMessageAdmin
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>You have a new reply on your conversation:</p>
                    <p>{conversation.message}</p>
                    <p><a href="{conversation.url}">{conversation.url}</a></p>
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Members,
                Subject = "{conversation.subject} - {title}",
                Type = EmailType.SiteConversationMessage
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>A new reply has been added to a conversation:</p>
                    <p>{conversation.message}</p>
                    <p><a href="{conversation.url}">{conversation.url}</a></p>
                    """,
                IsGroupEmail = false,
                RecipientType = EmailRecipientType.Admins,
                Subject = "{conversation.subject} - {title}",
                Type = EmailType.SiteConversationMessageAdmin
            },
            new Email
            {
                BodyHtml =
                    """
                    <p>You have been removed from the {group.name} group</p>
                    <p>Reason: {member.removedReason}</p>
                    """,
                IsGroupEmail = true,
                RecipientType = EmailRecipientType.Members,
                Subject = "{title} - you have been removed from a group",
                Type = EmailType.MemberRemoved
            }
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertEmails(EmailSchemaEra.IdKeyBodyHtml, Emails);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var email in Emails)
            {
                migrationBuilder.DeleteEmail(EmailSchemaEra.IdKeyBodyHtml, email.Type);
            }
        }
    }
}
