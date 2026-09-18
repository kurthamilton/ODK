namespace ODK.E2E.Data;

/// <summary>
/// Records a group's setup checklist directly, so provisioning can get a group to the point where its
/// owner may submit it for approval without driving every step of the setup UI.
/// </summary>
/// <remarks>
/// A recorded row wins outright over what the app would infer, so writing one resolves its step whatever
/// the group actually has. That is the point: nearly every fixture needs a published group and none of
/// them are about how it got set up, so the steps are recorded here and the submission itself is still
/// driven through the UI. <c>GroupOwnerTests.SubmitGroup_ChecklistComplete_SubmitsForApproval</c> is the
/// one that completes the checklist the way an owner does, and it goes nowhere near this.
/// </remarks>
public class ChapterChecklistDataHelper : DataHelperBase
{
    public ChapterChecklistDataHelper(string connectionString)
        : base(connectionString)
    {
    }

    /// <summary>
    /// Takes every step above submission off the group's checklist: the required ones completed, the
    /// optional ones skipped, which is how an owner would leave them. Creation needs no row - the app
    /// reads that step off the group's own timestamp - and the steps below submission are not in its way.
    /// </summary>
    public async Task ResolveStepsBeforeSubmit(Guid chapterId)
    {
        /* MembershipSettings is only on the checklist of a group whose owner has the MemberSubscriptions
           feature, so most groups never read its row. Written anyway, because which groups have that
           step is the app's business rather than something a caller here should have to know. */
        var completed = new[]
        {
            ChecklistItemTypeIds.Picture,
            ChecklistItemTypeIds.Description,
            ChecklistItemTypeIds.MembershipSettings,
            ChecklistItemTypeIds.PrivacySettings
        };

        var dismissed = new[]
        {
            ChecklistItemTypeIds.Questions,
            ChecklistItemTypeIds.MemberProperties,
            ChecklistItemTypeIds.Topics
        };

        foreach (var type in completed)
        {
            await Record(chapterId, type, completedUtc: DateTime.UtcNow, dismissedUtc: null);
        }

        foreach (var type in dismissed)
        {
            await Record(chapterId, type, completedUtc: null, dismissedUtc: DateTime.UtcNow);
        }
    }

    private async Task Record(Guid chapterId, int checklistItemTypeId, DateTime? completedUtc, DateTime? dismissedUtc)
    {
        // The app records a step the moment it observes one completing, so a row can already be there.
        const string sql =
            """
            UPDATE ChapterChecklistItems
            SET CompletedUtc = @completedUtc, DismissedUtc = @dismissedUtc
            WHERE ChapterId = @id AND ChecklistItemTypeId = @type;

            INSERT INTO ChapterChecklistItems (ChapterId, ChecklistItemTypeId, CompletedUtc, DismissedUtc)
            SELECT @id, @type, @completedUtc, @dismissedUtc
            WHERE NOT EXISTS (
                SELECT 1 FROM ChapterChecklistItems
                WHERE ChapterId = @id AND ChecklistItemTypeId = @type);
            """;

        await using var builder = Builder(sql)
            .AddParameter("@id", chapterId)
            .AddParameter("@type", checklistItemTypeId)
            .AddParameter("@completedUtc", completedUtc ?? (object)DBNull.Value)
            .AddParameter("@dismissedUtc", dismissedUtc ?? (object)DBNull.Value);

        await builder.ExecuteNonQuery();
    }
}
