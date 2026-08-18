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

Scripts open a Windows Terminal with three tabs (app + tests + ngrok) and tear the app down after:

- `script.run.tests.default.bat` — Default platform only
- `script.run.tests.dk.bat` — DrunkenKnitwits only
- `script.run.tests.bat [Default|DrunkenKnitwits|E2E]` — generic (default `E2E` = both)

Under the hood: `dotnet test --filter "TestCategory=<platform>"`. `script.e2e.bat <port> <csproj> [category]`
is the generic wait-for-ready → run → kill-port runner. The ngrok tab is left running — ngrok owns its
console, so tearing it down from another tab just garbles the output; close the terminal window when
done. Its tunnel config lives in the gitignored root `ngrok.yml` (see the root README).

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

## Categories

Three axes, composed by the filter:

- **Platform** — `Default` / `DrunkenKnitwits`, from the base class (plus `E2E` on everything).
- **Workflow** — one category per state machine in the app's `**/Workflows` folders, named for the machine's
  class: `AccountStateMachine` → `AccountWorkflows`. A test belongs to a workflow when the behaviour it
  asserts is an *edge* of that machine — whichever platform it runs on and whoever drives it — so these cut
  across the other two axes deliberately. Keeping the names in step with the class names is the whole point:
  "run the tests for the machine I just changed" stays a mechanical question rather than a judgement.
  - **`AccountWorkflows`** — every route to an account that can sign in, on both platforms, applied at
    *fixture* level: `AccountFlowTests` (Group Squirrel sign-up → activate → log in),
    `DrunkenKnitwitsAccountFlowTests` (where signing up is joining the chapter) and
    `DrunkenKnitwitsInvitedMemberTests` (an imported member accepting an invitation). Worth isolating because
    account creation is what nearly every other fixture provisions through, so it is the first thing to run
    when a change might have broken sign-up — and because the invited flows branch four ways on state a test
    has to arrange (invited or not, address kept or changed, account or no account).
  - **`ChapterMembershipWorkflows`** — every route into a group. `GroupTests` and the two DrunkenKnitwits
    fixtures above carry it at *fixture* level; the two `JoinChapter_*` tests in `MemberProfileTestsBase`
    carry it at *method* level, because a group's required questions are a step on the Join transition while
    the fixture's other six tests are about the member page.
  - **`ChapterPublicationWorkflows`** — a group becoming findable: the site admin approving it
    (`SiteAdminTests`) and its owner publishing it (`GroupOwnerTests`). Applied at *method* level, because
    both fixtures are named for an **actor** rather than for a workflow and so will attract unrelated tests —
    a fixture-level category would swallow them silently. `GroupTests`'s not-approved and not-published cases
    stay out of it: they assert that the *join* is blocked, which is an edge of the membership machine, and
    the publication machine has no edge for being read.
- **Capability** — added where a subset is worth running on its own:
  - **`Stripe`** — the four payment fixtures (site/chapter purchase, recurring renewal, cancellation),
    applied at *fixture* level. These are the slow ones — real Stripe calls, webhook round-trips via the
    ngrok tunnel, test clocks — so being able to run or skip them separately matters.
  - **`Venues`** — the venue-admin scenarios (creation, name normalising, slug collisions) plus the
    events-list venue filter, which drives the slug through the query string. Applied at *method* level
    because they live in `EventTestsBase` alongside the event tests. Deliberately only the venue-focused
    tests: most event and RSVP tests create a venue while arranging, so including everything that touches
    one would cover most of the suite and the filter would stop meaning anything.
  - **`SiteQuestions`** — site FAQ admin (create, reorder, edit, delete) and the About page that displays
    it, applied at *fixture* level. Group Squirrel only: site questions are per-platform and Drunken
    Knitwits has none, which is exactly why its About page 404s.
  - **`EmailAdmin`** — a group customising its email templates, applied at *fixture* level. Each test
    provisions its own group plus a subscription carrying the CustomEmails feature, so the fixture is slow
    to arrange, and the behaviour it covers is fiddly enough to iterate on: subject and body are customised
    independently, and the form's state is driven by client script.

