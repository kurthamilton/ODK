using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class MemberRepository : RepositoryBase, IMemberRepository
    {
        public MemberRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task AddMemberGroup(Guid chapterId, string name)
        {
            MemberGroup memberGroup = new MemberGroup(Guid.Empty, chapterId, name);
            await Context.InsertAsync(memberGroup);
        }

        public async Task AddMemberToGroup(Guid memberId, Guid memberGroupId)
        {
            MemberGroupMember memberGroupMember = new MemberGroupMember(memberGroupId, memberId);
            await Context.InsertAsync(memberGroupMember);
        }

        public async Task AddPasswordResetRequest(Guid memberId, DateTime created, DateTime expires, string token)
        {
            MemberPasswordResetRequest request = new MemberPasswordResetRequest(Guid.Empty, memberId, created, expires, token);
            await Context.InsertAsync(request);
        }

        public async Task AddRefreshToken(Guid memberId, string refreshToken, DateTime expires)
        {
            MemberRefreshToken token = new MemberRefreshToken(Guid.Empty, memberId, expires, refreshToken);
            await Context.InsertAsync(token);
        }

        public async Task DeleteMemberGroup(Guid id)
        {
            await Context
                .Delete<MemberGroup>()
                .Where(x => x.Id).EqualTo(id)
                .ExecuteAsync();
        }

        public async Task DeletePasswordResetRequest(Guid passwordResetRequestId)
        {
            await Context
                .Delete<MemberPasswordResetRequest>()
                .Where(x => x.Id).EqualTo(passwordResetRequestId)
                .ExecuteAsync();
        }

        public async Task DeleteRefreshToken(Guid id)
        {
            await Context
                .Delete<MemberRefreshToken>()
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

        public async Task<Member> GetMember(Guid memberId)
        {
            return await Context
                .Select<Member>()
                .Where(x => x.Id).EqualTo(memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<MemberGroup>> GetMemberGroups(Guid chapterId)
        {
            return await Context
                .Select<MemberGroup>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<MemberGroup>> GetMemberGroupsForMember(Guid memberId)
        {
            return await Context
                .Select<MemberGroup>()
                .Join<MemberGroupMember, Guid>(x => x.Id, x => x.MemberGroupId)
                .Where<MemberGroupMember, Guid>(x => x.MemberId).EqualTo(memberId)
                .ToArrayAsync();
        }

        public async Task<MemberImage> GetMemberImage(Guid memberId)
        {
            return await Context
                .Select<MemberImage>()
                .Where(x => x.MemberId).EqualTo(memberId)
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

        public async Task<IReadOnlyCollection<Member>> GetMembers(Guid chapterId)
        {
            return await Context
                .Select<Member>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
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

        public async Task RemoveMemberFromGroup(Guid memberId, Guid memberGroupId)
        {
            await Context
                .Delete<MemberGroupMember>()
                .Where(x => x.MemberGroupId).EqualTo(memberGroupId)
                .Where(x => x.MemberId).EqualTo(memberId)
                .ExecuteAsync();
        }

        public async Task UpdateMember(Guid memberId, string emailAddress, bool emailOptIn, string firstName, string lastName)
        {
            await Context
                .Update<Member>()
                .Set(x => x.EmailAddress, emailAddress)
                .Set(x => x.EmailOptIn, emailOptIn)
                .Set(x => x.FirstName, firstName)
                .Set(x => x.LastName, lastName)
                .Where(x => x.Id).EqualTo(memberId)
                .ExecuteAsync();
        }

        public async Task UpdateMemberGroup(MemberGroup memberGroup)
        {
            await Context
                .Update<MemberGroup>()
                .Set(x => x.Name, memberGroup.Name)
                .Where(x => x.Id).EqualTo(memberGroup.Id)
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
                await Context.InsertAsync(image);
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
                    await Context.InsertAsync(memberProperty);
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

        private async Task<bool> MemberHasImage(Guid memberId)
        {
            return await Context
                .Select<MemberImage>()
                .Where(x => x.MemberId).EqualTo(memberId)
                .CountAsync() > 0;
        }
    }
}
