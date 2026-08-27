# CLAUDE.md

Guidance for working in this repository. Keep this file current as conventions solidify.

## What this is

ODK is an ASP.NET Core (net10.0) server-rendered web app that runs **two platforms** off the
same codebase, selected by base URL:

- **ODK / Drunken Knitwits** — groups for Drunken Knitwits chapters.
- **Group Squirrel** — a Meetup-style platform, under active development.

Platform-specific behaviour is branched on `PlatformType` (usually a `switch`), so when adding
features consider both platforms — see `GroupAdminRoutes` for the pattern.

## Solution layout

Dependencies flow downward; never reference upward.

```
ODK.Core                    Domain entities, enums, low-level utils. No infrastructure.
ODK.Core.Workflows          State machine framework. Depends on nothing, including ODK.Core.
ODK.Data.Core               Repository/data abstractions.
ODK.Data.EntityFramework    EF Core implementations.
ODK.Resources               Embedded resources (email templates etc.).
ODK.Services                Business logic. The heart of the app.
ODK.Services.Integrations   Third-party integrations.
ODK.Infrastructure          Composition / cross-cutting wiring.
ODK.Web.Common              Web helpers shared across web projects (e.g. route builders).
ODK.Web.Razor               The web app: Razor Pages, MVC controllers, views, wwwroot.
```

Tests: `ODK.Core.Tests`, `ODK.Core.Workflows.Tests`, `ODK.Services.Tests`,
`ODK.Services.Integrations.Tests`.

## Build / test / run

- **Build:** `dotnet build ODK.Web.Razor/ODK.Web.Razor.csproj` (builds the referenced graph).
- **Test:** `dotnet test ODK.Services.Tests/ODK.Services.Tests.csproj` (or the solution).
- **Run locally:** `Scripts/run.app.bat`. One process serves both platforms, resolved from the request URL.
  Requires a local SQL Server restored from a prod backup.
- **CSS:** `.scss` in `wwwroot/scss` compiles to `wwwroot/css`, which is **generated and gitignored** - the
  `BuildClientAssets` csproj target compiles it on every build, so nothing deployed depends on a local
  build being current. `Scripts/run.build.css.bat` (or `npm run build:css` from `ODK.Web.Razor`) recompiles
  it on its own, which is what you want mid-session, since `wwwroot/scss` is not watched. Don't hand-edit
  compiled `.css`, and **don't run a Sass watcher alongside `dotnet watch`** - a stylesheet rewritten while
  MSBuild is evaluating the project takes `dotnet watch` down with it. See the README.
- **CSS/JS bundles:** the four script bundles and the vendor stylesheet bundle the layouts reference are
  built by `ODK.Web.Razor/build/build-bundles.mjs` (esbuild) into `wwwroot/js/odk.bundle*.js` and
  `wwwroot/css/odk.bundle.lib.css`, and are generated and gitignored, like the compiled CSS. `BUNDLES` at the top of
  that script is the whole definition of what each bundle contains. It concatenates then minifies as one
  script, and deliberately does **not** use esbuild's `--bundle`: that would wrap the vendored UMD libraries
  so they assign to `module.exports` instead of `window`, and would take `odk.global.js`'s top-level
  `setImageError` out of global scope, where an inline `onerror=` calls it. There is no watch mode, for the
  same reason there is no Sass watcher - see the CSS note above. Any full build rebuilds the bundles, so if
  `dotnet watch` does not pick a script edit up, `npm run build:bundles` and a hard refresh does.
- **Client-side libraries:** the vendored browser libraries come from npm and are copied into `wwwroot/lib`
  by `ODK.Web.Razor/build/copy-client-libs.mjs`, which the `BuildClientAssets` csproj target runs on every
  build, followed by the SCSS compile and the bundle build (so `dotnet build`/`publish` needs Node on the
  machine). All three outputs - `wwwroot/lib`, `wwwroot/css` and `wwwroot/js/odk.bundle*.js` - are generated
  and gitignored, and none may come from the default Content glob: it is expanded before any target runs, so
  on a clean checkout the files do not exist yet. `DefaultItemExcludes` keeps them out and the target adds
  them back as `Content`, which is what puts them in the publish output.
  Never edit `wwwroot/lib`, and never reference a path the script's `COPIES` list doesn't produce. Adding
  or upgrading a library is a `package.json` change plus a `COPIES` line; `ClientLibraryAssetTests` fails
  when something the app references stops being copied.
- **Batch scripts live in `Scripts/`, and resolve paths from `%~dp0`, never from the current directory.** A
  `cd ..` is relative to the caller, and a failed `cd` does not stop a batch file - it carries on in the wrong
  directory.

Project defaults: `net10.0`, nullable enabled, implicit usings enabled.

## Git workflow

**Don't perform git actions unless explicitly asked.** The user manages branches, commits, pushes, and
PRs. Make code changes on whatever branch is already checked out and leave them uncommitted for the user
to review and commit — don't `git commit`, `git branch`/`switch`, `git push`, merge, rebase, or reset on
your own initiative. When a change is ready, say so and stop; act on git only on an explicit request
("commit this", "create a branch", …).

