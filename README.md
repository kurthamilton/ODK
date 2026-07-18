# Getting started

## Installation
1. Install the latest version of .NET
2. Install the latest version of SQL Server
3. Take a backup of the prod DB and restore locally

## Apps
The project runs two different platforms based on the base URL.

### ODK
The Drunken Knitwits platform, specifically for Drunken Knitwits groups around the world.

### Group Squirrel
A Meetup-style platform currently under development.

## Running locally
Run `run-odk.bat` to run the ODK platform or `run-gs.bat` to run the Group Squirrel platform.

The batch file spawns two tabs: the `dotnet` process in the main tab and a sass builder in the other.

## CSS
`.css` files are compiled into `wwwroot/css` from the `.scss` files in `wwwroot/scss`.

To compile, run `npm run build:css`. The compilation script also runs when the app is run from one of the batch files.

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