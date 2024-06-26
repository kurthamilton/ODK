﻿using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IChapterRepository _chapterRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IChapterRepository chapterRepository, IPaymentProviderFactory paymentProviderFactory,
        ICountryRepository countryRepository, IPaymentRepository paymentRepository)
    {
        _chapterRepository = chapterRepository;
        _countryRepository = countryRepository;
        _paymentProviderFactory = paymentProviderFactory;
        _paymentRepository = paymentRepository;
    }

    public async Task<ServiceResult> MakePayment(Member member, double amount, string cardToken, string reference)
    {
        var paymentSettings = await _chapterRepository.GetChapterPaymentSettings(member.ChapterId);
        if (paymentSettings == null || paymentSettings.Provider == null)
        {
            return ServiceResult.Failure("Payment settings not found");
        }

        var chapter = await _chapterRepository.GetChapter(member.ChapterId);
        var country = await _countryRepository.GetCountryAsync(chapter!.CountryId);

        PaymentProviderType providerType = (PaymentProviderType)Enum.Parse(typeof(PaymentProviderType), paymentSettings.Provider, true);

        IPaymentProvider paymentProvider = _paymentProviderFactory.GetPaymentProvider(providerType);

        ServiceResult paymentResult = await paymentProvider.MakePayment(paymentSettings, country.CurrencyCode, amount, cardToken, reference,
            $"{member.FirstName} {member.LastName}");
        if (!paymentResult.Success)
        {
            return paymentResult;
        }

        Payment payment = new Payment(Guid.Empty, member.Id, DateTime.UtcNow, country.CurrencyCode, amount, reference);
        await _paymentRepository.CreatePayment(payment);

        return ServiceResult.Successful();
    }
}
