using NUnit.Framework;

// Run different fixtures in parallel. Tests within a fixture stay sequential, so each fixture's single
// PageTest browser and instance state are never used by two tests at once - only distinct fixtures
// (each with its own instance/browser) overlap. The E2E suite is dominated by UI-driven provisioning
// (each test spins up throwaway browsers to create accounts, groups, events, etc.), so overlapping
// fixtures is a large wall-clock win. LevelOfParallelism caps how many run at once - raise it on a
// beefier host, lower it if browser memory becomes the bottleneck.
[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: LevelOfParallelism(4)]