**Never make changes while `master` is the active branch.** `master` is the mainline that deploys to
prod — all work lands via a branch and PR. Before editing anything, check the current branch; if it's
`master`, stop and ask the user to switch to a feature branch rather than creating one yourself. This
applies to every change, however small (a one-line fix, a doc tweak, a migration).

A `pre-commit` hook enforces the no-commit-on-`master` rule. It lives in the tracked `.githooks/`
directory; enable it once per clone with:

```
git config core.hooksPath .githooks
```

## Database migrations

EF Core migrations live in `ODK.Data.EntityFramework.Migrations` and are run explicitly (never on app
start). Add one after changing entity mappings; see that project's `README.md` for the exact commands
and workflow. **Migration names follow `{TableName}[-{ColumnName}]-{Action}`** (e.g.
`MemberSubscriptionLog-InitiatorId-Add`) — the naming convention and examples are documented in the
migrations README.

**Some enums are mirrored by a lookup table that EF doesn't know about** (`SiteFeatureType` →
`SiteFeatures`, foreign-keyed from `SiteSubscriptionFeatures`). Adding a member to one of those enums
needs a migration inserting the row, or every write of the new value fails the foreign key at runtime
— nothing at build or scaffold time catches it. `Enums/EnumTables.cs` in the migrations project lists
which enums are mirrored, and `MigrationBuilderExtensions` emits the SQL; see the migrations README.

## Data access

Repositories are the **EF boundary**: never leak `IQueryable` out of a repository — it has to be
materialised carefully to work with EF, so composition stays behind the repository. Query results are
returned as deferred queries (`IDeferredQuery*`), and `IUnitOfWork.RunAsync(...)` batches several into
one round-trip (watch for the per-entity N+1 — prefer a batched `...ByChapterIds`-style query over a
loop of single-id calls).

Two styles coexist for composing those queries:

- **Query builders (`XQueryBuilder`) — preferred for new work.** A repo exposes `Query()` returning a
  fluent builder; chain filters (`.InChapter(id)`, `.Active()`, …) then terminate (`.GetAll()`,
  `.GetSingleOrDefault()`, `.Any()`, …). This keeps `IQueryable` encapsulated while avoiding a
  proliferation of near-identical repository methods. Add a filter to the builder rather than a new
  bespoke repository method where a builder already exists.
- **Bespoke repository methods — legacy.** Older repositories expose one method per query. Not all
  repositories have been converted (the builder pattern is verbose to set up), so this style remains;
  it's fine to extend a repo that already uses it, but reach for a query builder for anything new.

**Cluster on the most suitable column(s) where it makes sense.** EF clusters the primary key by default, which
is only the right answer when rows are actually looked up by their key. Plenty here are not: a member entity is
almost always queried *by member*, so ordering the rows that way serves the reads the table exists for. Where a
better key is clear, say so explicitly — `.IsClustered(false)` on the `HasKey` and `.IsClustered()` on the index
that earns it (see `MemberChapterInviteMap`, clustered on `(MemberId, ChapterId)`, and `ChapterEmailMap`,
clustered on `ChapterId`). Lead the composite with the column the reads start from, so one index answers both
the exact-row lookup and the "all of this member's rows" one; a foreign key on another column keeps the index EF
gives it, which covers reads from that direction.

Deliberately a judgement call rather than a rule — what a table is read by is not something a convention can
decide for it. The point is to *make a choice* rather than inherit the default without looking, and to leave a
comment saying what the reads are, since that is the reasoning a later reader cannot recover from the schema.

## Web architecture

The app is **server-rendered** with minimal client-side JavaScript (Bootstrap + small
progressive-enhancement scripts using `data-*` hooks). Prefer an SSR solution — a Razor partial —
over introducing client rendering.

### Razor Pages vs MVC controllers (PRG)

The app follows a clear split — respect it:

- **Razor Pages** (`Pages/**`) render GET requests. Admin pages derive from `AdminPageModel`
  (which derives from `OdkPageModel`).
- **MVC controllers** (`Controllers/**`) handle POSTs that mutate state, then **redirect**
  (Post/Redirect/Get). Controllers derive from `OdkControllerBase` / `AdminControllerBase`.

**Load a page's view model in the `.cshtml`, not the `.cshtml.cs`.** `@inject` the service and call it
in the `@{ }` block, keeping the result in a local. The page model then holds only what a view cannot
get for itself — the route values it captures in `OnGet`, the `Securable`, and any POST handler. See
`Pages/My/Groups/Group/Email.cshtml`.

**Loaded once, in one round-trip.** A view model is fetched a single time on load — by the page's `.cshtml` or by
a shared partial, whichever owns that piece of the screen — and never re-fetched further down the render. One
`Get...ViewModel(request)` call per view model, held in a local. The service behind it batches everything it needs
through `IUnitOfWork.RunAsync(...)` so the page costs one database round-trip rather than one per value; a partial
that quietly loads its own extras turns a single round-trip into several and hides them from the page that pays
for them. When a partial genuinely owns its data, give it its own view model and its own single load — do not
have the parent fetch on its behalf and pass fragments down.

The same applies to a `GET` that arrives with something to look up, such as a token in the query string: resolve
it once into the view model the page renders, rather than reading it in `OnGet` and again in the view.

