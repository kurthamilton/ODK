namespace ODK.Core.Members;

public interface IMemberRepository
{
    Task ActivateMemberAsync(Guid memberId);
    Task AddActivationTokenAsync(MemberActivationToken token);
    Task AddEmailAddressUpdateTokenAsync(MemberEmailAddressUpdateToken token);
    Task AddMemberImageAsync(MemberImage image);
    Task AddMemberSubscriptionRecordAsync(MemberSubscriptionRecord record);
    Task AddPasswordResetRequestAsync(Guid memberId, DateTime created, DateTime expires, string token);
    Task<Guid> CreateMemberAsync(Member member);
    Task DeleteActivationTokenAsync(Guid memberId);
    Task DeleteEmailAddressUpdateTokenAsync(Guid memberId);
    Task DeleteMemberAsync(Guid memberId);
    Task DeletePasswordResetRequestAsync(Guid passwordResetRequestId);
    Task DisableMemberAsync(Guid id);
    Task EnableMemberAsync(Guid id);
    Task<Member?> FindMemberByEmailAddressAsync(string emailAddress);
    Task<Member?> GetMemberAsync(Guid memberId, bool searchAll = false);
    Task<MemberActivationToken?> GetMemberActivationTokenAsync(string activationToken);
    Task<MemberEmailAddressUpdateToken?> GetMemberEmailAddressUpdateTokenAsync(Guid memberId);
    Task<MemberImage?> GetMemberImageAsync(Guid memberId);
    Task<MemberPassword?> GetMemberPasswordAsync(Guid memberId);
    Task<IReadOnlyCollection<MemberProperty>> GetMemberPropertiesAsync(Guid memberId);
    Task<IReadOnlyCollection<Member>> GetMembersAsync(Guid chapterId, bool searchAll = false);
    Task<MemberSubscription?> GetMemberSubscriptionAsync(Guid memberId);
    Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptionsAsync(Guid chapterId);
    Task<long> GetMembersVersionAsync(Guid chapterId, bool searchAll = false);
    Task<MemberPasswordResetRequest?> GetPasswordResetRequestAsync(string token);
    Task UpdateMemberAsync(Guid memberId, bool emailOptIn, string firstName, string lastName);
    Task UpdateMemberEmailAddressAsync(Guid memberId, string emailAddress);
    Task UpdateMemberImageAsync(MemberImage image);
    Task UpdateMemberPasswordAsync(MemberPassword password);
    Task UpdateMemberPropertiesAsync(Guid memberId, IEnumerable<MemberProperty> memberProperties);
    Task UpdateMemberSubscriptionAsync(MemberSubscription memberSubscription);
}