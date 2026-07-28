using ODK.E2E.Data.Models;

namespace ODK.E2E.Tests.Helpers;

/// <summary>
/// Shared read-only backdrop for the member-facing page smoke tests: a published chapter, a member of
/// it, and one published event. Provisioned once per platform and only navigated to (never mutated), so
/// it is safe to share across the smoke fixtures' parallel runs.
/// </summary>
internal sealed record SmokeContext(TestGroup Group, TestAccount Member, TestEvent Event);
