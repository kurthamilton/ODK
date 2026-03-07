using Microsoft.EntityFrameworkCore;
using ODK.Core.Messages;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class SiteContactMessageQueryBuilder
    : DatabaseEntityQueryBuilder<SiteContactMessage, ISiteContactMessageQueryBuilder>, ISiteContactMessageQueryBuilder
{
    public SiteContactMessageQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override ISiteContactMessageQueryBuilder Builder => this;

    public ISiteContactMessageQueryBuilder ForStatus(MessageStatus status, double spamThreshold)
    {
        if (status == MessageStatus.Unreplied)
        {
            Query = Query.Where(x => x.RecaptchaScore >= spamThreshold && x.RepliedUtc == null);
        }
        else if (status == MessageStatus.Replied)
        {
            Query = Query.Where(x => x.RecaptchaScore >= spamThreshold && x.RepliedUtc != null);
        }
        else if (status == MessageStatus.Spam)
        {
            Query = Query.Where(x => x.RecaptchaScore < spamThreshold);
        }

        return this;
    }
}