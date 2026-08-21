namespace ODK.E2E.Data.Models;

/// <summary>
/// The subject a test's site conversation carries, so the rows it leaves on real accounts can be found
/// again. Writing to the site notifies and emails <em>every</em> site admin, which on a database restored
/// from prod includes real ones - and those rows belong to the admin who was told, not to the member who
/// wrote, so nothing cascades them away with the test member.
/// </summary>
/// <remarks>
/// <see cref="TestDataCleaner"/> finds the notifications through the conversation they point at, so those
/// need no prefix. A <c>SentEmails</c> row carries only its recipient and subject, and the recipient is a
/// real address - the subject is the only thing left that leads back here, which is what this is for. A
/// test that starts a thread with a subject of its own making leaks one email row per site admin per run.
/// </remarks>
public static class TestSiteConversations
{
    public const string SubjectPrefix = "e2e-conversation-";

    // The guid keeps it unique across runs and across the tests within one, so a subject identifies exactly
    // one thread; the prefix is what drives cleanup.
    public static string NewSubject() => $"{SubjectPrefix}{Guid.NewGuid():N}";
}
