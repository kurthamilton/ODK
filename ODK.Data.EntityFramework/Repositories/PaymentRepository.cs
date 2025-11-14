using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class PaymentRepository : ReadWriteRepositoryBase<Payment>, IPaymentRepository
{
    public PaymentRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Payment> GetAll() => Set().DeferredMultiple();

    public IDeferredQueryMultiple<PaymentChapterDto> GetChapterDtosByMemberId(Guid memberId)
    {
        var query =
            from dto in DtoQuery()
            from chapter in Set<Chapter>()
                .Where(x => x.Id == dto.Payment.ChapterId)
            where dto.Payment.MemberId == memberId
            select new PaymentChapterDto
            {
                Chapter = chapter,
                Currency = dto.Currency,
                Payment = dto.Payment,
                PaymentReconciliation = dto.PaymentReconciliation
            };

        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<PaymentDto> GetMemberChapterPayments(Guid memberId, Guid chapterId)
        => DtoQuery()
            .Where(x => x.Payment.ChapterId == chapterId && x.Payment.MemberId == memberId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<PaymentMemberDto> GetMemberDtosByChapterId(Guid chapterId)
    {
        var query =
            from dto in DtoQuery()
            from member in Set<Member>()
                .Where(x => x.Id == dto.Payment.MemberId)
            where dto.Payment.ChapterId == chapterId
            select new PaymentMemberDto
            {
                Currency = dto.Currency,
                Member = member,
                Payment = dto.Payment,
                PaymentReconciliation = dto.PaymentReconciliation
            };

        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<PaymentMemberDto> GetMemberDtosPendingReconciliation(Guid chapterId)
    {
        var query =
            from dto in DtoQuery()
            from member in Set<Member>()
                .Where(x => x.Id == dto.Payment.MemberId)
            where 
                dto.Payment.ChapterId == chapterId &&
                dto.PaymentReconciliation == null && 
                !dto.Payment.ExemptFromReconciliation
            select new PaymentMemberDto
            {
                Currency = dto.Currency,
                Member = member,
                Payment = dto.Payment,
                PaymentReconciliation = null
            };
        
        return query.DeferredMultiple();
    }

    private IQueryable<PaymentDto> DtoQuery()
    {
        var query =
            from payment in Set()
            from currency in Set<Currency>()
                .Where(x => x.Id == payment.CurrencyId)
            from paymentReconciliation in Set<PaymentReconciliation>()
                .Where(x => x.Id == payment.PaymentReconciliationId)
                .DefaultIfEmpty()
            select new PaymentDto
            {
                Currency = currency,
                Payment = payment,
                PaymentReconciliation = paymentReconciliation
            };

        return query;
    }
}
