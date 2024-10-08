﻿using MimeKit;
using ODK.Core.Emails;

namespace ODK.Services.Integrations.Emails.Extensions;

internal static class MailAddressExtensions
{
    public static MailboxAddress ToMailboxAddress(this EmailAddressee address)
    {
        return new MailboxAddress(address.Name, address.Address);
    }
}
