using ODK.Core.Emails;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EmailRepository : WriteRepositoryBase<Email>, IEmailRepository
{
    public EmailRepository(OdkContext context)
        : base(context)
    {
    }

    public void AddSentEmail(SentEmail sentEmail)
    {
        if (sentEmail.Id == default)
        {
            sentEmail.Id = Guid.NewGuid();
        }

        AddSingle(sentEmail);
    }

    public IDeferredQueryMultiple<Email> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQuerySingle<Email> GetByType(EmailType type) => Set()
        .Where(x => x.Type == type)
        .DeferredSingle();    
}