```
script.run.tests.bat                              # prompts for a category
script.run.tests.bat AccountWorkflows             # every route to an account
script.run.tests.bat ChapterMembershipWorkflows   # every route into a group
script.run.tests.bat ChapterPublicationWorkflows  # approving and publishing a group
script.run.tests.bat Stripe                       # just the payment tests
script.run.tests.bat Venues                       # just the venue admin tests
script.run.tests.bat SiteQuestions                # just the site FAQ tests
script.run.tests.bat EmailAdmin                   # just the email customisation tests
script.run.tests.bat Default                      # one platform
script.run.tests.bat NoStripe                     # everything except payments - skips the slow ones
```

A bare name is wrapped as `TestCategory=<name>`; anything mentioning `TestCategory` is used verbatim.
`NoStripe` is an alias for `TestCategory!=Stripe`, expanded inside `script.e2e.bat` - it can't be passed as
a raw filter because **cmd treats `=` as an argument delimiter**, so `TestCategory!=Stripe` arrives split
into two arguments. Same reason a filter using `&` (AND) can't go through the scripts. For either, call
`dotnet test` directly:

```
dotnet test ODK.E2E.Tests\ODK.E2E.Tests.csproj --filter "TestCategory=Stripe&TestCategory=Default"
```

Add a capability category when a group of tests is slow, needs extra setup, or is worth isolating while
iterating - not for every feature, or filtering stops meaning anything. **A workflow category is not that
judgement:** a state machine gets one, and every test asserting one of its edges carries it, so a new machine
means a new category and the E2E suite tracks the app's workflows rather than a curated selection of them.

## Conventions

- **Repo-wide C# style applies here too.** The "Conventions & style" section of the root
  [`CLAUDE.md`](../CLAUDE.md) governs this solution as well — file-scoped namespaces, `using` directives
  over fully-qualified names, one top-level type per file, **member ordering within a type** (by kind,
  then access/static, then alphabetical), no trailing whitespace, `required` init props, and the rest.
  The points below are E2E-specific *additions*, not a replacement.
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
- **Waiting for a form submit takes two waits, not one.** Match the POST that **navigates**
  (`r.Request.ResourceType == "document"`), not any POST: a page whose fields are validated by an XHR - the
  email template body posts itself to a validate endpoint on change - fires a POST of its own, so an
  any-POST wait returns before the form has submitted and the assertions then read the database as it was.
  **Then wait for the document `GET` that follows.** Every form here is Post/Redirect/Get, so the POST
  response *is* the 302 and arrives while the redirected GET is still in flight; `WaitForLoadStateAsync` right
  after it just re-reports the page being left. A caller that then navigates to the same URL has its
  navigation cut short by the one already running - `Navigation to X is interrupted by another navigation to X`.
  Register the GET waiter *before* the click so it cannot be missed. `ChapterEmailAdminPage.Submit` is the
  pattern.
- **Don't select on text a page's chrome also carries.** The header renders "Sign in" on every anonymous page
  and the group menu renders "Join", so `a:has-text('Sign in')` resolves to two elements: an *action* on it
  throws a strict-mode violation, and - worse - a `CountAsync() > 0` presence check quietly passes on the
  chrome alone, whether or not the thing under test rendered. Add a `data-*` hook to the element the test means
  (`[data-invite-signin]`) rather than narrowing by ancestor. `button:has-text(...)` is usually safe because
  the chrome's equivalents are links.
- **Data helpers** (`ODK.E2E.Data/*DataHelper.cs`): all DB access goes through `E2EQueryBuilder`
  (`Create(sql).AddParameter(...).ExecuteScalar<T>()/ReadMany(...)/ExecuteNonQuery()`), never inline
  `SqlConnection`. **`ExecuteScalar<T>()` gotcha:** for a value-type column that can be null, call it with
  the nullable type — `ExecuteScalar<DateTime?>()`, `ExecuteScalar<int?>()`. With a non-nullable `T`, `T?`
  is just `T`, so a missing value comes back as `default(T)`, not null.