Loading in the page model means a property per value passed through, and those properties collide with
the base's own — `Title` is already the browser title, so an email's title had to become `AppliedTitle`
purely to get past the compiler. A local in the view has no such problem. Where a POST handler renders
the page again on failure, set the captured route values there too, or the re-render loads against a
`default` route value.

A page's form typically `action`s a controller endpoint; the controller does the work, calls
`AddFeedback(...)`, and returns `RedirectToReferrer()` or `Redirect(OdkRoutes...Path)`.

Exception: a **preview / multi-step** flow legitimately renders from a POST (it can't redirect and
keep the posted data). See the member-import flow (`ImportMembers` pages + `_ImportMembersContent`)
for the pattern — page handler renders the preview; a confirm form posts to a controller to commit.

**When a POST does stay on the page, use the default `OnPostAsync` handler and a plain
`<form method="post">`** (no `action`, no `asp-page-handler`). Pages use fully custom absolute
`@page` routes and render their forms through shared partials nested in a layout shell
(`_AdminBody` / `_TwoColLeftMenu`), and in that setup the Form Tag Helper does not reliably emit a
`?handler=…` self-link — the POST silently falls back to the default handler. So keep one POST
action per page and let it be the default handler; if a page genuinely needs several, post those
forms to controller endpoints instead of naming page handlers.

### Two-platform admin pages

The same admin function is exposed on **both** platforms under different route trees and page
chrome. The functionality is written once, in a shared partial; each platform gets a thin page that
supplies only its own infrastructure.

| | Drunken Knitwits | Group Squirrel |
|---|---|---|
| Page tree | `Pages/Chapters/Admin/**` | `Pages/My/Groups/**` |
| Route | `/{chapterName}/admin/...` | `/my/groups/{chapterId:guid}/...` |
| Layout | `_OdkChapterAdminLayout` | `_GroupAdminLayout` |
| Page shell partial | `Admin/_AdminBody` (`AdminBodyViewModel`) | `Components/_TwoColLeftMenu` (`TwoColLeftMenuViewModel`) |
| Page model base | `AdminPageModel` | `OdkGroupAdminPageModel` |

**Rule — a new admin function = 2 pages + 1 shared partial:**

1. `Views/Shared/**/_XxxContent.cshtml` — **all** the functionality (forms, tables, preview, etc.),
   driven by a view model built by a service.
2. `Pages/Chapters/Admin/**/Xxx.cshtml` (+ `.cshtml.cs : AdminPageModel`) — Drunken Knitwits shell.
3. `Pages/My/Groups/**/Xxx.cshtml` (+ `.cshtml.cs : OdkGroupAdminPageModel`) — Group Squirrel shell.

Each page only sets its route, layout, shell partial, breadcrumbs, and securable, then renders the
shared partial. Never put feature markup directly in a page. Handlers that must live on a page (an
`OnPost`, etc.) stay thin: delegate the real work to a shared helper so both platforms behave
identically. Add a matching `GroupAdminRoutes` helper — it resolves the platform-correct URL via
`Base(chapter)`, so callers and links are platform-agnostic.

### View models

- **Service-layer view models** live in `ODK.Services/**/ViewModels` and are built by services
  (`Get...ViewModel(request)`). Pages inject the service and call these.
- **Web-layer form view models** live in `ODK.Web.Razor/Models/**` and bind incoming form posts
  (`[FromForm]`). Name them `...FormViewModel` / `...SubmitViewModel`.
- **Split the POST surface from the GET surface: `...FormSubmitViewModel` holds exactly the properties the
  form posts, and `...FormViewModel` inherits it and adds what only the render needs** (a `ReadOnly` flag, a
  "can this member edit" bool, options for a dropdown). Controllers bind the *submit* model, so a
  render-only property is unreachable from a post rather than merely undeclared — which lets it be
  `required` instead of an optional field with a comment explaining why it can be ignored. The form partial
  keeps `@model ...FormViewModel`, and because that derives from the submit model the field names are
  unchanged, so the POST still binds. See `ThemeFormSubmitViewModel` / `ThemeFormViewModel`.
- Partials live in `Views/Shared/**` (e.g. `Admin/Members/_MembersAdminContent`) and are rendered with
  `Html.PartialAsync`. Reusable page chrome goes through `Admin/_AdminBody` and `Admin/_AdminLink`.
- **Where a component view model offers a plain property and a `...ContentFunc` template for the same slot,
  pass the plain property.** `PanelViewModel` has `BodyContent` *and* `BodyContentFunc`. The template is for
  a slot that holds more than the value can express — a badge beside the title, a link, a count — never for
  wrapping a string in a tag the plain property already emits.
  It is not only verbosity: **Razor allows exactly one level of nested inline markup block**
  (`@<div>…</div>`), so a template spent on a title is a level unavailable to the content that needs one,
  and the compiler reports it as `RZ2003` at the inner block rather than at the title that took the level.
- **A panel titles itself through `Heading`, and that is the only way it can.** `PanelViewModel` offers no
  plain `Title` string and no title template: `Heading = new HeadingViewModel { Title = "Bulk email",
  Type = HeadingType.H5 }` is the whole surface, so every panel title carries the level its page needs and
  none of them hand-roll a heading tag. Anything that sits *beside* the title — a link, a badge, a count —
  goes in `TitleEndContentFunc`, which is why no template belongs on the title itself.
