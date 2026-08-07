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