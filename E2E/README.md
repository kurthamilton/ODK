# ODK E2E tests

Black-box, browser-driven end-to-end tests for the ODK app — **Playwright** + **NUnit** +
**FluentAssertions**, in their own solution (`odk.e2e.slnx`). They drive a *running* instance with a real
browser and read the database only for what a user cannot see: activation tokens, invitation tokens, sent-email
records, and outcome assertions.

They **do not reference the app's projects**, which is why this is a separate solution. Two projects:
`ODK.E2E.Tests` (fixtures, page objects, provisioning, `Config/E2ESettings`) and `ODK.E2E.Data` (all database
access), with `Tests → Data`.

> **`CLAUDE.md` in this folder is the detailed guide** — categories, platform targeting, page-object and
> selector conventions, test-isolation rules, and the DrunkenKnitwits specifics. This file is just how to get
> a run going. Anything here that contradicts it is this file being out of date.

## Prerequisites

1. **A database** — the same one the app under test uses, since the tests read tokens from it. Both
   `ODK.Web.Razor/appsettings.e2e.json` and `ODK.E2E.Tests/appsettings.json` point at the local dev `odk`
   database. Test data is cleaned up on the `@e2e.odk.test` domain, but run against a disposable one anyway.
2. **Playwright browsers**, once, after the first build:

   ```
   powershell -File ODK.E2E.Tests\bin\Debug\net10.0\playwright.ps1 install
   ```

   (`pwsh` instead of `powershell` on PowerShell 7, or `playwright install` if the CLI is on your PATH.)
3. **An ngrok tunnel**, only for the `Stripe` category — see the ngrok section in the [root
   README](../README.md).

## Running

```
run.tests.bat                 prompts for a category
run.tests.bat NoStripe        everything except the slow payment tests
run.tests.bat Default         Group Squirrel only
```

That opens one Windows Terminal window with four tabs: an app instance per platform, the test run, and ngrok.
The tests tab waits for both instances to answer, runs the filtered suite, then stops both. A run report
lands at `ODK.E2E.Tests/TestResults/e2e.html` and opens itself when anything failed, with Playwright traces
and screenshots for failures beside it in `TestResults/artifacts`.

**Two app instances, because an instance serves one platform.** The app reads the platform it serves from its
own config, so the suite needs one per platform: the `e2e-gs` launch profile on `:8125` and `e2e-dk` on
`:8126`. A fixture picks its instance through its platform base class, so both start whatever category you
filter to. Those ports also let an E2E run sit alongside an ordinary dev instance (`:8123` / `:8124`).

Each instance runs under an **`--artifacts-path` of its own** (`artifacts/e2e-gs`, `artifacts/e2e-dk`), which
relocates `bin` *and* `obj` for every project in the graph. That is what lets the two run together, and beside
a dev instance: nothing is shared, so both tabs build at once and neither has to be pre-built. See the note in
`Scripts/app/run.bat`.

To drive the pieces yourself:

```
run.app.bat gs                    one instance
e2e.bat 8125+8126 ODK.E2E.Tests\ODK.E2E.Tests.csproj NoStripe
dotnet test ODK.E2E.Tests\ODK.E2E.Tests.csproj --filter "TestCategory=E2E"
```

The fixtures are `[Explicit]`, so a filter is always required — a plain `dotnet test` runs none of them, which
is what keeps them out of the unit-test run.

## The `e2e` environment

An instance runs under the `e2e` environment, so its config is `appsettings.json` → `appsettings.e2e.json`,
plus the environment, port and `Platform` its launch profile supplies (`e2e-gs` / `e2e-dk`, in
`ODK.Web.Razor/Properties/launchSettings.json`, beside the `gs` / `dk` dev pair).
`appsettings.e2e.json` is what turns the outside world off:

- `Emails:UseConsoleClient: true` — emails are logged rather than sent, and still recorded in `SentEmails`,
  which is how a test asserts one was sent. There is no mail sink, so **an email's body is never readable** —
  only its subject.
- `Hibp:Enabled: false` — no breach-check call during activation, so any password passes.
- `Hangfire:InMemory: true` — the job queue is per process, so the two instances cannot run each other's jobs.
- `ConnectionStrings:Default` — the local dev database.
- `Logging:Platforms:*:Path` — a log directory per platform, since each instance is its own process.

**Some of that environment's values are restated in `ODK.E2E.Tests/appsettings.json`, and each pair has to
agree** — these tests cannot read the app's configuration. `Environment`, `SiteSubscriptionCooldownMonths` and
the per-platform Stripe keys are the cases; `CLAUDE.md` explains what breaks when one drifts, and the failures
are silent.

## Configuration

`ODK.E2E.Tests/appsettings.json`, copied to the output directory. Override per machine in the git-ignored
`appsettings.Development.json` / `appsettings.local.json`, or per environment with `ODK_E2E_`-prefixed
variables (`ODK_E2E_ConnectionString`; use `:` for nesting, since `_` is read as a config level).

| Key | Purpose |
|---|---|
| `DefaultBaseUrl` / `DrunkenKnitwitsBaseUrl` | where each platform's instance is listening |
| `ConnectionString` | the app's database, for tokens and outcome assertions |
| `Environment` | the deployment the app runs as, so seeded payment rows carry what it reads back |
| `SiteSubscriptionCooldownMonths` | the app's cooldown, so a test knows which side of it it is arranging |
| `Stripe:Platforms:*:SecretApiKey` / `ConnectedAccountId` | the account the app transacts through |
| `Stripe:WebhookBaseUrl` | the ngrok tunnel Stripe delivers to; blank disables the webhook tests |

Playwright's own options are set the standard way:

```
HEADED=1 BROWSER=chromium dotnet test ODK.E2E.Tests\ODK.E2E.Tests.csproj --filter "TestCategory=Default"
```

## Test data

Test members use the dedicated `@e2e.odk.test` domain. A namespace `[SetUpFixture]` (`E2ETestRunFixture`)
provisions the site admin in `[OneTimeSetUp]` and runs `TestDataCleaner` in `[OneTimeTearDown]` — always,
pass or fail — removing every member on that domain along with the groups, memberships and emails that hang
off them. Anything a test writes against somebody *else* falls outside that cascade and needs its own way
back; see the test-data rules in `CLAUDE.md`.
