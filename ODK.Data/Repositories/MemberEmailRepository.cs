using System;
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

        public async Task<Guid> AddMemberEmail(MemberEmail email)
        {
            return await Context.InsertAsync(email);
        }

        public async Task ConfirmMemberEmailSent(Guid id)
        {
            await Context
                .Update<MemberEmail>()
                .Set(x => x.Sent, true)
                .Where(x => x.Id).EqualTo(id)
                .ExecuteAsync();
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
