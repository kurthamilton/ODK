using ODK.Core.Members;
using ODK.Data.Sql;
using ODK.Data.Sql.Queries;

namespace ODK.Data.Repositories;

public class MemberRepository : RepositoryBase, IMemberRepository
{
    public MemberRepository(SqlContext context)
        : base(context)
    {
    }

    public async Task ActivateMemberAsync(Guid memberId)
    {
        await Context
            .Update<Member>()
            .Set(x => x.Activated, true)
            .ExecuteAsync();
    }

    public async Task AddActivationTokenAsync(MemberActivationToken token)
    {
        await Context
            .Insert(token)
            .ExecuteAsync();
    }

    public async Task AddEmailAddressUpdateTokenAsync(MemberEmailAddressUpdateToken token)
    {
        await Context
            .Insert(token)
            .ExecuteAsync();
    }

    public async Task AddMemberImageAsync(MemberImage image)
    {
        await Context
            .Insert(image)
            .ExecuteAsync();
    }

    public async Task AddMemberSubscriptionRecordAsync(MemberSubscriptionRecord record)
    {
        await Context
            .Insert(record)
            .ExecuteAsync();
    }

    public async Task AddPasswordResetRequestAsync(Guid memberId, DateTime created, DateTime expires, string token)
    {
        MemberPasswordResetRequest request = new MemberPasswordResetRequest(Guid.Empty, memberId, created, expires, token);
        await Context
            .Insert(request)
            .ExecuteAsync();
    }
    
    public async Task<Guid> CreateMemberAsync(Member member)
    {
        return await Context
            .Insert(member)
            .GetIdentityAsync();
    }

    public async Task DeleteActivationTokenAsync(Guid memberId)
    {
        await Context
            .Delete<MemberActivationToken>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .ExecuteAsync();
    }

    public async Task DeleteEmailAddressUpdateTokenAsync(Guid memberId)
    {
        await Context
            .Delete<MemberEmailAddressUpdateToken>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .ExecuteAsync();
    }

    public async Task DeleteMemberAsync(Guid memberId)
    {
        await Context
            .Delete<Member>()
            .Where(x => x.Id).EqualTo(memberId)
            .ExecuteAsync();
    }

    public async Task DeletePasswordResetRequestAsync(Guid passwordResetRequestId)
    {
        await Context
            .Delete<MemberPasswordResetRequest>()
            .Where(x => x.Id).EqualTo(passwordResetRequestId)
            .ExecuteAsync();
    }
    
    public async Task DisableMemberAsync(Guid id)
    {
        await Context
            .Update<Member>()
            .Set(x => x.Disabled, true)
            .Where(x => x.Id).EqualTo(id)
            .ExecuteAsync();
    }

    public async Task EnableMemberAsync(Guid id)
    {
        await Context
            .Update<Member>()
            .Set(x => x.Disabled, false)
            .Where(x => x.Id).EqualTo(id)
            .ExecuteAsync();
    }

