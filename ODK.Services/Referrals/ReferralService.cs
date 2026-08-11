using ODK.Core.Emails;
using ODK.Core.Referrals;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Web;

namespace ODK.Services.Referrals;

public class ReferralService : IReferralService
{
    private readonly IEmailService _emailService;
    private readonly IEmailValidationService _emailValidationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProviderFactory _urlProviderFactory;

    public ReferralService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IEmailValidationService emailValidationService,
        IUrlProviderFactory urlProviderFactory)
    {
        _emailService = emailService;
        _emailValidationService = emailValidationService;
        _unitOfWork = unitOfWork;
        _urlProviderFactory = urlProviderFactory;
    }

    public async Task<ServiceResult> CreateReferral(IMemberServiceRequest request, string emailAddress)
    {
        var currentMember = request.CurrentMember;
        emailAddress = emailAddress.Trim();

        var validationResult = await _emailValidationService.Validate(emailAddress, EmailValidationLevel.Full);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (string.Equals(emailAddress, currentMember.EmailAddress, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Failure("You cannot refer yourself");
        }

        var (campaign, existingMember) = await _unitOfWork.RunAsync(
            x => x.ReferralCampaignRepository.GetMostRecentActive(DateTime.UtcNow),
            x => x.MemberRepository.GetByEmailAddress(emailAddress));

        if (campaign == null)
        {
            return ServiceResult.Failure("There is no referral campaign running");
        }

        var referral = _unitOfWork.ReferralRepository.Add(new Referral
        {
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = emailAddress,
            Id = Guid.NewGuid(),
            MemberId = currentMember.Id,
            ReferralCampaignId = campaign.Id
        });

        // Committed before the send, so the referral the email points at is already durable - the email
        // carries its id, and an id that rolled back would arrive as a dead link.
        await _unitOfWork.SaveChangesAsync();

        // Already a member: the referral is recorded, but no email goes out - there is nothing to invite
        // them to. The result is identical to a real referral either way, so the response can't be used to
        // test whether an address holds an account.
        if (existingMember != null)
        {
            return Sent();
        }

        var urlProvider = await _urlProviderFactory.Create(request);

        await _emailService.SendEmail(
            request,
            chapter: null,
            to: [new EmailAddressee(emailAddress, string.Empty)],
            subject: campaign.EmailSubject,
            body: campaign.EmailText,
            parameters: new CustomEmailParameters
            {
                { "member.fullName", currentMember.FullName },
                { "referral.id", referral.Id.ToString() },
                { "group.urls.join", urlProvider.JoinUrl() }
            });

        return Sent();
    }

    private static ServiceResult Sent() => ServiceResult.Successful("Referral sent");
}
