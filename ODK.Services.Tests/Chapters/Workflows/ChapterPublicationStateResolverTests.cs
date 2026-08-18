using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Chapters.Workflows;

namespace ODK.Services.Tests.Chapters.Workflows;

[Parallelizable]
public static class ChapterPublicationStateResolverTests
{
    [Test]
    public static void Resolve_NotApproved_ReturnsDraft()
    {
        // Arrange
        var context = Context(approved: false, published: false);

        // Act
        var result = new ChapterPublicationStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterPublicationState.Draft);
    }

    [Test]
    public static void Resolve_ApprovedButNotPublished_ReturnsApproved()
    {
        // Arrange
        var context = Context(approved: true, published: false);

        // Act
        var result = new ChapterPublicationStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterPublicationState.Approved);
    }

    [Test]
    public static void Resolve_ApprovedAndPublished_ReturnsPublished()
    {
        // Arrange
        var context = Context(approved: true, published: true);

        // Act
        var result = new ChapterPublicationStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterPublicationState.Published);
    }

    [Test]
    public static void Resolve_PublishedWithoutApproval_ReturnsDraft()
    {
        /* Arrange - a combination nothing should produce, since publishing requires approval. Approval is the
           outer gate, so it reads as a draft: a group nobody can reach. */
        var context = Context(approved: false, published: true);

        // Act
        var result = new ChapterPublicationStateResolver().Resolve(context);

        // Assert
        result.Should().Be(ChapterPublicationState.Draft);
    }

    [Test]
    public static void Resolve_EveryCombinationOfTheDatesItReads_ReturnsOneDeclaredState()
    {
        /* Arrange - derived state has to be total: nothing stores it, so both dates being set or not, in every
           combination, has to land on exactly one state. */
        var resolver = new ChapterPublicationStateResolver();
        var contexts = new List<ChapterPublicationContext>();
        foreach (var approved in new[] { false, true })
        {
            foreach (var published in new[] { false, true })
            {
                contexts.Add(Context(approved, published));
            }
        }

        // Act
        var results = contexts.Select(resolver.Resolve).ToArray();

        // Assert
        contexts.Should().HaveCount(4);
        results.Should().OnlyContain(x => x != ChapterPublicationState.None);
    }

    private static ChapterPublicationContext Context(bool approved, bool published) => new()
    {
        Chapter = new Chapter
        {
            ApprovedUtc = approved ? DateTime.UtcNow : null,
            Id = Guid.NewGuid(),
            Name = "E2E",
            Slug = "e2e",
            PublishedUtc = published ? DateTime.UtcNow : null
        },
        Request = Mock.Of<IServiceRequest>()
    };
}
