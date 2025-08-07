using System.Diagnostics.CodeAnalysis;
using ODK.Core.Exceptions;
using ODK.Core.Members;

namespace ODK.Core;

public static class OdkAssertions
{
    public static T Exists<T>([NotNull] T? value) => Exists(value, "");

    public static T Exists<T>([NotNull] T? value, string message) => value ?? throw new OdkNotFoundException(message);

    public static T BelongsToChapter<T>([NotNull] T? value, Guid chapterId) where T : IChapterEntity
        => MeetsCondition(value, x => x.ChapterId == chapterId);

    public static T MeetsCondition<T>([NotNull] T? value, Func<T, bool> condition) 
        => MeetsCondition(value, condition, "");

    public static T MeetsCondition<T>([NotNull] T? value, Func<T, bool> condition, string message)
        => value != null && condition(value) ? value : throw new OdkNotFoundException(message);

    public static Member MemberOf([NotNull] Member? member, Guid chapterId)
        => MeetsCondition(member, x => x.IsMemberOf(chapterId));
}
