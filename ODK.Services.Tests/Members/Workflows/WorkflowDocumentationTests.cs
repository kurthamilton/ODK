using System;
using System.IO;
using NUnit.Framework;
using ODK.Core.Workflows;
using ODK.Services.Chapters.Workflows;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.ChapterMembership;

namespace ODK.Services.Tests.Members.Workflows;

/// <summary>
/// Holds each committed diagram to its definition. A transition changed without the page being regenerated
/// fails here, which is what stops the documentation describing a machine the code no longer runs.
/// </summary>
[Parallelizable]
public static class WorkflowDocumentationTests
{
    [Test]
    public static void AccountStateMachine_CommittedDocumentation_MatchesTheDefinition() =>
        AssertMatchesCommittedPage("account.md", MarkdownExporter.ToDocument(AccountStateMachine.Create()));

    [Test]
    public static void ChapterPublicationStateMachine_CommittedDocumentation_MatchesTheDefinition() =>
        AssertMatchesCommittedPage(
            "chapter-publication.md",
            MarkdownExporter.ToDocument(ChapterPublicationStateMachine.Create()));

    [Test]
    public static void ChapterMembershipStateMachine_CommittedDocumentation_MatchesTheDefinition() =>
        AssertMatchesCommittedPage(
            "chapter-membership.md",
            MarkdownExporter.ToDocument(ChapterMembershipStateMachine.Create()));

    private static void AssertMatchesCommittedPage(string fileName, string generated)
    {
        var path = Path.Combine(RepositoryRoot(), "docs", "workflows", fileName);

        var committed = File.Exists(path) ? File.ReadAllText(path) : null;
        if (Normalise(committed) == Normalise(generated))
        {
            return;
        }

        // Rewritten rather than merely reported, so a definition change is committed rather than retyped.
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, generated.Replace("\n", "\r\n"));

        Assert.Fail($"{path} did not match its definition and has been regenerated. Review it and commit it.");
    }

    private static string? Normalise(string? value) => value?.Replace("\r\n", "\n");

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "odk.slnx")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("The repository root could not be found from the test assembly");
        }

        return directory.FullName;
    }
}
