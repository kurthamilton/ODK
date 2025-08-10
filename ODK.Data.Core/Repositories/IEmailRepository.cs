using ODK.Core.Emails;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEmailRepository : IWriteRepository<Email>
{
    IDeferredQueryMultiple<Email> GetAll();

    IDeferredQuerySingle<Email> GetByType(EmailType type);    
}
