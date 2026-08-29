using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Payments;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentRepository : ReadWriteRepositoryBase<Payment, IPaymentQueryBuilder>, IPaymentRepository
{
    public PaymentRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingle<Payment> GetByCheckoutSessionId(string sessionId)
    {
        var query =
            from payment in Set()
            from session in Set<PaymentCheckoutSession>()
                .Where(x => x.PaymentId == payment.Id)
            where session.SessionId == sessionId
            select payment;

        return query.DeferredSingle();
    }

    public IDeferredQueryMultiple<PaymentChapterDto> GetChapterDtosByMemberId(Guid memberId)
        => Query()
            .ForMember(memberId)
            .WithChapter()
            .GetAll();

    public IDeferredQueryMultiple<Payment> GetMemberChapterPayments(Guid memberId, Guid chapterId)
        => Query()
            .ForChapter(chapterId)
            .ForMember(memberId)
            .GetAll();

    public IDeferredQueryMultiple<PaymentMemberDto> GetMemberDtosByChapterId(Guid chapterId)
        => Query()
            .ForChapter(chapterId)
            .WithMember()
            .GetAll();

    public IDeferredQueryMultiple<Payment> GetSitePaymentsByMemberId(Guid memberId)
        => Query()
            .ForSite()
            .ForMember(memberId)
            .GetAll();

    public override IPaymentQueryBuilder Query() => CreateQueryBuilder(context => new PaymentQueryBuilder(context));
}