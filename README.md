# Getting started

## Installation
1. Install the latest version of .NET
2. Install the latest version of SQL Server
3. Take a backup of the prod DB and restore locally
4. Install [ngrok](https://ngrok.com/download) and create `ngrok.yml` in the repo root (see [ngrok](#ngrok))

## Apps
The project runs two different platforms based on the base URL.

### ODK
The Drunken Knitwits platform, specifically for Drunken Knitwits groups around the world.

### Group Squirrel
A Meetup-style platform currently under development.

## Running locally
Run `run.app.bat`. One process serves **both** platforms — Group Squirrel on
[localhost:8123](http://localhost:8123) and ODK on [localhost:8124](http://localhost:8124) — with the
platform resolved from the request URL (the `Platforms` config in `appsettings.Development.json`).

It opens a Windows Terminal window with two tabs: `app` (`dotnet watch`, hot reload) and `sass` (the SCSS
watchers). They're separate tabs rather than one `concurrently` process because `concurrently` redirects
stdin, which silently disables `dotnet watch`'s keyboard shortcuts — in its own tab you keep **Ctrl+R** to
force a restart when a change isn't picked up or the app doesn't recover from an error.

Use `run.app.simple.bat` for a plain run with no hot reload and no watchers (compiles the SCSS once first).

## ngrok
[ngrok](https://ngrok.com) exposes a local app on a public URL, so third parties can reach it — needed for
anything that calls back in (payment/email webhooks) or that validates the request origin (reCAPTCHA).

Two tunnels are defined, one per local app:

| Tunnel | Local app | Started by |
|---|---|---|
| `odk` | `http://localhost:8123` (dev) | `run.ngrok.odk.bat` (repo root) |
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

To compile, run `npm run build:css`. The compilation script also runs when the app is run from one of the batch files.

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