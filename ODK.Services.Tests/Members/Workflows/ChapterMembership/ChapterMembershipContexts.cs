using System;
using Moq;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services;
using ODK.Services.Members.Workflows.ChapterMembership;

namespace ODK.Services.Tests.Members.Workflows.ChapterMembership;

/// <summary>
/// A context carrying only what a guard or the state resolver reads, so a test of one of those states the two
/// or three values it cares about and nothing else. Everything a step would need is left empty.
/// </summary>
internal static class ChapterMembershipContexts
{
    public static ChapterMembershipContext Create(
        Guid chapterId,
        Member member,
        MemberChapterInvite? invite = null,
        bool approvalRequired = false) => new()
    {
        AdminMembers = [],
        ApprovalRequired = approvalRequired,
        ChapterId = chapterId,
        ChapterProperties = [],
        Invite = invite,
        Member = member,
        MemberCount = 0,
        MemberProperties = [],
        NotificationSettings = [],
        OwnerSubscriptionFeatures = [],
        Platform = PlatformType.DrunkenKnitwits,
        Properties = [],
        Request = Mock.Of<IChapterServiceRequest>()
    };
}
