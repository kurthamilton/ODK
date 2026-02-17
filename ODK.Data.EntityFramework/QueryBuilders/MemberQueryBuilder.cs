using Microsoft.EntityFrameworkCore;
using ODK.Core.Members;
using ODK.Data.Core.Members;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class MemberQueryBuilder : QueryBuilder<Member>, IMemberQueryBuilder
{
    internal MemberQueryBuilder(OdkContext context)
        : base(context)
    {
    }

    public IMemberQueryBuilder InChapter(Guid chapterId)
    {
        _query = _query.InChapter(chapterId);
        return this;
    }

    public IQueryBuilder<MemberWithAvatarDto> ProjectTo()
    {
        return new QueryBuilder<MemberWithAvatarDto>(_context)
    }

    protected override IQueryable<Member> Set()
        => base.Set()
            .Include(x => x.Chapters);
}