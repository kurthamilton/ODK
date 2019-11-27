using System;
using System.Collections.Generic;
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
            return await Context
                .Insert(email)
                .GetIdentityAsync();
        }

        public async Task AddMemberEventEmail(MemberEventEmail email)
        {
            await Context.Insert(email)
                .ExecuteAsync();
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

        public async Task<IReadOnlyCollection<MemberEventEmail>> GetMemberEventEmails(Guid eventId)
        {
            return await Context
                .Select<MemberEventEmail>()
                .Where(x => x.EventId).EqualTo(eventId)
                .ToArrayAsync();
        }
    }
}
