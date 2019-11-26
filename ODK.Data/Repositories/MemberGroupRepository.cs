using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Data.Sql;

namespace ODK.Data.Repositories
{
    public class MemberGroupRepository : RepositoryBase, IMemberGroupRepository
    {
        public MemberGroupRepository(SqlContext context)
            : base(context)
        {
        }

        public async Task AddMemberToGroup(Guid memberId, Guid memberGroupId)
        {
            MemberGroupMember memberGroupMember = new MemberGroupMember(memberGroupId, memberId);
            await Context.InsertAsync(memberGroupMember);
        }

        public async Task<MemberGroup> CreateMemberGroup(MemberGroup memberGroup)
        {
            Guid id = await Context.InsertAsync(memberGroup);
            return await GetMemberGroup(id);
        }

        public async Task DeleteMemberGroup(Guid id)
        {
            await Context
                .Delete<MemberGroup>()
                .Where(x => x.Id).EqualTo(id)
                .ExecuteAsync();
        }

        public async Task<MemberGroup> GetMemberGroup(Guid id)
        {
            return await Context
                .Select<MemberGroup>()
                .Where(x => x.Id).EqualTo(id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<MemberGroupMember>> GetMemberGroupMembers(Guid chapterId)
        {
            return await Context
                .Select<MemberGroupMember>()
                .Join<MemberGroup, Guid>(x => x.MemberGroupId, x => x.Id)
                .Where<MemberGroup, Guid>(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<MemberGroup>> GetMemberGroups(Guid chapterId)
        {
            return await Context
                .Select<MemberGroup>()
                .Where(x => x.ChapterId).EqualTo(chapterId)
                .ToArrayAsync();
        }

        public async Task RemoveMemberFromGroup(Guid memberId, Guid memberGroupId)
        {
            await Context
                .Delete<MemberGroupMember>()
                .Where(x => x.MemberGroupId).EqualTo(memberGroupId)
                .Where(x => x.MemberId).EqualTo(memberId)
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

    }
}
