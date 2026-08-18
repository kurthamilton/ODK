using System;
using System.IO;
using NUnit.Framework;
using ODK.Services.Members.Workflows;
using ODK.Core.Workflows;

namespace ODK.Services.Tests.Members.Workflows;

/// <summary>
/// Holds the committed diagram to the definition. A transition changed without the page being
/// regenerated fails here, which is what stops the documentation describing a machine the code no
/// longer runs.
/// </summary>
[Parallelizable]
public static class AccountStateMachineDocumentationTests
{
    [Test]
    public static void AccountStateMachine_CommittedDocumentation_MatchesTheDefinition()
    {
        // Arrange
        var definition = AccountStateMachine.Create();
        var path = DocumentPath();

        // Act
        var generated = MarkdownExporter.ToDocument(definition);

        // Assert
        var committed = File.Exists(path) ? File.ReadAllText(path) : null;
        if (Normalise(committed) == Normalise(generated))
        {
            return;
        }

        // Rewritten rather than merely reported, so a definition change is committed rather than retyped.
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, generated.Replace("\n", "\r\n"));

        Assert.Fail(
            $"{path} did not match {AccountStateMachine.Name} and has been regenerated. " +
            "Review the change and commit it.");
    }

    private static string DocumentPath()
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

        return Path.Combine(directory.FullName, "docs", "workflows", "account-creation.md");
    }

    private static string? Normalise(string? value) => value?.Replace("\r\n", "\n");
}
