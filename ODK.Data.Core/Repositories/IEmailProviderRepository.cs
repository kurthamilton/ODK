using ODK.Core.Emails;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEmailProviderRepository : IReadWriteRepository<EmailProvider>
{
    IDeferredQueryMultiple<EmailProvider> GetAll();

    IDeferredQueryMultiple<EmailProviderSummaryDto> GetEmailsSentToday();
}
