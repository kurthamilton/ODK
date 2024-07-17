using ODK.Core.Emails;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEmailRepository : IWriteRepository<Email>
{
    void AddSentEmail(SentEmail sentEmail);

    IDeferredQueryMultiple<Email> GetAll();

    IDeferredQuerySingle<Email> GetByType(EmailType type);    
}