- **A title rendered at a caller-chosen level goes through `Components/_Heading`** (`HeadingViewModel`:
  `Title`, an optional `Type` from `HeadingType`, an optional `Class`). A component that holds a title takes
  a `HeadingViewModel` rather than picking a level itself, since the right level depends on what the page
  around it already uses; `Type` left unset renders a div, so a caller with no view on the level gets text
  rather than a heading the document outline has to account for.
- **A partial's file name is unique across the whole project** — the directory disambiguates for the
  framework, but nothing else works that way: editor tabs, "go to file", and search results show the file
  name alone, so four `_EmailForm.cshtml` are four indistinguishable results. Qualify the name with the
  context its directory denotes, in the codebase's own vocabulary: `Admin/Chapter/_ChapterAdminEmailForm`
  and `SiteAdmin/_SiteAdminEmailForm`, not two `_EmailForm`. The prefixes in use are `ChapterAdmin`,
  `SiteAdmin`, `EventAdmin`, `MemberAdmin`, `Account`, `Chapter` and `Site`.
  Three refinements, because a mechanical prefix reads badly often enough to be worth stating:
  **don't repeat a word the name already has** (`_EventAdminContent`, not `_EventAdminEventContent`);
  **leave the plain member-facing partial unqualified** where qualifying its admin counterpart already makes
  both unique (`Events/_EventContent` stays, `Admin/Events/Event/_EventAdminContent` moves) — the public one
  is the unmarked case, and prefixing it would only restate its folder; and **suffix instead of prefix when
  the difference is asset type rather than context** (`_ImagingScripts` / `_ImagingStyles`).
- **Partial paths are strings, so the compiler never checks them.** A rename builds clean and fails at
  request time. After moving or renaming a partial, confirm every `Html.PartialAsync("…")` argument still
  resolves to a file — and mind the casing while you are there: Windows resolves
  `Components/_panel` to `_Panel.cshtml`, and a case-sensitive filesystem would not.
- **`data-odk-component` carries the partial's file name, exactly** — primarily a dev-tools aid for finding
  which partial emitted an element, but E2E tests also select on it
  (`[data-odk-component='_ChapterSidebar']`), so treat a change to a value as a change to a selector rather
  than to a comment. Keep it in step with a rename, and keep it the bare name: a path-qualified value only
  ever meant the file name was ambiguous, which the rule above stops it being.
- **Anything in the markup whose only reader is an E2E test says so, at the use site.** The E2E suite is a
  separate solution that deliberately does not reference this one, so from here a "find usages" turns up
  nothing: an attribute or class that no CSS and no script touches looks like dead markup, and the next
  person to tidy up is right to remove it on the evidence available. A one-line comment naming the test
  dependency is the only thing that makes the coupling visible from this side. This applies to a class with
  no styling behind it as much as to a bare `data-*` attribute. `data-odk-component` is the exception that
  needs no note per use - the convention above covers every one of them at once.

### Routing

URLs are built through **`IOdkRoutes`** (inject it), not hard-coded strings. Group-admin routes
live in `ODK.Web.Common/Routes/GroupAdminRoutes.cs`; add a helper there rather than composing paths
inline. Route helpers carry the `ChapterAdminSecurable` they require, and `_AdminLink` can
auto-hide/disable based on it (`UnauthorizedBehaviour`). Site-admin routes live in
`SiteAdminRoutes.cs` and return `SiteAdminRoute` — site-admin access is binary, so unlike
`GroupAdminRoute` there is no securable to carry.

**Admin routes are both strongly typed and enumerable.** The typed accessors (`Events(chapter)`,
`Venue(chapter, id)`, …) stay the way you build a *specific* URL. On top of them,
`GroupAdminRoutes.Navigation(chapter)` defines the admin menu tree — the single registry of what the
admin area contains — and everything that needs to *iterate* admin routes derives from it:

- `PermittedNavigation(chapter, adminMember, currentMember)` filters that tree by securable and
  platform. `_AdminSideMenu` renders it; a section with no surviving items drops out.
- `LandingRoute(...)` picks where to send a member with no specific destination — the admin landing
  page, and `AdminPageModel`'s fallback when bouncing a member off a page. It prefers the events page:
  both platforms are events platforms, so any role with elevated group privileges can reach it by
  definition. Otherwise it takes the first permitted page in menu order. **Never fall back to a fixed route** — a member who lacks access
  to that route is redirected to it, bounced again, and loops. It returns null when no admin page is
  permitted; that's a 403, not a redirect.

So **adding an admin page means registering it in `Navigation`**, not just adding an accessor —
otherwise it exists but is unreachable from the menu and invisible to the redirect logic.
`SiteAdminRoutes.Navigation()` does the same for the site-admin menu (no filtering needed).

## Services conventions

- **`ServiceResult` / `ServiceResult<T>`** is the standard return for operations that can fail.
  Use `.Successful(...)` / `.Failure(msg)`; the web layer surfaces it via `AddFeedback(result)`.
