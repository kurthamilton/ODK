using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Services.Authorization;

namespace ODK.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMemberRepository _memberRepository;

        public PaymentService(IChapterRepository chapterRepository, IMemberRepository memberRepository, IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
        }

        public async Task<string> CreatePayment(Guid memberId, Guid subscriptionId, string successUrl, string cancelUrl)
        {
            Member member = await _memberRepository.GetMember(memberId);
            _authorizationService.AssertMemberIsCurrent(member);

            ChapterSubscription chapterSubscription = null;

            ChapterPaymentSettings paymentSettings = await _chapterRepository.GetChapterPaymentSettings(member.ChapterId);
            IPaymentProvider paymentProvider = GetPaymentProvider(paymentSettings.Provider);
            return await paymentProvider.CreatePayment(member.EmailAddress, paymentSettings.ApiSecretKey,
                "CURRENCYCODE", chapterSubscription, "SUCCESS", "CANCEL");
        }

        private IPaymentProvider GetPaymentProvider(string providerName)
        {
            throw new NotImplementedException();
        }
    }
}
