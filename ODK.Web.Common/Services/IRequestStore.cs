using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services;

namespace ODK.Web.Common.Services;

public interface IRequestStore
{
    Chapter Chapter { get; }

    Chapter? ChapterOrDefault { get; }

    Member CurrentMember { get; }

    Guid CurrentMemberId { get; }

    Guid? CurrentMemberIdOrDefault { get; }

    Member? CurrentMemberOrDefault { get; }

    MemberChapterServiceRequest MemberChapterServiceRequest { get; }

    MemberServiceRequest MemberServiceRequest { get; }

    PlatformType Platform { get; }

    ServiceRequest ServiceRequest { get; }    

    Task<ChapterAdminMember> GetCurrentChapterAdminMember();    

    Task<IRequestStore> Load(ServiceRequest request);

    void Reset();
}