- **When the payload is a primitive, derive a named result type instead of using `ServiceResult<T>`.**
  `ServiceResult<Member?>` is fine — the type says what the value is. `ServiceResult<string?>` does not: `Value`
  could be a token, a URL or a reference, so the meaning lives in a comment on the method and in the reader's
  memory. A derived type names it at the use site — `result.ActivationToken` needs no comment. Name it after the
  method (`CreateChapterAccount` → `CreateChapterAccountResult`), keep the constructor `private` behind static
  factories named for the outcome they describe, and re-declare `Failure` / `Successful` with `new` so they
  return the derived type. Give each factory the arguments that outcome actually has — a success carrying a token
  and a success carrying a message must not be two calls with the same signature, or the wrong one is a silent
  mistake rather than a compiler error. `FromResult(result)` carries a failure raised elsewhere through unchanged.
  See `CreateChapterAccountResult`.
- **Service requests** (`IServiceRequest`, `IMemberChapterServiceRequest`,
  `IMemberChapterAdminServiceRequest`, …) carry the caller context (platform, chapter, member).
  Controllers/pages get them from the request store; don't reconstruct them ad hoc.
- **Authorization** for admin actions goes through the securable on the request:
  `MemberChapterAdminServiceRequest.Create(ChapterAdminSecurable.X, MemberChapterServiceRequest)`
  in controllers, and `AdminPageModel.Securable` on pages. Use the **specific** securable for the
  action (e.g. `MemberImport`, not a nearby one). Admin services enforce it via
  `GetChapterAdminRestrictedContent(...)` in `OdkAdminServiceBase`.
- Keep business logic in `ODK.Services`, not in controllers or pages.
- **Centralise shared guards in the service, not the callers.** An access/visibility check a view-model
  service can enforce (e.g. "only members may view this", "hidden pages 404") belongs inside the
  `Get...ViewModel` method — `throw` the appropriate `Odk*Exception` (or return a failure) there — rather
  than each page/controller repeating an `if (!vm.X) throw`. A guard the service owns can't be forgotten
  by a new caller, and the two-platform pages stay thin (the same service backs both). This applies
  generally: when the same rule is duplicated across the Drunken Knitwits and Group Squirrel copies of a
  page, push it down into the shared service. See `MemberViewModelService.GetMemberPage` — it asserts the
  viewed member **and** the viewer belong to the chapter and that the Members page isn't hidden, so both
  member pages just call it and render.
- **Atomicity.** A single `IUnitOfWork.SaveChangesAsync()` commits all pending changes across every
  repository in one implicit EF transaction (they share one `DbContext`), so a multi-repository write
  is already atomic — batch the writes and save once. There is deliberately no explicit transaction
  API. Where a method commits, performs an external side effect (send an email, schedule/enqueue a
  Hangfire job, call a payment provider), then commits again, **that split is intentional and must be
  preserved**: the first commit persists state *before* the irreversible external action, so the
  action is never taken against state that later rolls back. Do not wrap a
  commit → external call → commit sequence in a transaction. Rely on job/webhook idempotency (see
  `InitiatorId` in `PaymentService`) for the window between the external action and the final commit.

## Localisation (date/number formatting)

Date and number **formatting** follows the request's locale; the sitewide default is
`Localisation:DefaultLocale` (config, currently `en-GB`). This is about *formatting* only — the UI copy is
authored in the default language, so `CurrentUICulture` (resource lookups) is always the default; only
`CurrentCulture` (formatting) varies.

**How the request locale is determined.** `LocaleUtils.GetPreferredLocale` parses the `Accept-Language`
header (quality-ordered) and returns the first entry that is a valid **specific** culture (`CultureInfo`),
canonicalised; neutral cultures (e.g. `en`) are skipped so a region-less hint falls through to the default,
and an unusable header yields the default. That single parse feeds **two** consumers:

- **`CultureInfo.CurrentCulture`** — set to the request locale **for rendering only**, by
  `RequestCultureResultFilter` (a global result filter, runs after model binding, before the view renders).
  This drives *all* .NET formatting during rendering: direct `@date` renders, standard specifiers (`"d"`,
  `"D"`, `"t"`), and `DateUtils.ToFriendlyDateString`. **Model binding is deliberately *not* affected** —
  the app-wide default culture is pinned in `Program.cs` (`CultureInfo.DefaultThreadCurrentCulture`), so
  posted values (flatpickr dates are posted as a fixed `dd/MM/yyyy`, decimals, etc.) always *parse* under
  the default culture regardless of the request locale. Formatting follows the locale; parsing never does.
- **`HttpRequestContext.Locale`** → `LocaleService.GetShortDatePattern(request)` — supplies the
  short-date pattern string the flatpickr date-picker uses for its display (`_Layout`).

Both derive from the same `GetPreferredLocale` call, so they never disagree.

**Formatting rules for new work:**

- **Human-facing dates** — use `DateUtils.ToFriendlyDateString` / `ToFriendlyDateTimeString` (or standard
  specifiers). They follow `CurrentCulture`, so day/month order and month names localise automatically —
  never hardcode an order like `"d MMMM yyyy"` for display. The friendly helper takes a UTC value and a
  `TimeZone` and converts internally.
