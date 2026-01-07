using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EmailProviderRepository : CachingReadWriteRepositoryBase<EmailProvider>, IEmailProviderRepository
{
    private static readonly EntityCache<Guid, EmailProvider> _cache = new DatabaseEntityCache<EmailProvider>();

    public EmailProviderRepository(OdkContext context)
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<EmailProvider> GetAll() => Set()
        .DeferredMultiple(
            _cache.GetAll,
            _cache.SetAll);

    public IDeferredQueryMultiple<EmailProviderSummaryDto> GetEmailsSentToday()
    {
        var query =
            from provider in Set()
            from sentEmail in Set<SentEmail>()
            where sentEmail.EmailProviderId == provider.Id &&
                sentEmail.SentUtc >= DateTime.UtcNow.Date &&
                sentEmail.SentUtc < DateTime.UtcNow.Date.AddDays(1)
            group provider by provider.Id into g
            select new EmailProviderSummaryDto
            {
                Sent = g.Count(),
                EmailProviderId = g.Key
            };

        return query.DeferredMultiple();
    }
}
