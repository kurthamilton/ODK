using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class SitePaymentSettingsQueryBuilder
    : DatabaseEntityQueryBuilder<SitePaymentSettings, ISitePaymentSettingsQueryBuilder>, ISitePaymentSettingsQueryBuilder
{
    public SitePaymentSettingsQueryBuilder(DbContext context)
        : base(context)
    {
    }

    public SitePaymentSettingsQueryBuilder(DbContext context, IQueryable<SitePaymentSettings> query)
        : base(context, query)
    {
    }

    protected override ISitePaymentSettingsQueryBuilder Builder => this;
}
