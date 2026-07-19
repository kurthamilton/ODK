using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members.Tasks;

/// <summary>
/// An outstanding action for a member - typically something skipped during sign-up. Computed from
/// member state by an <see cref="IMemberTaskProvider"/>; the web layer renders its title and link.
/// </summary>
public class MemberTask
{
    public required MemberTaskType Type { get; init; }

    /// <summary>The chapter a chapter-scoped task relates to; null for member-level tasks.</summary>
    public Chapter? Chapter { get; init; }
}