- **Times stay 24-hour** — the app's house style (see `TimeSpanUtils`, event headers). The friendly helper
  localises the *date* order/names but keeps `HH:mm`; don't introduce AM/PM.
- **Request-independent text** (emails, notifications, CSV/exports — anything created off a background job
  or persisted for another reader) — pass an explicit culture so it never inherits the ambient request
  culture. For **member-facing** output (emails, notifications) use the *recipient's* stored locale via
  `IMemberLocaleService.GetCulture(memberId)` (or `GetCultures(memberIds)` to batch), passed as
  `FriendlyDateStringOptions.Culture`; it falls back to `LocaleUtils.DefaultCulture` when the member has no
  stored locale. A **multi-recipient** email whose body carries a formatted date must group recipients by
  culture and send one copy per group (see `MemberEmailService.SendEventInvites`) — the email pipeline
  builds one body per send. For **machine/interchange** values (`<input type="date">` values, sort keys, ISO
  timestamps, CSV) use `CultureInfo.InvariantCulture`.

**Member locale.** `MemberPreferences.Locale` stores a member's formatting locale, captured from the request
locale at account creation and refreshed on every request: `RequestStore` compares the stored locale with
the request locale and enqueues a background `IMemberLocaleService.UpdateLocale` job when they differ. Null
(not yet captured) falls back to the default — the gap shrinks as members make requests.

(Timezone conversion is a separate concern — see the timezone-aware bullet under *Conventions & style*.)

## Tests

- NUnit + FluentAssertions + Moq; EF Core InMemory for data.
- Decorate test classes with `[Parallelizable]`.
- Arrange / Act / Assert with `// Arrange` etc. comments.
- Test method names: `Method_Scenario_ExpectedResult`.
- Assert with FluentAssertions (`result.Should().Be(...)`).

## Conventions & style

