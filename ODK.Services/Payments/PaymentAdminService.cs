using ODK.Core.Payments;
using ODK.Data.Core;
using ODK.Services.Exceptions;
using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

public class PaymentAdminService : OdkAdminServiceBase, IPaymentAdminService
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentAdminService(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory) 
        : base(unitOfWork)
    {
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateReconciliation(MemberChapterServiceRequest request, CreateReconciliationModel model)
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

    public async Task<ChapterPaymentsViewModel> GetPayments(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

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

    public async Task<ChapterReconciliationsViewModel> GetReconciliations(MemberChapterServiceRequest request)
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

    public async Task<IReadOnlyCollection<MissingPaymentModel>> GetMissingPayments(Guid currentMemberId)
    {
        var (ourPayments, sitePaymentSettings, currencies) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.PaymentRepository.GetAll(),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.CurrencyRepository.GetAll());
        
        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            sitePaymentSettings, connectedAccountId: null);

        var theirPayments = await paymentProvider.GetAllPayments();

        var ourPaymentsDictionary = ourPayments
            .Where(x => !string.IsNullOrEmpty(x.ExternalId))
            .ToDictionary(x => x.ExternalId ?? "");

        var missingPayments = theirPayments
            .Where(x =>
                !ourPaymentsDictionary.ContainsKey(x.PaymentId) &&
                (x.SubscriptionId == null || !ourPaymentsDictionary.ContainsKey(x.SubscriptionId)))
            .ToArray();

        var testSubId = "sub_1SPivRKsnJKOyzEL51zR0HfO";
        var theirPayments2 = theirPayments
            .Where(x => string.Equals(x.SubscriptionId, testSubId, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var ourPayments2 = ourPayments
            .Where(x => string.Equals(x.ExternalId, testSubId, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var ourPaymentsDictionary2 = ourPayments2
            .Where(x => !string.IsNullOrEmpty(x.ExternalId))
            .ToDictionary(x => x.ExternalId ?? "");

        var missingPayments2 = theirPayments2
            .Where(x =>
                !ourPaymentsDictionary2.ContainsKey(x.PaymentId) &&
                (x.SubscriptionId == null || !ourPaymentsDictionary2.ContainsKey(x.SubscriptionId)))
            .ToArray();

        var emailAddresses = missingPayments
            .Where(x => !string.IsNullOrEmpty(x.CustomerEmail))
            .Select(x => x.CustomerEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToArray();

        var externalIds = missingPayments
            .Select(x => x.PaymentId)
            .ToArray();

        var members = await _unitOfWork.MemberRepository.GetByEmailAddresses(emailAddresses).Run();
        var memberDictionary = members
            .ToDictionary(x => x.EmailAddress, StringComparer.OrdinalIgnoreCase);

        var memberSubscriptionRecords = await _unitOfWork.MemberSubscriptionRecordRepository
            .GetByExternalIds(externalIds)
            .Run();
        var memberSubscriptionRecordDictionary = memberSubscriptionRecords
            .ToDictionary(x => x.ExternalId ?? "", StringComparer.OrdinalIgnoreCase);

        var currencyDictionary = currencies
            .ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        return missingPayments
            .Select(x => new MissingPaymentModel
            {
                Amount = x.Amount,
                Created = x.Created,
                Currency = currencyDictionary[x.Currency],
                Member = !string.IsNullOrEmpty(x.CustomerEmail) && memberDictionary.ContainsKey(x.CustomerEmail) 
                    ? memberDictionary[x.CustomerEmail] 
                    : null,
                MemberEmail = x.CustomerEmail,
                MemberSubscriptionRecord = memberSubscriptionRecordDictionary.ContainsKey(x.PaymentId)
                    ? memberSubscriptionRecordDictionary[x.PaymentId] 
                    : null,
                PaymentId = x.PaymentId,
                SubscriptionId = x.SubscriptionId
            })
            .ToArray();
    }

    public async Task SetPaymentReconciliationExemption(MemberChapterServiceRequest request, Guid paymentId, bool exempt)
    {
        var payment = await GetSuperAdminRestrictedContent(request,
            x => x.PaymentRepository.GetById(paymentId));

        payment.ExemptFromReconciliation = exempt;

        _unitOfWork.PaymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync();
    }
}
