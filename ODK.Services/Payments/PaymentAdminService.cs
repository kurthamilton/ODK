using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Exceptions;
using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public class PaymentAdminService : OdkAdminServiceBase, IPaymentAdminService
{
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentAdminService(
        IUnitOfWork unitOfWork,
        IPlatformProvider platformProvider) 
        : base(unitOfWork)
    {
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateReconciliation(AdminServiceRequest request, CreateReconciliationModel model)
    {
        var (payments, chapterPaymentSettings, sitePaymentSettings) = await GetSuperAdminRestrictedContent(
            request.CurrentMemberId,
            x => x.PaymentRepository.GetByIds(model.PaymentIds),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.SitePaymentSettingsRepository.GetActive());

        if (payments.Count == 0)
        {
            return;
        }

        if (payments.Any(x => x.ChapterId != request.ChapterId))
        {
            throw new OdkServiceException("Error creating reconciliation: payment chapter mismatch");
        }

        var reconciliationId = Guid.NewGuid();

        decimal total = 0;

        foreach (var payment in payments)
        {
            payment.PaymentReconciliationId = reconciliationId;
            payment.PaymentReconciliationAmount = payment.CalculateReconciliationAmount(sitePaymentSettings.Commission);
            total += payment.PaymentReconciliationAmount.Value;

            _unitOfWork.PaymentRepository.Update(payment);
        }

        var reconciliation = new PaymentReconciliation
        {
            Amount = total,
            ChapterId = request.ChapterId,
            CreatedUtc = DateTime.UtcNow,
            Id = reconciliationId,
            PaymentReference = model.PaymentReference
        };

        _unitOfWork.PaymentReconciliationRepository.Add(reconciliation);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ChapterPaymentsViewModel> GetPayments(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, payments, paymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.PaymentRepository.GetMemberDtosByChapterId(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId));
        
        return new ChapterPaymentsViewModel
        {
            Chapter = chapter,
            Payments = payments
                .OrderByDescending(x => x.Payment.PaidUtc)
                .ToArray(),
            PaymentSettings = paymentSettings,
            Platform = platform
        };
    }

    public async Task<ChapterReconciliationsViewModel> GetReconciliations(AdminServiceRequest request)
    {
        var (reconciliations, pendingReconciliations, paymentSettings, sitePaymentSettings) = await GetSuperAdminRestrictedContent(
            request.CurrentMemberId,
            x => x.PaymentReconciliationRepository.GetByChapterId(request.ChapterId),
            x => x.PaymentRepository.GetMemberDtosPendingReconciliation(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.SitePaymentSettingsRepository.GetActive());        

        foreach (var pendingPayment in pendingReconciliations.Select(x => x.Payment))
        {
            pendingPayment.PaymentReconciliationAmount = pendingPayment.CalculateReconciliationAmount(sitePaymentSettings.Commission);
        }

        var pendingReconciliationAmount = pendingReconciliations
            .Select(x => x.Payment)
            .Sum(x => x.PaymentReconciliationAmount ?? 0);

        return new ChapterReconciliationsViewModel
        {
            PaymentSettings = paymentSettings,
            PendingReconciliations = pendingReconciliations,
            PendingReconciliationsAmount = pendingReconciliationAmount,
            Reconciliations = reconciliations
                .OrderByDescending(x => x.CreatedUtc)
                .ToArray()
        };
    }

    public async Task SetPaymentReconciliationExemption(AdminServiceRequest request, Guid paymentId, bool exempt)
    {
        var payment = await GetSuperAdminRestrictedContent(request,
            x => x.PaymentRepository.GetById(paymentId));

        payment.ExemptFromReconciliation = exempt;

        _unitOfWork.PaymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync();
    }
}