- **An email's *body* is not readable — only its subject.** `SentEmails` records `To`, `Subject` and
  `SentUtc`, and there is no test mail sink, so a link inside an email can never be scraped. Read the token the
  link would carry straight from the database and build the URL in the test: `ActivationTokenDataHelper` does
  that for activation, `MemberChapterInviteDataHelper` for an invitation. Where a subject is the only signal
  available, keep the fragment asserted on in a named constant, since it is seeded wording that may change.
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

## Test isolation: shared vs local provisioning

Provisioning (accounts, chapters, members — each driving the real UI on a browser) dominates run time, so
reusing records provisioned **once** cuts it. But **integrity comes first — never trade correctness for
speed.** A suite that silently false-passes or flakes is far worse than a slow one. **When in any doubt,
provision locally (fresh per test).** Fixtures run in parallel (`[assembly: Parallelizable(ParallelScope.Fixtures)]`
in `AssemblyInfo.cs`; tests within a fixture stay sequential), which makes leaked shared state especially
corrosive — so bias hard toward caution. The static-vs-dynamic-state boundary is genuinely hard to pin down
and **shifts as features are added**: re-check these rules whenever a shared record is touched by new
behaviour, and downgrade to local at the first hint of doubt.

**Local is the default.** Provision fresh per test unless a record clearly qualifies as *shareable
context* under every rule below. The goal is not to share everything — it's to stop re-creating a
plain "chapter of type X" (e.g. free-subscription, published) every single test when the test only needs
it as a backdrop.

**A record may be shared only if ALL hold:**

- **Pure context/actor, never mutated in a test-observable way.** "Mutated" = any change a test could
  assert: name, email, subscription/features, membership approval, profile answers, event settings,
  property set/order. Incidental writes (login timestamp, last-seen) are fine. Any test that changes such
  state works with a **locally-scoped** record.
- **Members: isolate by (platform, chapter, role); never cross platforms** (site admin is the only
  cross-cutting shared account). A shared member has one role in one chapter and is never mutated.
- **Shared parents back only presence assertions on a specific, uniquely-keyed child** — never *count*,
  *order*, *emptiness*, or *absence* of their children (that's whole-state, which other tests' additions
  break). Anything a test adds to a shared parent must be **uniquely keyed (GUID)** to avoid collisions
  under parallelism.
- **Order-independent.** The test must pass under *any* interleaving — never assume a "clean" shared
  record or rely on what ran before. If it can't, it's local.
- **Dynamic / multi-user behaviour is always local.** Anything exercising a record's aggregate or
  dynamic state — attendance/capacity limits, waitlists, "email already in use", reordering, approval
  flows — is scoped per test. Sharing is for static backdrops only.

**Mechanics when you do share:**

- Create **exactly once**, thread-safe, run-scoped — the `SharedAccounts` `Lazy<Task<T>>` pattern.
  Cleaned only by the namespace `[OneTimeTearDown]` (`TestDataCleaner`), **never** per-test/per-fixture,
  or a parallel fixture loses its context mid-run.
- **Share the record, never the browser session** — always a fresh Playwright context per test.
- Route sharing through a **dedicated, obviously-named surface** (`SharedAccounts` today; a
  `SharedChapters`-style registry keyed by (platform, subscription type) for "give me a free-subscription
  published chapter" needs). Sharing is an explicit opt-in; ordinary `Provisioning.*` stays fresh.
- A shared owner (if introduced) needs an **immutable, high-`GroupLimit`** subscription: its owned-chapter
  count only grows, and its features gate real behaviour, so no test may change them.

**Remember the blast radius:** a shared record's provisioning failure fails *every* dependent test at
once, and a single leaked mutation cascades into false results elsewhere. That asymmetry is exactly why
the bar for sharing is high and the default is local.

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
