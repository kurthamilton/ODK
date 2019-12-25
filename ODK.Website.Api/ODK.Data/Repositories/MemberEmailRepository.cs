using System.Threading.Tasks;
using ODK.Core.Mail;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class MemberEmailRepository : RepositoryBase, IMemberEmailRepository
    {
        public MemberEmailRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task<Email> GetEmail(EmailType type)
        {
            return await Context
                .Select<Email>()
                .Where(x => x.Type).EqualTo(type)
                .FirstOrDefaultAsync();
        }
    }
}
