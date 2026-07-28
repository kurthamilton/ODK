using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services;

namespace ODK.Web.Common.Services;

public interface IRequestStore
{
    Chapter Chapter { get; }

    Chapter? ChapterOrDefault { get; }

    IChapterServiceRequest ChapterServiceRequest { get; }

    Member CurrentMember { get; }

    Country? CurrentMemberCountry { get; }

    Member? CurrentMemberOrDefault { get; }

    MemberPreferences? CurrentMemberPreferences { get; }

    bool Loaded { get; }

    IMemberChapterServiceRequest MemberChapterServiceRequest { get; }

    IMemberServiceRequest MemberServiceRequest { get; }

    PlatformType Platform { get; }

    IServiceRequest ServiceRequest { get; }

    Task<ChapterAdminMember?> GetCurrentChapterAdminMember();

    Task<IRequestStore> Load(IHttpRequestContext context, Guid? currentMemberIdOrDefault);

    void Reset();
}