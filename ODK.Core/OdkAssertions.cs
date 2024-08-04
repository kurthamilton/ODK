using System.Diagnostics.CodeAnalysis;
using ODK.Core.Exceptions;
using ODK.Core.Members;

namespace ODK.Core;

public static class OdkAssertions
{
    public static T Exists<T>([NotNull] T? value) => value ?? throw new OdkNotFoundException();

    public static T BelongsToChapter<T>([NotNull] T? value, Guid chapterId) where T : IChapterEntity
        => MeetsCondition(value, x => x.ChapterId == chapterId);    

    public static T MeetsCondition<T>([NotNull] T? value, Func<T, bool> condition) 
        => value != null && condition(value) ? value : throw new OdkNotFoundException();

    public static Member MemberOf([NotNull] Member? member, Guid chapterId)
        => MeetsCondition(member, x => x.IsMemberOf(chapterId));
}
