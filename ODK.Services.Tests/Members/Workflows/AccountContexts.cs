using System;
using Moq;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Services.Members.Workflows;

namespace ODK.Services.Tests.Members.Workflows;

/// <summary>
/// A context carrying only what a guard or the state resolver reads, so a test of one of those states the
/// two or three values it cares about and nothing else. Everything a step would need is left empty.
/// </summary>
internal static class AccountContexts
{
    public static AccountContext Create(
        Guid chapterId,
        Member? member = null,
        MemberChapterInvite? invite = null,
        string? inviteToken = null,
        bool approvalRequired = false,
        PlatformType platform = PlatformType.DrunkenKnitwits) => new()
    {
        AdminMembers = [],
        ApprovalRequired = approvalRequired,
        ChapterId = chapterId,
        ChapterProperties = [],
        Invite = invite,
        InviteToken = inviteToken,
        Member = member,
        MemberCount = 0,
        MemberProperties = [],
        NotificationSettings = [],
        OwnerSubscriptionFeatures = [],
        Platform = platform,
        Properties = [],
        Request = Mock.Of<IChapterServiceRequest>(),
        VerifiedByOAuth = false
    };
}
