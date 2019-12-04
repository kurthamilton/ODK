using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IPaymentProvider _paymentProvider;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IChapterRepository chapterRepository, IPaymentProvider paymentProvider, ICountryRepository countryRepository,
            IPaymentRepository paymentRepository)
        {
            _chapterRepository = chapterRepository;
            _countryRepository = countryRepository;
            _paymentProvider = paymentProvider;
            _paymentRepository = paymentRepository;
        }

        public async Task<Guid> MakePayment(Member member, double amount, string token, string reference)
        {
            ChapterPaymentSettings paymentSettings = await _chapterRepository.GetChapterPaymentSettings(member.ChapterId);
            Chapter chapter = await _chapterRepository.GetChapter(member.ChapterId);
            Country country = await _countryRepository.GetCountry(chapter.CountryId);

            await _paymentProvider.MakePayment(member.EmailAddress, paymentSettings.ApiSecretKey, country.CurrencyCode, amount, token);

            Payment payment = new Payment(Guid.Empty, member.Id, DateTime.UtcNow, country.CurrencyCode, amount, reference);
            return await _paymentRepository.CreatePayment(payment);
        }
    }
}
