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
ODK.Data.Core               Repository/data abstractions.
ODK.Data.EntityFramework    EF Core implementations.
ODK.Resources               Embedded resources (email templates etc.).
ODK.Services                Business logic. The heart of the app.
ODK.Services.Integrations   Third-party integrations.
ODK.Infrastructure          Composition / cross-cutting wiring.
ODK.Web.Common              Web helpers shared across web projects (e.g. route builders).
ODK.Web.Razor               The web app: Razor Pages, MVC controllers, views, wwwroot.
```

Tests: `ODK.Core.Tests`, `ODK.Services.Tests`, `ODK.Services.Integrations.Tests`.

## Build / test / run

- **Build:** `dotnet build ODK.Web.Razor/ODK.Web.Razor.csproj` (builds the referenced graph).
- **Test:** `dotnet test ODK.Services.Tests/ODK.Services.Tests.csproj` (or the solution).
- **Run locally:** `run-odk.bat` (Drunken Knitwits) or `run-gs.bat` (Group Squirrel). These spawn
  the `dotnet` process and a Sass watcher. Requires a local SQL Server restored from a prod backup.
- **CSS:** `.scss` in `wwwroot/scss` compiles to `wwwroot/css` via `npm run build:css` (also run by
  the batch files). Don't hand-edit compiled `.css`.

Project defaults: `net10.0`, nullable enabled, implicit usings enabled.

## Database migrations

EF Core migrations live in `ODK.Data.EntityFramework.Migrations` and are run explicitly (never on app
start). Add one after changing entity mappings; see that project's `README.md` for the exact commands
and workflow. **Migration names follow `{TableName}[-{ColumnName}]-{Action}`** (e.g.
`MemberSubscriptionLog-InitiatorId-Add`) — the naming convention and examples are documented in the
migrations README.

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
- Partials live in `Views/Shared/**` (e.g. `Admin/Members/_MembersContent`) and are rendered with
  `Html.PartialAsync`. Reusable page chrome goes through `Admin/_AdminBody` and `Admin/_AdminLink`.

### Routing

URLs are built through **`IOdkRoutes`** (inject it), not hard-coded strings. Group-admin routes
live in `ODK.Web.Common/Routes/GroupAdminRoutes.cs`; add a helper there rather than composing paths
inline. Route helpers carry the `ChapterAdminSecurable` they require, and `_AdminLink` can
auto-hide/disable based on it (`UnauthorizedBehaviour`).

## Services conventions

- **`ServiceResult` / `ServiceResult<T>`** is the standard return for operations that can fail.
  Use `.Successful(...)` / `.Failure(msg)`; the web layer surfaces it via `AddFeedback(result)`.
- **Service requests** (`IServiceRequest`, `IMemberChapterServiceRequest`,
  `IMemberChapterAdminServiceRequest`, …) carry the caller context (platform, chapter, member).
  Controllers/pages get them from the request store; don't reconstruct them ad hoc.
- **Authorization** for admin actions goes through the securable on the request:
  `MemberChapterAdminServiceRequest.Create(ChapterAdminSecurable.X, MemberChapterServiceRequest)`
  in controllers, and `AdminPageModel.Securable` on pages. Use the **specific** securable for the
  action (e.g. `MemberImport`, not a nearby one). Admin services enforce it via
  `GetChapterAdminRestrictedContent(...)` in `OdkAdminServiceBase`.
- Keep business logic in `ODK.Services`, not in controllers or pages.
- **Atomicity.** A single `IUnitOfWork.SaveChangesAsync()` commits all pending changes across every
  repository in one implicit EF transaction (they share one `DbContext`), so a multi-repository write
  is already atomic — batch the writes and save once. There is deliberately no explicit transaction
  API. Where a method commits, performs an external side effect (send an email, schedule/enqueue a
  Hangfire job, call a payment provider), then commits again, **that split is intentional and must be
  preserved**: the first commit persists state *before* the irreversible external action, so the
  action is never taken against state that later rolls back. Do not wrap a
  commit → external call → commit sequence in a transaction. Rely on job/webhook idempotency (see
  `InitiatorId` in `PaymentService`) for the window between the external action and the final commit.

## Tests

- NUnit + FluentAssertions + Moq; EF Core InMemory for data.
- Decorate test classes with `[Parallelizable]`.
- Arrange / Act / Assert with `// Arrange` etc. comments.
- Test method names: `Method_Scenario_ExpectedResult`.
- Assert with FluentAssertions (`result.Should().Be(...)`).

## Conventions & style

- Match the style of surrounding code (tabs vs spaces varies by project — follow the file).
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
  2. **By access, within a kind:** `public`, then `protected`, then `private`; and `static` before instance
     within each access level.
  3. **Alphabetically by name, within the resulting group.**
  Applies everywhere, including test classes (e.g. private helper methods sit below the public `[Test]` methods).
  (StyleCop enforces axes 1–2 but not the alphabetical axis, so keep axis 3 in mind manually.)
- **No trailing whitespace.**
- **Leave auto-generated files alone** — never hand-edit `*.Designer.cs`, `*.generated.cs`, EF migration
  scaffolding, or anything else with an `<auto-generated>` header. Conventions (ordering, whitespace,
  etc.) don't apply to them; the `.editorconfig` marks them `generated_code = true` so analyzers skip them.
- Prefer `required` init properties for model/view-model types.
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
