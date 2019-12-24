using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Data.Sql;
using ODK.Data.Sql.Queries;

namespace ODK.Data.Repositories
{
    public class MemberRepository : RepositoryBase, IMemberRepository
    {
        public MemberRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task ActivateMember(Guid memberId)
        {
            await Context
                .Update<Member>()
                .Set(x => x.Activated, true)
                .ExecuteAsync();
        }

        public async Task AddActivationToken(MemberActivationToken token)
        {
            await Context
                .Insert(token)
                .ExecuteAsync();
        }

        public async Task AddMemberImage(MemberImage image)
        {
            await Context
                .Insert(image)
                .ExecuteAsync();
        }

        public async Task AddMemberSubscriptionRecord(MemberSubscriptionRecord record)
        {
            await Context
                .Insert(record)
                .ExecuteAsync();
        }

        public async Task AddPasswordResetRequest(Guid memberId, DateTime created, DateTime expires, string token)
        {
            MemberPasswordResetRequest request = new MemberPasswordResetRequest(Guid.Empty, memberId, created, expires, token);
            await Context
                .Insert(request)
                .ExecuteAsync();
        }

        public async Task AddRefreshToken(MemberRefreshToken token)
        {
            await Context
                .Insert(token)
                .ExecuteAsync();
        }

        public async Task<Guid> CreateMember(Member member)
        {
            return await Context
                .Insert(member)
                .GetIdentityAsync();
        }

        public async Task DeleteActivationToken(Guid memberId)
        {
            await Context
                .Delete<MemberActivationToken>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .ExecuteAsync();
        }

        public async Task DeletePasswordResetRequest(Guid passwordResetRequestId)
        {
            await Context
                .Delete<MemberPasswordResetRequest>()
                .Where(x => x.Id).EqualTo(passwordResetRequestId)
                .ExecuteAsync();
        }

        public async Task DeleteRefreshToken(MemberRefreshToken refreshToken)
        {
            await Context
                .Delete<MemberRefreshToken>()
                .Where(x => x.Id).EqualTo(refreshToken.Id)
                .ExecuteAsync();
        }

        public async Task DisableMember(Guid id)
        {
            await Context
                .Update<Member>()
                .Set(x => x.Disabled, true)
                .Where(x => x.Id).EqualTo(id)
                .ExecuteAsync();
        }

        public async Task EnableMember(Guid id)
        {
            await Context
                .Update<Member>()
                .Set(x => x.Disabled, false)
                .Where(x => x.Id).EqualTo(id)
                .ExecuteAsync();
        }

