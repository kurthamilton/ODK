using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.Core.Members;
using ODK.Data.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class PaymentQueryBuilder : DatabaseEntityQueryBuilder<Payment, IPaymentQueryBuilder>, IPaymentQueryBuilder
{
    public PaymentQueryBuilder(OdkContext context) 
        : base(context)
    {
    }

    protected override IPaymentQueryBuilder Builder => this;

    public IPaymentQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IPaymentQueryBuilder ForMember(Guid memberId)
    {
        Query = Query.Where(x => x.MemberId == memberId);
        return this;
    }

    public IPaymentQueryBuilder ForSite()
    {
        Query = Query.Where(x => x.ChapterId == null);
        return this;
    }

    public IQueryBuilder<PaymentChapterDto> WithChapter()
    {
        var query =
            from payment in Query
            from chapter in Set<Chapter>()
                .Where(x => x.Id == payment.ChapterId)
            select new PaymentChapterDto
            {
                Chapter = chapter,
                Payment = payment
            };

        return ProjectTo(query);
    }

    public IQueryBuilder<PaymentMemberDto> WithMember()
    {
        var query =
            from payment in Query
            from member in Set<Member>()
                .Where(x => x.Id == payment.MemberId)
            select new PaymentMemberDto
            {
                Member = member,
                Payment = payment
            };

        return ProjectTo(query);
    }

    protected override IQueryable<Payment> Set(OdkContext context)
        => base.Set(context)
            .Include(x => x.Currency);
}
