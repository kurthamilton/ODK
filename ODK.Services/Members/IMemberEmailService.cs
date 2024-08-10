using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberEmailService
{
    Task SendActivationEmail(Chapter? chapter, Member member, string activationToken);

    Task SendAddressUpdateEmail(Chapter? chapter, Member member, string newEmailAddress, string token);
}