    public async Task<Member?> FindMemberByEmailAddressAsync(string emailAddress)
    {
        return await Context
            .Select<Member>()
            .Where(x => x.EmailAddress).EqualTo(emailAddress)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Member?> GetMemberAsync(Guid memberId, bool searchAll = false)
    {
        return await MembersQuery(searchAll)
            .Where(x => x.Id).EqualTo(memberId)
            .FirstOrDefaultAsync();
    }
    
    public async Task<MemberActivationToken?> GetMemberActivationTokenAsync(string activationToken)
    {
        return await Context
            .Select<MemberActivationToken>()
            .Where(x => x.ActivationToken).EqualTo(activationToken)
            .FirstOrDefaultAsync();
    }

    public async Task<MemberEmailAddressUpdateToken?> GetMemberEmailAddressUpdateTokenAsync(Guid memberId)
    {
        return await Context
            .Select<MemberEmailAddressUpdateToken>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .FirstOrDefaultAsync();
    }

    public async Task<MemberImage?> GetMemberImageAsync(Guid memberId)
    {
        return await Context
            .Select<MemberImage>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .FirstOrDefaultAsync();
    }

    public async Task<MemberPassword?> GetMemberPasswordAsync(Guid memberId)
    {
        return await Context
            .Select<MemberPassword>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<MemberProperty>> GetMemberPropertiesAsync(Guid memberId)
    {
        return await Context
            .Select<MemberProperty>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<Member>> GetMembersAsync(Guid chapterId, bool searchAll = false)
    {
        return await MembersQuery(searchAll)
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .ToArrayAsync();
    }

    public async Task<MemberSubscription?> GetMemberSubscriptionAsync(Guid memberId)
    {
        return await Context
            .Select<MemberSubscription>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptionsAsync(Guid chapterId)
    {
        return await Context
            .Select<MemberSubscription>()
            .Where<Member, Guid>(x => x.ChapterId).EqualTo(chapterId)
            .ToArrayAsync();
    }

    public async Task<long> GetMembersVersionAsync(Guid chapterId, bool searchAll = false)
    {
        return await MembersQuery(searchAll)
            .Where(x => x.ChapterId).EqualTo(chapterId)
            .VersionAsync();
    }

    public async Task<MemberPasswordResetRequest?> GetPasswordResetRequestAsync(string token)
    {
        return await Context
            .Select<MemberPasswordResetRequest>()
            .Where(x => x.Token).EqualTo(token)
            .FirstOrDefaultAsync();
    }
    
    public async Task UpdateMemberAsync(Guid memberId, bool emailOptIn, string firstName, string lastName)
    {
        await Context
            .Update<Member>()
            .Set(x => x.EmailOptIn, emailOptIn)
            .Set(x => x.FirstName, firstName)
            .Set(x => x.LastName, lastName)
            .Where(x => x.Id).EqualTo(memberId)
            .ExecuteAsync();
    }

    public async Task UpdateMemberEmailAddressAsync(Guid memberId, string emailAddress)
    {
        await Context
            .Update<Member>()
            .Set(x => x.EmailAddress, emailAddress)
            .Where(x => x.Id).EqualTo(memberId)
            .ExecuteAsync();
    }

    public async Task UpdateMemberImageAsync(MemberImage image)
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
            await AddMemberImageAsync(image);
        }
    }

    public async Task UpdateMemberPasswordAsync(MemberPassword password)
    {
        await Context
            .Update<MemberPassword>()
            .Set(x => x.Password, password.Password)
            .Set(x => x.Salt, password.Salt)
            .Where(x => x.MemberId).EqualTo(password.MemberId)
            .ExecuteAsync();
    }

    public async Task UpdateMemberPropertiesAsync(Guid memberId, IEnumerable<MemberProperty> memberProperties)
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

    public async Task UpdateMemberSubscriptionAsync(MemberSubscription memberSubscription)
    {
        await Context
            .Update<MemberSubscription>()
            .Set(x => x.Type, memberSubscription.Type)
            .Set(x => x.ExpiryDate, memberSubscription.ExpiryDate)
            .Where(x => x.MemberId).EqualTo(memberSubscription.MemberId)
            .ExecuteAsync();
    }

    private SqlSelectQuery<Member> MembersQuery(bool searchAll)
    {
        SqlSelectQuery<Member> query = Context.Select<Member>();
        if (searchAll)
        {
            return query;
        }

        return query
            .Where(x => x.Activated).EqualTo(true)
            .Where(x => x.Disabled).EqualTo(false)
            .WhereAny<MemberSubscription, SubscriptionType>(x => x.Type,
                new [] { SubscriptionType.Trial, SubscriptionType.Full, SubscriptionType.Partial });
    }

    private async Task<bool> MemberHasImage(Guid memberId)
    {
        return await Context
            .Select<MemberImage>()
            .Where(x => x.MemberId).EqualTo(memberId)
            .CountAsync() > 0;
    }
}