- Match the style of surrounding code (tabs vs spaces varies by project — follow the file).
- **Comments describe the code as it is now, never how it got that way.** No "this used to…", "changed
  from…", "previously…", or an account of the bug that prompted the current shape. Someone reading the
  file for the first time is trying to understand what it does; the history is in git and does not earn
  space here. Say what the code does and why it is that way in the present tense.
  The exception is documenting a road *not* taken, where the comment stops someone making a change that
  looks like an improvement — "not X, because Y" is about the current design and is worth keeping. Write
  it as a standing rule rather than as a story ("do not merge these lists: a migration that runs before a
  column exists must insert without it"), so it reads as a constraint rather than a changelog.
  That exception is narrower than it looks, and two tests keep it honest. The comment must be **anchored to code
  that exists** — never explaining why something is absent, because a comment on a hole has nothing that prompts
  a reader to revisit it when the hole gets filled, and it then reads as an argument against the very change
  being made. And the *because* must be a **constraint that cannot change**, not a design preference in the
  grammar of one: "a migration that runs before a column exists must insert without it" stays true, whereas "this
  carries no token, because accepting requires being signed in" only describes a choice, and became wrong the day
  a token was wanted. If you are explaining an absence, or a reason a new requirement could overturn, say nothing
  and let the code speak.
  Nor should a comment narrate what a call plainly does — the code is definitive.
- File-scoped namespaces.
- **Prefer `using` directives over fully-qualified type names.** Import the namespace and use the short
  type name rather than inlining a namespace path at the use site. Applies to Razor too — add a
  `@using` and shorten `@inject`/type references (e.g. `@using Microsoft.AspNetCore.Antiforgery` +
  `@inject IAntiforgery Antiforgery`, not `@inject Microsoft.AspNetCore.Antiforgery.IAntiforgery …`).
- **Remove unused `using`s in any file you edit.** When you finish working in a file, drop imports your
  changes left unused — don't leave orphaned usings behind.
- **One top-level declaration per file**, named after the file — don't put an interface and its
  implementation (or several types) in the same file. Give each its own file (e.g.
  `IMemberImportStagingService.cs` and `MemberImportStagingService.cs`).
- **Member ordering within a type**, along three axes (each breaks ties in the one before):
  1. **By kind:** constants, then fields, then constructors, then properties, then methods.
  2. **By access then lifetime, within a kind.** `static` sorts before instance, but *only inside one access
     level* — it never promotes a member past a more accessible one. In full precedence order:
     1. `public static`
     2. `public` instance
     3. `internal static`
     4. `internal` instance
     5. `protected static`
     6. `protected` instance
     7. `private static`
     8. `private` instance
  3. **Alphabetically by name, within the resulting group.**
  Applies everywhere, including test classes (e.g. private helper methods sit below the public `[Test]` methods).
  (StyleCop enforces axes 1–2 but not the alphabetical axis, so keep axis 3 in mind manually.)
  The trap worth naming: a `private static` helper added next to the instance methods that call it looks
  tidy and is wrong — it belongs above every `private` instance method, however far that is from its caller.
- **No trailing whitespace.**
- **Preserve line endings and encoding: CRLF, and a UTF-8 BOM on `.cs`/`.cshtml`.** The repo is checked out
  with `core.autocrlf=true`, so working files are CRLF; `.gitattributes` pins the few deliberate exceptions
  (generated `wwwroot/css/*.css` and `.githooks/**` are LF). Normal editing preserves both, but **a script
  that rewrites whole files can silently convert them** — e.g. Python's `open(path)` applies universal-newline
  translation on read, so a naive read/modify/write turns a CRLF file into LF (read and write with
  `newline=''`, or fix up afterwards). Git normalises on compare, so `git diff` still looks small and hides
  it, while GUI diff tools show every line as changed. After any bulk/scripted edit, check with
  `git diff --numstat` — a "LF will be replaced by CRLF" warning means a file needs converting back.
- **Leave auto-generated files alone** — never hand-edit `*.Designer.cs`, `*.generated.cs`, EF migration
  scaffolding, or anything else with an `<auto-generated>` header. Conventions (ordering, whitespace,
  etc.) don't apply to them; the `.editorconfig` marks them `generated_code = true` so analyzers skip them.
- Prefer `required` init properties for model/view-model types.
- **Settings classes: every property `required`, and never a hard-coded default.** A settings type binds
  configuration, so config must state every value — `public required string Mode { get; init; }`, not
  `public string Mode { get; init; } = "power";`. A default in the class is a value that doesn't appear in
  `appsettings.json`, so nobody reviewing the config can see what the app will actually use, and the two
  quietly disagree. A property that is legitimately empty in some environments (an API key absent locally)
  is still `required` — it's set to `""` in config, which is a statement rather than an omission.
  **`required` states intent and enforces nothing here.** The configuration binder constructs settings
  reflectively, so a key missing from `appsettings.json` arrives as `null` (or `0`, or `false`) whatever the
  declaration says — `required` only binds C# code using an object initialiser. So the code reading a settings
  value must still cope with absence; `DependencyRegistrar` coalescing `x.Paths ?? []` is not redundant.
- **A value kept out of git still has its structure committed to `appsettings.json`, emptied.** `""` for a
  string, `[]` for an array, and never an omitted section — the real value goes in the git-ignored
  `appsettings.Development.json` and in Doppler (`Payments:Stripe:Platforms:*:WebhookSecretV1`,
  `Recaptcha:SecretKey`, `Platforms:*:Urls`). The tracked file then doubles as the template for a new
  environment while giving nothing away, and `AppSettingsTests` keeps working: it binds the tracked file and
  skips nullable properties, so an omitted section is a nullable property it walks straight past, taking every
  non-nullable value inside it out of coverage too.
  Empty **only** what is actually sensitive — a value already public elsewhere in the same file stays stated.
  And read an empty value as *unstated*, never as "expected to be empty": a check with no expectation is
  neither met nor unmet, and code that conflates the two reports a failure it has no grounds for. See
  `StripeWebhookParser`, which reads a blank webhook secret as unconfigured.
- **Declare a setting nullable when config genuinely cannot state it, and coalesce at the mapping.** Two cases,
  both of which the binder resolves to `null` rather than to something empty: a **dictionary**, because `{}`
  produces no config keys at all (`Instagram:Client:Cookies`); and any property an **array element** leaves out,
  because each entry states only the keys it uses (`Logging:IgnoreExceptions`). An empty **array** is not one of
  them — `[]` binds to an empty array, so a `string[]` that config declares as `[]` is safely non-null. Marking
  the unstatable ones `Dictionary<..>?` / `string[]?` makes the `?? []` in `DependencyRegistrar` compiler-enforced
  instead of remembered, and keeps the annotation honest about what the binder can deliver.
  `AppSettingsTests` walks the bound graph and fails on any *non-nullable* setting that came back null, so this
  stays enforced rather than reviewed.
- **Everything bound to `appsettings.json` lives in `ODK.Infrastructure/Settings`**, named for its config path
  plus `Settings` (`Payments:Stripe` → `PaymentsStripeSettings`, one entry of `Logging:IgnoreExceptions` →
  `LoggingIgnoreExceptionSettings`). That includes the element types of arrays and nested sections, not just the
  top-level ones. Where a service needs the same values it declares **its own** type (`LoggingServiceSettings`,
  `EmailServiceSettings`) and `DependencyRegistrar` maps one to the other. The duplication is the point: the
  config shape is a contract with deployed `appsettings.json` files and with Doppler, so a service must be able
  to change what it consumes without that being a breaking config change — and dependencies only ever point from
  `ODK.Infrastructure` into `ODK.Services`, never back. A config-bound type declared in a service inverts that.
- **Every enum reserves `None` for 0.** Without it the zero value is a real case, so anything that arrives
  unset — a `default(T)`, a field never assigned, a value that failed to bind — silently means whichever
  member happens to sit first, and the bug looks like correct behaviour. Reserving `None` makes an unset
  value distinguishable; branch on the member you want (`if (level != Full)`) rather than against the one
  you don't, so `None` never falls into the expensive or destructive path.
- **Assign explicit values only where the number itself is the contract** — persisted to the database,
  sent over an API, queued as a background job argument, or read from config — because there renumbering
  silently reinterprets existing data (`PaymentProviderType`, `EmailType`). Hangfire counts: it serialises an
  enum argument as its number, so a job queued by one version runs as a different member under the next
  (`AccountState`, `AccountTrigger`). An enum that never leaves the process keeps implicit values
  (`ChapterAdminSecurable`, `MemberImportRowStatus`): numbering it adds a promise nothing needs and
  invites the next member to be appended out of order to preserve it.
- **Be timezone-aware in new work.** Timestamps are stored/compared in UTC; a user's calendar
  input is in their local zone (the chapter's `TimeZone`, falling back to `Chapter.DefaultTimeZone`).
  Convert at the boundary in the **service** (e.g. `DateUtils.ToUtc`), not in repositories. For a
  local **date-range** filter, resolve the endpoints to UTC instants — start of the *From* day and
  start of the day *after* the *To* day — and query a half-open UTC range (`>= fromUtc && < toUtcExclusive`)
  so an event matches when its *local* date is on/after From and on/before To. Don't apply a fixed
  offset per row (DST makes it wrong); convert the two boundaries instead. See
  `EventAdminService.GetEventsAdminPageViewModel`.
- **Display timezone: prefer the viewing member's, fall back to the chapter's.** A user-facing datetime
  is rendered in the current member's `TimeZone` where one is known, otherwise the chapter's. In views,
  use `Html.DisplayTimeZone(Model.Chapter.TimeZone)` (resolves the current member via `IRequestStore`, no
  need for the VM to carry it) for **point-in-time** values (created/sent/joined/expires/paid/…). **Event
  start/end times are the exception** — they stay in the chapter (venue) timezone so the wall-clock time
  isn't misread. When the viewer's zone differs, append `Html.ChapterTimeZoneLabel(Model.Chapter.TimeZone,
  utc)` — a DST-aware `"(UTC+1)"` indicator (empty when the viewer is in the chapter's zone or unknown);
  see `_EventHeader` / `_ListEventBody`. A VM that already exposes a member-preferred `TimeZone` (the
  `GroupPageViewModel` base does: `CurrentMember?.TimeZone ?? Chapter.TimeZone`) can be used directly.
- When materialising a collection to satisfy an `IReadOnlyCollection<T>` (return type, property, or
  parameter), use `.ToArray()` rather than `.ToList()`.
- Don't add client-side frameworks; reach for a partial first.
- In Razor views, render form fields with the strongly-typed HTML helpers (`Html.HiddenFor`,
  `Html.TextBoxFor`, `Html.LabelFor`, …) bound to view-model properties — not hand-written
  `<input name="…">`. This keeps fields tied to the view model, so property references are findable and
  refactors stay safe.
  **Exception — display-only formatted values:** a helper binds the raw property value, so it can't show
  a *formatted* value (e.g. a currency string via `Currency.ToAmountString`). When you need to display a
  formatted, read-only value, a raw `<input readonly value="@…">` (or plain markup) is correct. If that
  value must also post back, bind the real value with `Html.HiddenFor` alongside the read-only display —
  see `_SubscriptionForm.cshtml` (hidden `Amount` + a read-only input showing the formatted amount).
- Reuse shared helpers rather than duplicating (e.g. CSV parsing lives in
  `ODK.Web.Razor/Services/CsvFileReader.cs`, used by both controllers and page models).

## Domain notes

- **Hidden members (`MemberChapter.HideProfile`).** This exists solely so a site admin can join a group
  — usually to test it — without appearing to the group's real members. It is *not* a general
  privacy/visibility feature. Member-listing queries filter these out by default (e.g. `InChapter`'s
  `!HideProfile` predicate); include them only where a site-admin view genuinely needs to.

- **Free site subscriptions.** `SiteSubscription.Free` states that a plan costs nothing, so a plan is usable
  without any price. **A usable plan is `Enabled && (Free || priced)`** - never read `Enabled` alone, and
  never infer "free" from a zero amount. Go through `SiteSubscription.IsActive(prices, sitePaymentSettings)`
  for a loaded subscription and `SiteSubscriptionQueryBuilder.Active()` for a query; the two mirror each
  other and have to agree. A free plan does not require its payment settings to be enabled - it takes no
  money - whereas a priced one does, since nothing can be bought while the provider is off.
  A member put on a free plan gets **no expiry**: `MemberSiteSubscriptionRecord.ExpiresUtc` stays null,
  which every expiry check already reads as never-expiring. Free and a *paid* price are mutually exclusive
  (`SiteSubscriptionAdminService` refuses both directions); a legacy zero-amount price alongside the flag is
  tolerated, because that is how a free plan was expressed before the flag existed.
- **Site subscription cooldown.** An expired site subscription keeps its access for a configured cooldown
  (`Subscriptions:DefaultCooldownMonths`, mapped to the injected `SiteSubscriptionCooldown`). The stored
  `ExpiresUtc` is never moved - the cooldown is applied wherever expiry is *read*, so never compare a site
  subscription's `ExpiresUtc` to `DateTime.UtcNow` directly. Go through
  `MemberSiteSubscriptionRecordQueryBuilder.Active(cooldown)` for a query and
  `MemberSiteSubscriptionState.IsActive`/`IsExpired` for a value already loaded.
  Chapter *membership* has a separate cooldown of its own - per-chapter
  `ChapterMembershipSettings.MembershipDisabledAfterDaysExpired`, applied in
  `AuthorizationService.GetSubscriptionStatus` and `PaymentService.RollExpiryForward`.
