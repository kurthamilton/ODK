# Getting started

- [Installation](#installation)
- [Apps](#apps)
- [Running locally](#running-locally)
- [ngrok](#ngrok)
- [CSS](#css)
- [Client-side libraries](#client-side-libraries)
- [Deployment](#deployment)
- [Subscriptions](#subscriptions)
- [Workflows](#workflows)
- [Database keys](#database-keys)
- [Antiforgery (CSRF)](#antiforgery-csrf)

## Installation
1. Install the latest version of .NET
2. Install the latest LTS version of [Node](https://nodejs.org) — the SCSS build and the client-side
   libraries both come from npm
3. Install the latest version of SQL Server
4. Take a backup of the prod DB and restore locally
5. Install [ngrok](https://ngrok.com/download) and create `ngrok.yml` in the repo root (see [ngrok](#ngrok))

## Apps
The project runs two different platforms based on the base URL.

### ODK
The Drunken Knitwits platform, specifically for Drunken Knitwits groups around the world.

### Group Squirrel
A Meetup-style platform currently under development.

## Running locally
Run `Scripts/run.app.bat`. One process serves **both** platforms — Group Squirrel on
[localhost:8123](http://localhost:8123) and ODK on [localhost:8124](http://localhost:8124) — with the
platform resolved from the request URL (the `Platforms` config in `appsettings.Development.json`).

It compiles the SCSS once, then runs `dotnet watch` in that window. `dotnet watch` owns the console's stdin,
so its shortcuts work — notably **Ctrl+R** to force a restart when a change isn't picked up or the app doesn't
recover from an error.

`.cs` and `.cshtml` changes hot-reload as usual. Styles do not — see below.

### Rebuilding the CSS after an SCSS change

Nothing watches `wwwroot/scss` while the app is running, so a change there has no effect until you compile
it. In a **second** terminal (leave `dotnet watch` running in the first):

```
Scripts/run.build.css.bat
```

or, equivalently, `npm run build:css` from `ODK.Web.Razor`.

Then refresh the browser. The `<link>` carries a content hash, so an ordinary refresh normally picks the new
file up; hard-refresh (Ctrl+F5) if it doesn't, since the caching headers for static assets are computed when
the project builds and a file changed since then can still be served as unmodified.

`build:css` writes all four outputs: `main.css` / `odk.css` and their `.min` counterparts. The app links only
the minified pair; the expanded pair is committed alongside so the compiled output is readable in a diff. Run
the whole script rather than `sass:min` alone, or the expanded files go stale.

**Do not run a sass watcher alongside `dotnet watch`.** MSBuild enumerates and hashes every file under
`wwwroot` as a static web asset on each rebuild, so a watcher rewriting `wwwroot/css` while `dotnet watch` is
evaluating the project takes `dotnet watch` down with it — it exits and leaves the console at a prompt. The
`Watch="false"` entries in the csproj do not prevent this: they keep those files out of the *watch* list, but
the build reads them either way.

If you would rather have styles compile on save, drop `dotnet watch`: run the app with a plain `dotnet run`
and `npm run watch:sass` beside it. That pairing is safe — there is no rebuild for a stylesheet write to land
in the middle of — and the cost is losing hot reload for `.cs` and `.cshtml`.

## ngrok
[ngrok](https://ngrok.com) exposes a local app on a public URL, so third parties can reach it — needed for
anything that calls back in (payment/email webhooks) or that validates the request origin (reCAPTCHA).

Two tunnels are defined, one per local app:

| Tunnel | Local app | Started by |
|---|---|---|
| `odk` | `http://localhost:8123` (dev) | `Scripts/run.ngrok.odk.bat` |
| `odk-e2e` | `http://localhost:8125` (e2e) | `E2E/script.run.ngrok.e2e.bat`, also opened as a third tab by `E2E/script.run.tests.bat` |

Both read `ngrok.yml` in the **repo root**. That file is gitignored (it holds your auth token), so create it
yourself with this structure, substituting your ngrok auth token and the reserved URL for each endpoint:

```yaml
version: 3

agent:
  authtoken: <AUTH_TOKEN>

endpoints:
  - name: odk
    url: <NGROK_URL>
    upstream:
      url: http://localhost:8123

  - name: odk-e2e
    url: <NGROK_URL>
    upstream:
      url: http://localhost:8125
```

YAML is indentation-sensitive: within each endpoint, `name`, `url` and `upstream` must line up in the same
column, and `upstream`'s own `url` is indented one level further.

## CSS
`.css` files are compiled into `wwwroot/css` from the `.scss` files in `wwwroot/scss`.

To compile, run `Scripts/run.build.css.bat` (or `npm run build:css` from `ODK.Web.Razor`).
`Scripts/run.app.bat` also compiles once before it starts the app.

`wwwroot/scss` imports Bootstrap's own Sass sources out of `wwwroot/lib`, so the compile needs the
client-side libraries in place. `build:css` restores them first, so there is no order to remember.

## CSS and JavaScript bundles
The layouts reference five bundles — one script bundle for every page, one for the admin area, one loaded in
`<head>`, one carrying the Ace editor, and one stylesheet bundle of the vendored CSS.
`ODK.Web.Razor/build/build-bundles.mjs` builds them with [esbuild](https://esbuild.github.io/), and `BUNDLES`
at the top of that script is the whole definition of what goes into each one.

Unlike `wwwroot/lib`, the outputs are **committed** — the same treatment the compiled CSS gets — so a clean
checkout serves the app before anything has run. They are still generated: don't edit them by hand.

The csproj runs the build after the client-library copy on every build, so any full build produces current
bundles. If `dotnet watch` does not pick up a script edit — it may treat one as a static-asset refresh rather
than a rebuild — run the bundle build yourself and hard-refresh, the way editing a `.scss` needs
`Scripts/run.build.css.bat`:

```
npm run build:bundles
```

Two things about it are deliberate and easy to undo by accident:

- **It concatenates and then minifies; it does not use esbuild's `--bundle`.** `--bundle` resolves a module
  graph and gives the result a scope. The vendored libraries are UMD builds, which inside a CommonJS wrapper
  assign to `module.exports` instead of `window` — so `window.bootstrap` would never appear. And
  `odk.global.js` declares a bare top-level `function setImageError` that `_MemberAvatar.cshtml` calls from an
  inline `onerror=`; in a scope, that silently stops working.
- **There is no watch mode**, for the same reason there is no Sass watcher: a process rewriting `wwwroot`
  while MSBuild is evaluating the project takes `dotnet watch` down with it.

Relative `url()` references in the vendored stylesheets are rewritten to absolute paths as they are
concatenated, because the bundle is served from a different directory than the file the reference was written
in — Font Awesome asks for `../webfonts/…` from `/lib/font-awesome/css/`, which resolves to nothing from
`/css/`.

## Client-side libraries
The browser libraries the app serves — Bootstrap, Font Awesome, TinyMCE, flatpickr and the rest — come from
npm, and `ODK.Web.Razor/build/copy-client-libs.mjs` copies them into `wwwroot/lib`.

`wwwroot/lib` is **generated and gitignored**. Nothing in it should be edited by hand; it is rebuilt whenever
a package version changes. The `RestoreClientLibraries` target in the csproj runs the copy on every build (and
then the bundle build below), so a plain `dotnet build`, `dotnet publish` or `Scripts/run.app.bat` produces a
working `wwwroot/lib` with no extra command. To run it on its own:

```
npm run build:lib
```

`COPIES`, at the top of the copy script, maps each package's npm layout onto the `lib/<library>/<file>` paths
the views and `build/build-bundles.mjs` reference — and is also the list of what gets served, since a file
it does not name never reaches the deploy. Adding a library means an `npm install --save-exact` plus a line or
two in `COPIES`. A path that moves in an upgrade fails the copy, naming every path it could not find.

Versions are pinned exactly, so `npm outdated` says what has moved on and `npm audit` says what is
vulnerable — both run from `ODK.Web.Razor`.

**`sass` is deliberately held at 1.101.3 rather than latest.** From 1.101.4 Dart Sass serialises `rgb()` with
percentage channels — `rgb(5.49%, 17.57%, 30.27%)` rather than `rgb(14, 44.8, 77.2)` — and Bootstrap's
`escape-svg()` does not escape `%`. The three custom properties whose value is an inline SVG data URI
(`--bs-form-switch-bg`, `--bs-accordion-btn-icon`, `--bs-accordion-btn-active-icon`) then carry a malformed
percent escape, the browser rejects the `fill`, and form switches lose their knob and accordions their
chevron. Recheck when Bootstrap escapes `%` in `$escaped-characters`.

Sass 1.100 and later also need **Node ≥ 20.19** — below that it dies with `ERR_REQUIRE_ESM` from chokidar.
It is the *developer's* Node that has to satisfy this, since the CSS is compiled locally and committed and CI
never runs Sass at all.

`ClientLibraryAssetTests` asserts that every `lib/…` path the app references, and every `url(…)` inside a
copied stylesheet, exists after a build. That is the guard when trimming a package down: an asset the browser
only asks for at runtime is invisible to the build and to every other test.

## Deployment
See [DEPLOYMENT.md](DEPLOYMENT.md) for how the app is built and deployed via GitHub Actions, how config
and secrets are managed, and how to add a new platform deployment.

## Subscriptions

Two independent kinds, both paid through Stripe Checkout:

| | Who pays | What for | Log table |
|---|---|---|---|
| **Chapter subscription** | a member | membership of one chapter | `MemberSubscriptionLog` |
| **Site subscription** | a group owner | platform features (`SiteFeatureType`) | `MemberSiteSubscriptionLog` |

The log tables are the source of truth. Each billing event **appends** a row rather than updating one, with
exactly one row flagged `IsCurrent` — so the payment history survives and the current state is a single read.

### Completion is webhook-driven, always

A completed checkout is recorded **only** by the Stripe webhook, processed off a Hangfire job. The return
page polls `/payments/sessions/{id}/status` and reloads once the webhook has landed; it records nothing
itself. Recording a payment from anywhere else would be a second path to recording it twice, so the webhook
stays the only writer.

Idempotency is keyed on the webhook **event** id, stored as `InitiatorId` on the appended row, with a unique
index as the backstop. A genuine renewal carries a distinct event id, so it is never mistaken for a retry.
Keying on the payment id instead would wrongly skip renewals, since recurring invoices reuse the original
checkout `Payment`.

### How the expiry date is set

**Recurring — the expiry *is* the next payment date.** It is read from the provider on the webhook
(`ExternalSubscription.NextBillingDate`, Stripe's subscription-item `current_period_end`), never calculated.
Calculating it lets the two drift apart: the provider anchors its schedule to the original purchase, while a
webhook arrives whenever it arrives, and every period compounds the delay of the one that carried it.
Reading it makes "expires" and "next charged" the same value by construction, so a membership can neither
lapse before the next charge nor outlive it.

Where the provider returns no date, the expiry degrades to the calculated date below rather than blocking
the payment from being recorded.

**One-off — calculated, continuing the existing period while the member is still effectively a member.** A
one-off has no schedule to read, so `PaymentService.RollExpiryForward` works it out:

- Expiry still in the future, or lapsed but within the chapter's cooldown → the new period continues from
  the old expiry, so an annual membership keeps its anniversary instead of drifting later every year.
- Otherwise → the period starts now.
- If continuing would land the new expiry in the past — a cooldown longer than the subscription's own length
  — the period starts now instead. A payment always has to leave the member current.

### The cooldown

`ChapterMembershipSettings.MembershipDisabledAfterDaysExpired` is how long an expired membership keeps its
access, and it does double duty: it is both that grace period and the window in which a renewal continues
the previous period. One setting, one meaning — "still effectively a member".

`AuthorizationService.GetSubscriptionStatus` reads it:

| Condition | Status |
|---|---|
| No expiry date (`ExpiresUtc == null`) | `Current` |
| Expiry beyond `MembershipExpiringWarningDays` | `Current` |
| Expiry within `MembershipExpiringWarningDays` | `Expiring` |
| Expired, within the cooldown | `Expired` |
| Expired, past the cooldown | `Disabled` |

A cooldown of **none (0) means access ends with the subscription** — an expired membership is immediately
`Disabled`. A negative value is meaningless and is treated as none. **A membership that never ends is
expressed by having no expiry date at all**, not by a sentinel cooldown value; if that is ever wanted as a
configurable feature it should be an explicit one.

## Workflows

Flows that vary by platform, entry point and what the member already has — account creation above all — are
modelled as state machines rather than as branches inside a service method: states, the triggers between
them, the conditions deciding which move is legal, and the ordered work each move performs.

The diagrams are **generated from the definitions the app executes**, and a test fails the build when a
committed page no longer matches its definition. See [docs/workflows](docs/workflows/README.md) for how a
machine is built and how to view the diagrams; GitHub renders them inline.

## Database keys

Every table keys on a `uniqueidentifier` called `Id`, and **the app generates it, not the database**. Two
things depend on the key existing before the row does:

- A payment's ids are sent to the payment provider as checkout metadata *before* the rows are written, and
  come back on the webhook — they are how a callback is matched to what started it.
- Foreign keys are set by value (`MemberId = member.Id`) while several rows are staged for one commit, so a
  key that only appeared on save would read as empty everywhere it is used in between.

That rules out a `NEWSEQUENTIALID()` column default and EF's own value generation, both of which supply the
key at insert.

**Keys are sequential, because the clustered index cares.** A random GUID lands anywhere in the index, so
every insert can split a page; a key that ascends appends instead. Which key each table clusters on is a
deliberate choice — see the data-access notes in [CLAUDE.md](CLAUDE.md) — but whatever it is, ascending keys
are what keep writes at the end of it.

**Sequential means sequential *to SQL Server*, which is the part that catches people out.** SQL Server does
not compare a `uniqueidentifier` in byte order: it compares the last six bytes first, then bytes 8-9, 6-7,
4-5 and 0-3. So a version 7 UUID, which carries its timestamp in the *first* six bytes, sorts as randomly
here as a version 4 does. `SequentialIdGenerator` wraps EF's `SequentialGuidValueGenerator`, which puts its
counter where SQL Server actually looks.

Three properties make that sound in this app:

- **One sequence.** The counter is static, so every caller draws on it — whether it arrives through
  `IUnitOfWork.NewId()` or through the repository base classes, which have nothing to inject through.
- **Restarts resume above the previous run.** The counter is seeded from `DateTime.UtcNow.Ticks` and
  increments once per key, while ticks advance ten million times a second. A new process therefore starts far
  beyond where the last one stopped.
- **Sharing one sequence across tables costs nothing.** Each table has its own index and only cares that its
  own inserts ascend, which a single monotonic counter gives all of them at once.

Nothing generates a database key with `Guid.NewGuid()`.

## Antiforgery (CSRF)

Antiforgery validation is enabled globally: Razor Page POST handlers validate by default, and MVC
controllers validate via the `AutoValidateAntiforgeryTokenAttribute` filter registered in `Program.cs`.
Every state-changing POST must therefore carry a valid antiforgery token, so **every `<form method="post">`
needs a token in the rendered HTML**. There must be exactly **one** token per form — zero fails validation,
and two (duplicate) tokens also fail (the values are comma-joined and no longer deserialize).

### When a form needs an explicit `@Html.AntiForgeryToken()`

The Form Tag Helper decides whether it auto-emits a hidden token field:

| Form | Tag Helper auto-emits? | What to write |
|---|---|---|
| `<form method="post">` (no `action`) | **Yes** | Nothing — do **not** add `@Html.AntiForgeryToken()` (that would duplicate it). |
| `<form method="post" asp-controller/asp-page/asp-action=...>` | **Yes** | Nothing — the token is auto-emitted. |
| `<form method="post" action="/some/url">` (literal `action`) | **No** | Add `@Html.AntiForgeryToken()` as the first child of the form. |

Rule of thumb: **a literal `action` attribute suppresses the auto-token, so those forms — and only those —
need an explicit `@Html.AntiForgeryToken()`.** This project posts most mutating forms to controller
endpoints via a literal `action` (the Post/Redirect/Get split), so most forms carry an explicit token.

**The table above applies to `Pages/**` only.** `Views/_ViewImports.cshtml` deliberately registers just
`ScriptTagHelper` and `LinkTagHelper` (for `asp-append-version`), *not* `Microsoft.AspNetCore.Mvc.TagHelpers`
wholesale, so the Form Tag Helper never runs in the shared partials under `Views/**`. Nothing auto-emits a
token there: **every** `<form method="post">` in a `Views/**` partial needs its own
`@Html.AntiForgeryToken()`, literal `action` or not. Do not widen that file to `@addTagHelper *` — it would
add a second token to every no-`action` form in `Views/**` and 400 them all.

### AJAX POSTs

A `fetch(..., { method: 'POST' })` has no form field, so it must send the token as a header. The layout
renders it in a `<meta name="request-verification-token">` tag; read it with the shared helper and spread
it into the request:

```js
fetch(url, { method: 'POST', headers: window.odk.antiforgeryHeaders() })
```

The configured header name is `RequestVerificationToken` (see `AddAntiforgery` in `Program.cs`).

### Exemptions

Only endpoints that are **not** first-party browser POSTs are exempt, via `[IgnoreAntiforgeryToken]`:

- `WebhooksController` (Stripe / Brevo) — authenticated by provider signature/secret.
- `ScheduledTasksController` (cron) — authenticated by the ScheduledTasks API key.
- `AccountController.GoogleSiteLogin` / `GoogleChapterLogin` — the token is posted by Google Identity JS,
  not a first-party form.

Do not add new exemptions for ordinary forms; fix the token instead.