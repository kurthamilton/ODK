using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public async Task AddMemberGroup(int chapterId, string name)
        {
            throw new NotImplementedException();
        }

        public async Task AddMemberToGroup(int memberId, int groupId)
        {
            throw new NotImplementedException();
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

        public async Task DeleteMemberGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public async Task DeletePasswordResetRequest(Guid passwordResetRequestId)
        {
            await Context
                .Delete<MemberPasswordResetRequest>()
                .Where(x => x.Id, passwordResetRequestId)
                .ExecuteAsync();
        }

        public async Task DeleteRefreshToken(Guid id)
        {
            await Context
                .Delete<MemberRefreshToken>()
                .Where(x => x.Id, id)
                .ExecuteAsync();
        }

        public async Task<Member> FindMemberByEmailAddress(string emailAddress)
        {
            return await Context
                .Select<Member>()
                .Where(x => x.EmailAddress, emailAddress)
                .FirstOrDefaultAsync();
        }

        public async Task<Member> GetMember(Guid memberId)
        {
            return await Context
                .Select<Member>()
                .Where(x => x.Id, memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, IReadOnlyCollection<MemberGroup>>> GetMemberGroupMembers(int chapterId)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<MemberGroup>> GetMemberGroups(int chapterId)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyCollection<MemberGroup>> GetMemberGroupsForMember(int memberId)
        {
            throw new NotImplementedException();
        }

        public async Task<MemberImage> GetMemberImage(Guid memberId)
        {
            return await Context
                .Select<MemberImage>()
                .Where(x => x.MemberId, memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<MemberPassword> GetMemberPassword(Guid memberId)
        {
            return await Context
                .Select<MemberPassword>()
                .Where(x => x.MemberId, memberId)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId)
        {
            return await Context
                .Select<MemberProperty>()
                .Where(x => x.MemberId, memberId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Member>> GetMembers(Guid chapterId)
        {
            return await Context
                .Select<Member>()
                .Where(x => x.ChapterId, chapterId)
                .ToArrayAsync();
        }

        public async Task<MemberPasswordResetRequest> GetPasswordResetRequest(string token)
        {
            return await Context
                .Select<MemberPasswordResetRequest>()
                .Where(x => x.Token, token)
                .FirstOrDefaultAsync();
        }

        public async Task<MemberRefreshToken> GetRefreshToken(string refreshToken)
        {
            return await Context
                .Select<MemberRefreshToken>()
                .Where(x => x.RefreshToken, refreshToken)
                .FirstOrDefaultAsync();
        }

        public async Task RemoveMemberFromGroup(int memberId, int groupId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateMember(Guid memberId, string emailAddress, bool emailOptIn, string firstName, string lastName)
        {
            await Context
                .Update<Member>()
                .Set(x => x.EmailAddress, emailAddress)
                .Set(x => x.EmailOptIn, emailOptIn)
                .Set(x => x.FirstName, firstName)
                .Set(x => x.LastName, lastName)
                .Where(x => x.Id, memberId)
                .ExecuteAsync();
        }

        public async Task UpdateMemberGroup(int chapterId, string oldName, string newName)
        {
            throw new NotImplementedException();
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
                    .Where(x => x.MemberId, image.MemberId)
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
                .Where(x => x.MemberId, password.MemberId)
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
                        .Where(x => x.Id, memberProperty.Id)
                        .ExecuteAsync();
                }
            }
        }

        private async Task<bool> MemberHasImage(Guid memberId)
        {
            return await Context
                .Select<MemberImage>()
                .Where(x => x.MemberId, memberId)
                .CountAsync() > 0;
        }
    }
}
