# CLAUDE.md — ODK E2E tests

Guidance for working in the end-to-end test solution (`E2E/odk.e2e.slnx`). Keep this current as
conventions solidify.

## What this is

Black-box, browser-driven end-to-end tests for the ODK app, using **Playwright** (`Microsoft.Playwright.NUnit`)
+ **NUnit** + **FluentAssertions**. The tests drive a *running* instance of the app with a real browser
and read the database only for things a user can't see (activation tokens, sent-email records, outcome
assertions). They **do not reference the app's projects** — production code and these tests never depend
on each other, so this solution is separate from `ODK.slnx`.

The app runs **two platforms** off one codebase, selected by base URL: **`Default`** (working title
"Group Squirrel") and **`DrunkenKnitwits`**. Tests target a platform via a base class + test category
(see below).

## Solution layout

```
ODK.E2E.Core     E2ESettings (config: per-platform base URLs, connection string). No test/UI deps.
ODK.E2E.Data     DB access: E2EQueryBuilder + *DataHelper classes + Models (TestAccount/TestGroup/...).
ODK.E2E.Tests    The tests: fixtures, Pages (page objects), Helpers (provisioning), assets.
```

Dependencies flow `Tests → Data → Core`. Never reference the app's projects from here.

## Running the app for E2E

The app runs in a dedicated ASP.NET environment, **`e2e`** (`appsettings.e2e.json` in `ODK.Web.Razor`):
console email client (no real mail), HIBP breach check off, in-memory Hangfire, and the local dev DB.
`script.run.app.e2e.bat` binds **both** platform ports in one process — `:8125` (Default) and `:8126`
(DrunkenKnitwits) — and `PlatformProvider` resolves the platform from the request URL.

**One-time prerequisite:** install the Playwright browsers after the first build:

```
powershell -File ODK.E2E.Tests\bin\Debug\net10.0\playwright.ps1 install
```

## Running the tests

Scripts open a Windows Terminal with two tabs (app + tests) and tear the app down after:

- `script.run.tests.default.bat` — Default platform only
- `script.run.tests.dk.bat` — DrunkenKnitwits only
- `script.run.tests.bat [Default|DrunkenKnitwits|E2E]` — generic (default `E2E` = both)

Under the hood: `dotnet test --filter "TestCategory=<platform>"`. `script.e2e.bat <port> <csproj> [category]`
is the generic wait-for-ready → run → kill-port runner.

## Targeting a platform

- Fixtures derive from a **platform base class**, never `PageTest` directly:
  - `DefaultPageTest` → `[Category("Default")]`, browser `BaseURL` = the Default port.
  - `DrunkenKnitwitsPageTest` → `[Category("DrunkenKnitwits")]`, `BaseURL` = the DrunkenKnitwits port.
  - Both derive from `OdkPageTest`, which carries `[Category("E2E")]` + `[Explicit]` and sets the
    context `BaseURL` via `ContextOptions()`.
- NUnit inherits `[Category]` from base classes, so a fixture picks up `E2E` + its platform automatically —
  don't repeat those attributes on the fixture. Categories are how the per-platform runs filter.
- Because the base class sets the browser context `BaseURL`, **page objects navigate with relative paths**
  (`page.Navigate("/account/login")`) and automatically hit the correct platform.

## Conventions

- **Page objects** (`Pages/**`): one class per page/flow, constructor takes `IPage`, methods drive one
  journey. Navigate with `page.Navigate(relativePath)` (the `PageExtensions` helper — relative, resolves
  against the context `BaseURL`). Prefer stable selectors: `data-*` hooks, ids, or `button:has-text(...)`.
  A page object shared across both platforms takes the platform-correct relative URL(s) as method
  parameters (it stays platform-agnostic) — build them with `PlatformRoutes` (see below), don't hard-code
  a platform's tree inside the page object.
- **JS-enhanced form controls.** Some fields are enhanced by client JS that hides the native control, so
  Playwright's `FillAsync`/`SelectOptionAsync` can't see them. Drive them via the `PageExtensions` helpers
  instead: `SetEnhancedSelect(selector, value)` for a SlimSelect `<select>` (`[data-select]`/
  `[data-searchable]` — it sets the native value that posts and raises the `change`/`odk:change` events),
  and `SetDatePicker(selector, value)` for a flatpickr date input (`[data-datepicker]`, read-only — sets
  via the flatpickr instance in `dd/MM/yyyy HH:mm`). A TinyMCE textarea (`[data-html-editor]`) is filled
  via `tinymce.get(id).setContent(v); save()` after waiting for init (see `SiteAdminSubscriptionsPage`).
