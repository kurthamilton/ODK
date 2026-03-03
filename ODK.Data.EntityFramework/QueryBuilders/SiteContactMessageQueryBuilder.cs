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

    public ISiteContactMessageQueryBuilder ForSpamScore(bool isSpam, double threshold)
    {
        Query = Query
            .Where(x => isSpam
                ? x.RecaptchaScore < threshold
                : x.RecaptchaScore >= threshold);
        return this;
    }
}