# ODK.E2ETests

End-to-end browser tests for the **Group Squirrel** platform, using **Playwright** + **NUnit**.

These are black-box tests: they drive a *running* instance of the app with a real browser and read
the database only to obtain the account-activation token (an E2E test can't open the activation
email). They do **not** reference the app's projects.

## What's covered

- `AccountFlowTests.CreateAccount_ActivateAndLogIn_Succeeds` — signs up on `/account/create`,
  activates via the emailed token, sets a password, and logs in. It then asserts the registration
  emails were sent: the pipeline records every email in `SentEmails` right after the client (the e2e
  `ConsoleEmailClient`) reports success, so the test reads that table back for the activation and
  welcome emails to the member.

## Avoiding real remote services (a dedicated `e2e` environment)

The E2E instance runs as a **separate process** under a dedicated environment name, `e2e`. Standard
.NET config layering (base `appsettings.json` + `appsettings.e2e.json`) then turns off the external
services — no per-request routing and virtually no production code involved.

`appsettings.e2e.json` overrides just what's needed:

- `Emails:UseConsoleClient: true` — emails are logged via `ConsoleEmailClient` instead of sent (no
  Brevo quota). The client is chosen once at startup from this flag.
- `Hibp:Enabled: false` — the Have I Been Pwned password-breach check during activation is skipped (no
  external call, and any password passes).
- `Hangfire:InMemory: true` — the email queue runs in-memory (no Hangfire DB schema needed).
- `ConnectionStrings:Default` — the local dev database (the cleanup only removes `@e2e.odk.test`
  members, so it won't disturb real data).

Because the `e2e` environment does **not** load `appsettings.Development.json`, `appsettings.e2e.json`
carries the handful of values it needs on top of base `appsettings.json` (which already has every key).
Point it at a test DB by editing its connection string.

## Prerequisites

1. **The test database** must be the same one the running app uses — the tests read the activation
   token from it (both `appsettings.e2e.json` and this project's `appsettings.json` point at the local
   dev `odk` DB).
2. **Playwright browsers** installed once (after the first build):

   ```bash
   powershell -File ODK.E2ETests\bin\Debug\net10.0\playwright.ps1 install
   ```

   (Use `pwsh` instead of `powershell` if you have PowerShell 7 installed, or `playwright install` if
   the CLI is on your PATH.)

## Running

The app and the tests run as separate processes. Two ways to drive them:

- **One command (recommended):** `run.e2e.gs.bat` (repo root) opens a single Windows Terminal window
  with two tabs — one running the app in the `e2e` environment on `http://localhost:8125` (console
  email, HIBP off, in-memory Hangfire, local dev DB — see above), one that waits for the app, runs the
  tests, then stops the app. Port 8125 lets it run alongside a normal dev instance (`run.app.gs.bat`
  on 8123).
- **Manually:** start the app with `run.app.gs.e2e.bat`, then in another terminal run
  `dotnet test ODK.E2ETests/ODK.E2ETests.csproj --filter "TestCategory=E2E"` (the tests are
  `[Explicit]`, so the filter is required to include them). The tests target `:8125`.

The generic wait→test→stop orchestration lives in `run.e2e.tests.bat <port> <test-csproj>`, so a
future ODK E2E suite can reuse it with its own app-start script.

## Configuration

Settings live in `appsettings.json` (copied to the output directory):

```json
{
  "BaseUrl": "http://localhost:8125",
  "ConnectionString": "server=.\\SQLEXPRESS;database=odk;..."
}
```

- For machine-specific overrides that shouldn't be committed, add `appsettings.local.json` (same keys) — it's optional and git-ignored.
- For CI, override via environment variables prefixed `ODK_E2E_`: `ODK_E2E_BaseUrl` and `ODK_E2E_ConnectionString` (no internal underscores — those are treated as config nesting).

| Key | `appsettings.json` | Env override | Purpose |
|---|---|---|---|
| `BaseUrl` | `http://localhost:8125` | `ODK_E2E_BaseUrl` | Base URL of the running GS instance (the E2E port) |
| `ConnectionString` | local dev connection string | `ODK_E2E_ConnectionString` | DB to read activation tokens from |

Playwright's own options (headed mode, browser, slow-mo) are set the standard way, e.g.:

```bash
HEADED=1 BROWSER=chromium dotnet test ODK.E2ETests/ODK.E2ETests.csproj --filter "TestCategory=E2E"
```

## Notes / gotchas

- **Real data & cleanup:** each run creates a real member (unique `e2e-<guid>@e2e.odk.test`, a
  dedicated domain) and activates it. A namespace-level `[SetUpFixture]` (`E2ECleanupFixture`) deletes
  every member on that domain in `[OneTimeTearDown]` after all tests finish — which NUnit always runs,
  whether tests pass, fail, or error. Member foreign keys are `ON DELETE CASCADE`, so removing the
  member removes its related rows; `SentEmails` has no FK to members, so its rows for the test domain
  are deleted explicitly in the same step. Still, run against a disposable/dev database.
- **Password breach check:** the `e2e` environment disables the Have I Been Pwned check
  (`Hibp:Enabled: false`), so activation makes no external call and the test's random password passes.
- **Selectors** target stable hooks (`data-firstname`, `#Password`, wizard page ids `#wizard-1..4`);
  if the sign-up wizard markup changes, update `Pages/*`.
- These tests are intentionally **excluded from the normal unit-test run** — they need a live app and
  browsers, so run them explicitly (or in a dedicated CI job).
