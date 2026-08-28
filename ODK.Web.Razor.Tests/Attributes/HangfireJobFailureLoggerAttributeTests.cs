using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ODK.Services.Logging;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Tests.Attributes;

/// <summary>
/// Pins that a job which has exhausted its retries is reported, and reported with enough to act on.
/// </summary>
/// <remarks>
/// A job failing silently is the worst case for anything that relies on retries to finish its work - the
/// settlement read, most of all, where nothing else would ever say the money was not accounted for.
/// </remarks>
[Parallelizable]
public static class HangfireJobFailureLoggerAttributeTests
{
    [Test]
    public static void OnStateApplied_FailedState_RecordsTheException()
    {
        // Arrange
        var exception = new InvalidOperationException("Payment abc has not finished settling");
        var loggingService = new Mock<ILoggingService>();

        // Act
        CreateFilter(loggingService).OnStateApplied(
            CreateContext(new FailedState(exception)), Mock.Of<IWriteOnlyTransaction>());

        // Assert
        loggingService.Verify(
            x => x.Error(exception, It.IsAny<IDictionary<string, string?>>()),
            Times.Once);
    }

    [Test]
    public static void OnStateApplied_FailedState_RecordsWhichJobFailed()
    {
        // Arrange
        var loggingService = new Mock<ILoggingService>();

        // Act
        CreateFilter(loggingService).OnStateApplied(
            CreateContext(new FailedState(new InvalidOperationException("boom"))),
            Mock.Of<IWriteOnlyTransaction>());

        // Assert - the properties are what make a failure identifiable once it is in the Errors table
        loggingService.Verify(
            x => x.Error(
                It.IsAny<Exception>(),
                It.Is<IDictionary<string, string?>>(p =>
                    p["HANGFIRE.JOBID"] == "42" &&
                    p["HANGFIRE.JOBTYPE"] == nameof(TestJob) &&
                    p["HANGFIRE.JOBMETHOD"] == nameof(TestJob.Run))),
            Times.Once);
    }

    [Test]
    public static void OnStateApplied_StateOtherThanFailed_RecordsNothing()
    {
        // Arrange - every state transition passes through here, and only the last one is a failure
        var loggingService = new Mock<ILoggingService>();

        // Act
        CreateFilter(loggingService).OnStateApplied(
            CreateContext(new SucceededState(result: null, latency: 0, performanceDuration: 0)),
            Mock.Of<IWriteOnlyTransaction>());

        // Assert
        loggingService.VerifyNoOtherCalls();
    }

    private static ApplyStateContext CreateContext(IState newState)
        => new ApplyStateContext(
            Mock.Of<JobStorage>(),
            Mock.Of<IStorageConnection>(),
            Mock.Of<IWriteOnlyTransaction>(),
            new BackgroundJob("42", Job.FromExpression(() => TestJob.Run()), DateTime.UtcNow),
            newState,
            oldStateName: ProcessingState.StateName);

    private static HangfireJobFailureLoggerAttribute CreateFilter(Mock<ILoggingService> loggingService)
    {
        var services = new ServiceCollection()
            .AddScoped(_ => loggingService.Object)
            .BuildServiceProvider();

        return new HangfireJobFailureLoggerAttribute(services.GetRequiredService<IServiceScopeFactory>());
    }

    public static class TestJob
    {
        public static void Run()
        {
        }
    }
}
