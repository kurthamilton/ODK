using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using ODK.Services;
using ODK.Services.Payments.Models;
using ODK.Services.Tasks;

namespace ODK.Web.Razor.Tests.Tasks;

/// <summary>
/// Holds the set of methods Hangfire can bind a queued job to.
/// </summary>
/// <remarks>
/// <para>
/// A queued job names its method by type and parameter types, and the queue outlives the deploy that filled
/// it - a scheduled event email waits until the event. So these signatures are a published contract that the
/// compiler cannot see: renaming one, moving its type or changing a parameter makes every job already holding
/// it fail to deserialise, which is a production failure with no build or test warning behind it.
/// </para>
/// <para>
/// Changing an entry here is therefore a deliberate act, not a rename that fell out of a refactor. Add the new
/// method beside the old one, leave the old one until the queue has drained of jobs holding it, and only then
/// remove its entry.
/// </para>
/// </remarks>
[Parallelizable]
public static class BackgroundJobSignatureTests
{
    /* The declaring type is the one Hangfire records, which is the compile-time type of the call: an
       expression over an injected `_paymentService` names IPaymentService, and one calling a method on `this`
       names the concrete class. */
    private static readonly JobSignature[] ApprovedJobs =
    [
        new("ODK.Services.Emails.EmailService", "SendQueuedEmailTask", typeof(Guid)),
        new("ODK.Services.Events.EventAdminService", "SendScheduledEmailsJob",
            typeof(JobRequest), typeof(Guid)),
        new("ODK.Services.Events.EventService", "NotifyWaitlistJob", typeof(JobRequest), typeof(Guid)),
        new("ODK.Services.Events.IEventService", "CompleteEventTicketPurchase", typeof(Guid), typeof(Guid)),
        new("ODK.Services.Members.IMemberLocaleService", "UpdateLocale", typeof(Guid), typeof(string)),
        new("ODK.Services.Members.MemberAdminService", "SendImportActivationEmailJob",
            typeof(JobRequest), typeof(Guid), typeof(Guid)),
        new("ODK.Services.Members.MemberAdminService", "SendImportInviteEmailJob",
            typeof(JobRequest), typeof(Guid), typeof(Guid)),
        new("ODK.Services.Payments.PaymentService", "EnsureProductExistsJob", typeof(JobRequest)),
        new("ODK.Services.Payments.PaymentService", "ProcessWebhookActionJob",
            typeof(JobRequest), typeof(PaymentProviderWebhook)),
        new("ODK.Services.Payments.PaymentService", "ProcessWebhookJob",
            typeof(JobRequest), typeof(PaymentProviderWebhook)),
        new("ODK.Services.SocialMedia.SocialMediaService", "ScrapeLatestInstagramPosts",
            typeof(Queue<Guid>), typeof(int)),

        /* Bound only by jobs queued before the JobRequest split, which the work methods above them replaced
           without changing. They stay reachable until the queue has drained of those jobs, and their types
           - ServiceRequest and HttpRequestContext - must stay deserialisable for exactly as long. */
        new("ODK.Services.Events.EventAdminService", "SendScheduledEmails",
            typeof(IServiceRequest), typeof(Guid)),
        new("ODK.Services.Events.EventService", "NotifyWaitlist", typeof(IServiceRequest), typeof(Guid)),
        new("ODK.Services.Events.IEventService", "NotifyWaitlist", typeof(IServiceRequest), typeof(Guid)),
        new("ODK.Services.Members.MemberAdminService", "SendImportActivationEmail",
            typeof(IServiceRequest), typeof(Guid), typeof(Guid)),
        new("ODK.Services.Members.MemberAdminService", "SendImportInviteEmail",
            typeof(IServiceRequest), typeof(Guid), typeof(Guid)),
        new("ODK.Services.Payments.IPaymentService", "EnsureProductExists", typeof(IChapterServiceRequest)),
        new("ODK.Services.Payments.IPaymentService", "ProcessWebhook",
            typeof(IServiceRequest), typeof(PaymentProviderWebhook)),
        new("ODK.Services.Payments.PaymentService", "ProcessWebhookAction",
            typeof(IServiceRequest), typeof(PaymentProviderWebhook))
    ];

    [Test]
    public static void ApprovedJobs_AllStillExist()
    {
        // Arrange - the check that catches a rename or a changed parameter list.
        var missing = ApprovedJobs.Where(x => Resolve(x) == null);

        // Act / Assert
        missing.Should().BeEmpty(
            "a queued job names its method by signature, so anything listed here that no longer exists is a " +
            "job that will fail to deserialise when it is picked up");
    }

    [Test]
    public static void JobBodies_AreAllApproved()
    {
        /* Arrange - the check that catches a new job nobody recorded. Scoped to the naming convention rather
           than to every enqueue site, because an expression tree is not something reflection can follow: a
           job written to the house pattern is found, and one written around it is not. */
        var approved = ApprovedJobs
            .Select(x => $"{x.TypeName}.{x.MethodName}")
            .ToHashSet(StringComparer.Ordinal);

        var jobBodies = typeof(JobRequest).Assembly
            .GetTypes()
            .SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(IsJobBody)
            .Select(x => $"{x.DeclaringType?.FullName}.{x.Name}");

        // Act
        var unapproved = jobBodies.Where(x => !approved.Contains(x));

        // Assert
        unapproved.Should().BeEmpty("every method a job can bind to belongs in ApprovedJobs");
    }

    /* A job body takes a JobRequest and returns the work's Task. The enqueue wrappers match the name and the
       first parameter too, and are told apart by returning the job id rather than a Task. */
    private static bool IsJobBody(MethodInfo method)
        => method.Name.EndsWith("Job", StringComparison.Ordinal)
            && method.ReturnType == typeof(Task)
            && method.GetParameters().FirstOrDefault()?.ParameterType == typeof(JobRequest);

    private static MethodInfo? Resolve(JobSignature signature)
        => typeof(JobRequest).Assembly
            .GetType(signature.TypeName)
            ?.GetMethod(signature.MethodName, signature.ParameterTypes);

    private sealed class JobSignature
    {
        internal JobSignature(string typeName, string methodName, params Type[] parameterTypes)
        {
            MethodName = methodName;
            ParameterTypes = parameterTypes;
            TypeName = typeName;
        }

        internal string MethodName { get; }

        internal Type[] ParameterTypes { get; }

        internal string TypeName { get; }

        public override string ToString()
            => $"{TypeName}.{MethodName}({string.Join(", ", ParameterTypes.Select(x => x.Name))})";
    }
}