- **Same scenario on both platforms → one abstract base fixture + a thin concrete per platform.** When a
  feature exists on both platforms with identical forms but different route trees/provisioning, write the
  scenario bodies **once** in an `abstract XxxTestsBase : OdkPageTest` (NUnit runs `[Test]` methods from
  the base in each concrete subclass). Each concrete fixture (`XxxTests` `[Category("Default")]`,
  `DrunkenKnitwitsXxxTests` `[Category("DrunkenKnitwits")]`) supplies only `PlatformBaseUrl` and the
  platform-varying bits via `private protected abstract` hooks — provisioning (owner+chapter, member) and
  a `PlatformRoutes` factory. See `EventTestsBase` + `EventTests`/`DrunkenKnitwitsEventTests`. This is the
  way to honour "test cases do the duplication, not multiple methods" when a single fixture can't (the
  fixture's `BaseURL` is fixed per platform, so one method can't target both).
- **`PlatformRoutes`** (`Pages/PlatformRoutes.cs`): builds the platform-correct **relative** admin and
  member-facing URLs (Default `/my/groups/{chapterId}/...` vs DrunkenKnitwits `/{chapterName}/admin/...`,
  whose leaf segments even differ — `/new` vs `/create`). Add a route here rather than composing paths in
  a page object or test. Mirrors the app's `GroupAdminRoutes`/`GroupRoutes`.
- **Data helpers** (`ODK.E2E.Data/*DataHelper.cs`): all DB access goes through `E2EQueryBuilder`
  (`Create(sql).AddParameter(...).ExecuteScalar<T>()/ReadMany(...)/ExecuteNonQuery()`), never inline
  `SqlConnection`. **`ExecuteScalar<T>()` gotcha:** for a value-type column that can be null, call it with
  the nullable type — `ExecuteScalar<DateTime?>()`, `ExecuteScalar<int?>()`. With a non-nullable `T`, `T?`
  is just `T`, so a missing value comes back as `default(T)`, not null.
- **Provisioning** (`Helpers/Provisioning.cs`): builds prerequisite state by driving the real UI on a
  throwaway browser (its own context with `BaseURL` set). `SharedAccounts.GetAsync(role)` caches an
  account per role for the run; use `Provisioning.NewAccountAsync(role)` for a fresh, one-off account
  (e.g. a member that consumes a one-time join).
- **Test data & cleanup:** test members use the dedicated `@e2e.odk.test` domain (`TestAccounts`), and a
  constant policy-compliant password (`TestAccounts.Password`) so later steps can log in. A namespace
  `[SetUpFixture]` (`E2ETestRunFixture`) provisions the site admin in `[OneTimeSetUp]` and, in
  `[OneTimeTearDown]`, `TestDataCleaner` removes everything on that domain (members, their owned
  chapters/memberships, sent emails) — always, pass or fail. Still, run against a disposable/dev DB.
- **No `Async` suffix** on our own async methods (`LogIn`, `CreateGroup`, `Provisioning.NewAccount`, …).
  Library methods keep their names (Playwright's `GotoAsync`/`ClickAsync`, ADO's `ExecuteReaderAsync`,
  interface `DisposeAsync`).
- **Naming:** `Method_Scenario_ExpectedResult`; Arrange/Act/Assert with comments; FluentAssertions
  (`x.Should()...`); one top-level type per file; `required` init props for models.
- **Config:** `ODK.E2E.Tests/appsettings.json` holds `DefaultBaseUrl`, `DrunkenKnitwitsBaseUrl`,
  `ConnectionString`; override per-machine via git-ignored `appsettings.local.json` or `ODK_E2E_*` env vars.

## DrunkenKnitwits specifics

DrunkenKnitwits is chapter-scoped and differs fundamentally from Default, so its fixtures are a *different*
(smaller) suite, not a 1:1 duplicate:

- **No self-service group creation or publish** — those routes are disabled on DrunkenKnitwits, so the
  Default `GroupOwnerTests` (publish) and group-creation flows have no DrunkenKnitwits equivalent.
- **Sign-up IS the chapter join** — `/{chapterName}/account/join` creates the account; activate
  (`/{chapterName}/account/activate`) and login (`/{chapterName}/account/login`) are chapter-scoped. The
  URL segment is the chapter's `ShortName` (derived from `Name`; use a URL-safe, space-free name).
- **No seeded DrunkenKnitwits chapter** — `Provisioning.SeedDrunkenKnitwitsChapterAsync` creates a valid
  chapter through the Default UI (which writes all dependent rows) then flips it to DrunkenKnitwits and
  sets approve/publish state via one SQL `UPDATE` (`ChapterDataHelper.SetDrunkenKnitwitsChapter`). That
  update also appends the `" Drunken Knitwits"` suffix to `Name`: DrunkenKnitwits resolves a chapter
  from its URL by re-appending that suffix and matching on `Name`, so without it the URL 404s. Seed
  only the required dependencies this way; seed extra data (questions, properties, subscriptions) on
  demand in the test that needs it.
- **Site-admin area is platform-agnostic** — `/siteadmin/**` routes are shared across platforms.
