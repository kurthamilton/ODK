using ODK.Core.Chapters;
using ODK.Core.Referrals;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Referrals.Models;
using ODK.Services.Referrals.ViewModels;

namespace ODK.Services.Referrals;

public class ReferralAdminService : OdkAdminServiceBase, IReferralAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReferralAdminService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<Guid>> CreateCampaign(
        IMemberServiceRequest request, ReferralCampaignUpdateModel model)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        var validationResult = Validate(model);
        if (!validationResult.Success)
        {
            return ServiceResult<Guid>.Failure(validationResult.Message ?? string.Empty);
        }

        var campaign = new ReferralCampaign
        {
            CreatedUtc = DateTime.UtcNow,
            DescriptionHtml = model.DescriptionHtml,
            EmailSubject = model.EmailSubject,
            EmailTextHtml = model.EmailTextHtml,
            ExpiresUtc = ToExpiresUtc(model.ExpiresLocalDate),
            Id = _unitOfWork.NewId(),
            Name = model.Name.NormaliseWhitespace()
        };

        _unitOfWork.ReferralCampaignRepository.Add(campaign);
        await _unitOfWork.SaveChanges();

        return ServiceResult<Guid>.Successful(campaign.Id);
    }

    public async Task<ReferralCampaignAdminPageViewModel> GetCampaignViewModel(
        IMemberServiceRequest request, Guid campaignId)
    {
        var (campaign, referrals) = await GetSiteAdminRestrictedContent(
            request,
            x => x.ReferralCampaignRepository.GetById(campaignId),
            x => x.ReferralRepository.GetByCampaignId(campaignId));

        return new ReferralCampaignAdminPageViewModel
        {
            Campaign = campaign,
            Referrals = referrals,
            TimeZone = request.CurrentMember.TimeZone
        };
    }

    public async Task<ReferralCampaignsAdminPageViewModel> GetCampaignsViewModel(IMemberServiceRequest request)
    {
        var campaigns = await GetSiteAdminRestrictedContent(
            request,
            x => x.ReferralCampaignRepository.GetAllSummaries());

        return new ReferralCampaignsAdminPageViewModel
        {
            Campaigns = campaigns,
            TimeZone = request.CurrentMember.TimeZone
        };
    }

    public async Task<ServiceResult> UpdateCampaign(
        IMemberServiceRequest request, Guid campaignId, ReferralCampaignUpdateModel model)
    {
        var campaign = await GetSiteAdminRestrictedContent(
            request,
            x => x.ReferralCampaignRepository.GetById(campaignId));

        var validationResult = Validate(model);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        campaign.DescriptionHtml = model.DescriptionHtml;
        campaign.EmailSubject = model.EmailSubject;
        campaign.EmailTextHtml = model.EmailTextHtml;
        campaign.ExpiresUtc = ToExpiresUtc(model.ExpiresLocalDate);
        campaign.Name = model.Name.NormaliseWhitespace();

        _unitOfWork.ReferralCampaignRepository.Update(campaign);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    /// <summary>
    /// The instant an expiry date ends, so a campaign expiring "on the 31st" is open throughout the 31st
    /// and closed from midnight. Campaigns are site-wide with no chapter to take a timezone from, so the
    /// site default is used - the boundary is converted once here rather than per row at read time.
    /// </summary>
    private static DateTime? ToExpiresUtc(DateTime? expiresLocalDate)
        => expiresLocalDate?.Date.AddDays(1).ToUtc(Chapter.DefaultTimeZone);

    private static ServiceResult Validate(ReferralCampaignUpdateModel model)
        => string.IsNullOrWhiteSpace(model.Name)
            ? ServiceResult.Failure("Name required")
            : ServiceResult.Successful();
}
