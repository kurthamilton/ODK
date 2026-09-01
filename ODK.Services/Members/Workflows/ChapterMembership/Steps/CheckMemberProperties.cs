using ODK.Core.Chapters;
using ODK.Core.Workflows;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

/// <summary>The group's own questions, which it can mark required for an application.</summary>
public sealed class CheckMemberProperties : IStep<ChapterMembershipContext>
{
    public static string Description => "checks the group's required questions are answered";

    public static StepKind Kind => StepKind.Decision;

    public Task<StepOutcome> Execute(ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        var values = context.Properties
            .ToDictionary(x => x.ChapterPropertyId, x => (string?)x.Value);

        var missing = context.ChapterProperties
            .GetMissingRequired(values, forApplication: true)
            .Select(x => x.DisplayName)
            .ToArray();

        var outcome = missing.Length > 0
            ? StepOutcome.Fail($"The following properties are required: {string.Join(", ", missing)}")
            : StepOutcome.Continue();

        return Task.FromResult(outcome);
    }
}