        public async Task<Member> FindMemberByEmailAddress(string emailAddress)
        {
            return await Context
                .Select<Member>()
                .Where(x => x.EmailAddress).EqualTo(emailAddress)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<Member>> GetLatestMembers(Guid chapterId, int maxSize)
        {
            return await Context
                .Select<Member>()
                .Top(maxSize)
                .OrderBy(x => x.CreatedDate, SqlSortDirection.Descending)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<Member> GetMember(Guid memberId, bool searchAll = false)
        {
            return await MembersQuery(searchAll)
                .Where(x => x.Id).EqualTo(memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<MemberActivationToken> GetMemberActivationToken(Guid memberId)
        {
            return await Context
                .Select<MemberActivationToken>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<MemberActivationToken> GetMemberActivationToken(string activationToken)
        {
            return await Context
                .Select<MemberActivationToken>()
                .Where(x => x.ActivationToken).EqualTo(activationToken)
                .FirstOrDefaultAsync();
        }

        public async Task<MemberImage> GetMemberImage(Guid memberId, long? versionAfter)
        {
            return await Context
                .Select<MemberImage>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .ConditionalWhere(x => x.Version, versionAfter.HasValue).GreaterThan(versionAfter ?? 0)
                .FirstOrDefaultAsync();
        }

        public async Task<MemberPassword> GetMemberPassword(Guid memberId)
        {
            return await Context
                .Select<MemberPassword>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId)
        {
            return await Context
                .Select<MemberProperty>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Member>> GetMembers(Guid chapterId, bool searchAll = false)
        {
            return await MembersQuery(searchAll)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<MemberSubscription> GetMemberSubscription(Guid memberId)
        {
            return await Context
                .Select<MemberSubscription>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid chapterId)
        {
            return await Context
                .Select<MemberSubscription>()
                .Where<Member, Guid>(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<long> GetMembersVersion(Guid chapterId, bool searchAll = false)
        {
            return await MembersQuery(searchAll)
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .VersionAsync();
        }

        public async Task<MemberPasswordResetRequest> GetPasswordResetRequest(string token)
        {
            return await Context
                .Select<MemberPasswordResetRequest>()
                .Where(x => x.Token).EqualTo(token)
                .FirstOrDefaultAsync();
        }

        public async Task<MemberRefreshToken> GetRefreshToken(string refreshToken)
        {
            return await Context
                .Select<MemberRefreshToken>()
                .Where(x => x.RefreshToken).EqualTo(refreshToken)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateMember(Guid memberId, bool emailOptIn, string firstName, string lastName)
        {
            await Context
                .Update<Member>()
                .Set(x => x.EmailOptIn, emailOptIn)
                .Set(x => x.FirstName, firstName)
                .Set(x => x.LastName, lastName)
                .Where(x => x.Id).EqualTo(memberId)
                .ExecuteAsync();
        }

        public async Task UpdateMemberImage(MemberImage image)
        {
            bool memberHasImage = await MemberHasImage(image.MemberId);
            if (memberHasImage)
            {
                await Context
                    .Update<MemberImage>()
                    .Set(x => x.ImageData, image.ImageData)
                    .Set(x => x.MimeType, image.MimeType)
                    .Where(x => x.MemberId).EqualTo(image.MemberId)
                    .ExecuteAsync();
            }
            else
            {
                await AddMemberImage(image);
            }
        }

        public async Task UpdateMemberPassword(MemberPassword password)
        {
            await Context
                .Update<MemberPassword>()
                .Set(x => x.Password, password.Password)
                .Set(x => x.Salt, password.Salt)
                .Where(x => x.MemberId).EqualTo(password.MemberId)
                .ExecuteAsync();
        }

        public async Task UpdateMemberProperties(Guid memberId, IEnumerable<MemberProperty> memberProperties)
        {
            foreach (MemberProperty memberProperty in memberProperties)
            {
                if (memberProperty.Id == Guid.Empty)
                {
                    await Context
                        .Insert(memberProperty)
                        .ExecuteAsync();
                }
                else
                {
                    await Context
                        .Update<MemberProperty>()
                        .Set(x => x.Value, memberProperty.Value)
                        .Where(x => x.Id).EqualTo(memberProperty.Id)
                        .ExecuteAsync();
                }
            }
        }

        public async Task UpdateMemberSubscription(MemberSubscription memberSubscription)
        {
            await Context
                .Update<MemberSubscription>()
                .Set(x => x.Type, memberSubscription.Type)
                .Set(x => x.ExpiryDate, memberSubscription.ExpiryDate)
                .Where(x => x.MemberId).EqualTo(memberSubscription.MemberId)
                .ExecuteAsync();
        }

        private SqlConditionalQuery<Member> MembersQuery(bool searchAll)
        {
            SqlSelectQuery<Member> query = Context.Select<Member>();
            if (searchAll)
            {
                return query;
            }

            return query
                .Where(x => x.Activated).EqualTo(true)
                .Where(x => x.Disabled).EqualTo(false)
                .Where<MemberSubscription, SubscriptionType>(x => x.Type).NotEqualTo(SubscriptionType.Alum);
        }

        private async Task<bool> MemberHasImage(Guid memberId)
        {
            return await Context
                .Select<MemberImage>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .CountAsync() > 0;
        }
    }
}
