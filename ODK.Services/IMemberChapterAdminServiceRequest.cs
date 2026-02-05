using ODK.Services.Security;

namespace ODK.Services;

public interface IMemberChapterAdminServiceRequest : IMemberChapterServiceRequest
{
    ChapterAdminSecurable Securable { get; }
}