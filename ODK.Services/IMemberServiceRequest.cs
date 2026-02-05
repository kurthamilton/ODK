using ODK.Core.Members;

namespace ODK.Services;

public interface IMemberServiceRequest : IServiceRequest
{
    Member CurrentMember { get; }
}