# Deployment

How ODK is built and deployed to the hosting provider, and how to add a new platform deployment from scratch.

> **Status:** first draft for review. Nothing here changes app code — deployment is entirely CI/CD + config.

## 1. Overview

The app is one codebase serving multiple **platforms** (Group Squirrel = `Default`, Drunken Knitwits =
`DrunkenKnitwits`), each hosted on its **own site** at the hosting provider under its own domain. The running
app decides which platform a request is for from the request URL (the `Platforms` config), so the **same build
artifact is deployed to every site** — nothing platform-specific is baked into the binary.

Two GitHub Actions workflows:

| Workflow | Trigger | What it does |
|---|---|---|
| `.github/workflows/build.yml` | automatic, on push to `master` (or manual) | runs unit tests, publishes the app, uploads a secret-free artifact called `app` |
| `.github/workflows/deploy.yml` | manual (`Run workflow`) | takes the latest successful build, injects config + secrets, Web Deploys to the chosen site |

```
push to master ──▶ Build (test + publish) ──▶ artifact "app"
                                                   │
   you click Run workflow, pick a target env       ▼
                          Deploy ──▶ merge config + Doppler secrets ──▶ Web Deploy ──▶ hosting provider site
```

Build and deploy are **separate on purpose**: every push is validated automatically, and deploys are a
deliberate manual click that ships the already-built artifact (no rebuild). Both run on `windows-latest`
because Web Deploy (`msdeploy`) is Windows-only.

## 2. Where each value lives

At deploy time the pipeline builds `appsettings.Production.json` from three sources, which layer over the
committed base `appsettings.json`:

| Kind of value | Home | Example |
|---|---|---|
| Non-sensitive config, same for every site | committed `ODK.Web.Razor/config.production.json` (public) | `Platforms` (your live domains) |
| Non-sensitive config that differs per site | GitHub **environment Variable** (`vars.*`, viewable, scoped per environment) | `Logging:Path` |
| Credentials | Doppler (leaf secret) | `ConnectionStrings:Default`, `Payments:Stripe:WebhookSecretV1` |
| Sensitive **string list** you don't want public | Doppler (secret whose value is a **newline-delimited list**) | `RateLimiting:BlockPatterns` |
| Sensitive **structured** config (array of objects) | Doppler (secret whose **value is JSON**) | `Logging:IgnoreExceptions` |

**Decision rule for any value:** would I mind it being on public GitHub? **Yes → Doppler.** No → is it the
same on every site? **Same everywhere → `config.production.json`; differs per site → a GitHub environment
Variable** (`vars.*`).

> **Variables vs secrets.** GitHub *secrets* are write-only (see §3). GitHub *Variables* are plaintext and
> **viewable/editable in the UI**, so they're the right home for non-sensitive per-site config — you can read
> them back, unlike a secret. Never put a credential in a Variable.

Key points:

- **The app never calls Doppler.** Doppler is read once, at deploy time, by the pipeline. A Doppler outage
  can't affect a running or restarting site.
- **The app is unchanged** — it still reads `appsettings.json` + `appsettings.Production.json`. The latter is
  gitignored and only ever built inside the deploy runner (never committed, never hand-placed on the server).
- **A key must have exactly one home** across `config.production.json`, Doppler, and the GitHub Variables — if
  the same key comes from two of them the merged file contains a duplicate key and .NET's JSON provider throws
  on load.

### How the merge works (reference)

- Doppler stores flat keys. Its JSON import delimits config levels with a single underscore, so
  `Payments:Stripe:WebhookSecretV1` becomes `PAYMENTS_STRIPE_WEBHOOKSECRETV1`. The pipeline converts `_` → `:`.
  This is safe because no .NET config-key segment contains an underscore. Case doesn't matter — .NET matches
  config keys case-insensitively, so `PAYMENTS:STRIPE:WEBHOOKSECRETV1` overrides the nested key from the base
  `appsettings.json`.
- Doppler supplies **arrays** two ways:
  - **String lists** (`RateLimiting:BlockPatterns`, `RateLimiting:BlockIpAddresses`, `RateLimiting:BlockPaths`)
    are stored as a **newline-delimited plain-text** value — one entry per line, **no JSON escaping**. The
    pipeline splits them on newlines into a `string[]`. This is the friendly way to edit a regex blocklist: the
    raw regex goes in as-is (`.*/admin\.aspx`), no doubled backslashes. Newline is the delimiter (not comma)
    because regexes contain commas (`a{2,3}`). These keys are handled *before* the JSON check, so an entry that
    starts with `[` (e.g. `[0-9]+\.php`) isn't mistaken for JSON.
  - **Structured values** (arrays of objects like `Logging:IgnoreExceptions`) are stored as **JSON** — a value
    starting with `[` or `{` is parsed and injected as real structure. JSON strings must be validly escaped
    (backslashes doubled) since plain text can't represent nested objects.
  - Either way the config binds to a real array, not a single string. To make a *new* key a newline list, add it
    to `$stringListKeys` in `deploy.yml`'s build step.
- Finally, any per-environment GitHub Variable the step reads (currently `LOGGING_PATH` → `Logging:Path`) is
  injected — but only when set, so it stays optional. It's added by its full config key (e.g. `Logging:Path`),
  which overrides the nested key from the base `appsettings.json` the same way the Doppler keys do.

## 3. One-time repository setup

Do this once (it already exists today; documented here for completeness / disaster recovery).

1. **Workflow files** — `.github/workflows/build.yml` and `deploy.yml` are committed on `master`. They only
   become active (triggers fire, the `Run workflow` button appears) once they're on the default branch.
2. **Public prod config** — `ODK.Web.Razor/config.production.json` holds the non-sensitive prod config. It is
   *not* named `appsettings.*.json`, so it isn't caught by `.gitignore` and commits normally.
3. **Shared repo secrets** (Settings → Secrets and variables → Actions → *Repository secrets*):
   - `HOSTING_USER` — the hosting provider's Web Deploy username (shared across sites on one hosting account).
   - `HOSTING_PASSWORD` — its password.
   - `DOPPLER_TOKEN` — a **read-only** Doppler service token scoped to the prod config.

   > GitHub secrets are write-only — you can never read a value back in the UI, only overwrite it. That's
   > expected. The real secret values live in Doppler where they're viewable/auditable; `DOPPLER_TOKEN` is the
   > only value you can't see, and you can regenerate it from Doppler at any time.
4. **Doppler** — create a project (e.g. `odk`), use its `prd` config, and add the secrets (see §5). Generate a
   read-only service token for `prd` and store it as `DOPPLER_TOKEN` above.

## 4. Adding a platform (new deployment target)

Follow these steps to stand up a new site for a platform. (A brand-new *platform type* beyond `Default` /
`DrunkenKnitwits` additionally needs an app change — a new `PlatformType` value and `PlatformProvider`
handling — which is outside this deployment doc. These steps assume the platform type already exists.)

**Step 1 — Create the site at the hosting provider.** In the hosting provider's control panel, create the
website for the new domain and bind the domain(s). If it's on the same hosting account as the existing sites,
the Web Deploy username/password are shared; if it's a separate account, note its own credentials.

**Step 2 — Get its Web Deploy settings.** From the site's *Publish Settings* (or a one-off VS publish
profile), note:
- **Service URL** — `MSDeployServiceURL`, e.g. `https://<host>:8172/msdeploy.axd?site=<name>`
- **Site name** — `DeployIisAppPath`
- **Username / password** (only if this site uses different credentials from the shared repo secrets)

**Step 3 — Create the GitHub environment.** Settings → *Environments* → **New environment**. Name it
`prod-<platform>` (existing ones are `prod-odk`, `prod-gs`). Using environments purely to scope secrets is
free on private and public repos.

**Step 4 — Add the environment's deploy secrets** (inside that environment, *Add secret*):
- `HOSTING_MSDEPLOY_URL` — the Service URL from step 2.
- `HOSTING_SITE` — the site name from step 2.

  If this site uses different credentials, also add `HOSTING_USER` / `HOSTING_PASSWORD` here — an environment
  secret overrides the shared repo secret of the same name. Otherwise leave them at repo level.

**Step 5 — Map the domain to the platform.** Add an entry to `Platforms` in
`ODK.Web.Razor/config.production.json` so the running app routes the new domain to the right platform:

```json
{
  "Platforms": [
    { "BaseUrl": "https://<group-squirrel-domain>",  "Type": "Default" },
    { "BaseUrl": "https://<drunken-knitwits-domain>", "Type": "DrunkenKnitwits" }
  ]
}
```

`Type` is the `PlatformType` name (`Default` or `DrunkenKnitwits`); `BaseUrl` is the site's public domain.
Commit this change to `master` (it triggers a build).

**Step 6 — (Optional) platform-specific config/secrets.** If the new platform needs *different* values from
the others:

- **Non-sensitive** (e.g. a distinct `Logging:Path`): add it as an **environment Variable** in `prod-<platform>`
  (Settings → Environments → the environment → *Variables*). The pipeline reads `vars.LOGGING_PATH` and injects
  it as `Logging:Path` only when set. Ensure the same key isn't also in `config.production.json` or Doppler
  (one home per key). To wire up a *new* per-site key beyond `LOGGING_PATH`, add it to the env block and the
  inject line in `deploy.yml`'s "Build appsettings.Production.json" step, mirroring `LOGGING_PATH`.
- **Sensitive**: create a Doppler **branch config** (e.g. `prd_<platform>`) with the differing secrets, generate
  a service token for it, and add that token as a `DOPPLER_TOKEN` **environment** secret in `prod-<platform>`
  (it overrides the repo one).

Nothing else in the workflows changes. If the platform shares config (the default today), skip this — it uses
the shared `config.production.json`, the shared `prd` Doppler config, and the repo `DOPPLER_TOKEN`.

**Step 7 — Deploy.** Actions → **Deploy** → **Run workflow** → pick `prod-<platform>` (see §6).

## 5. Managing configuration & secrets

### Adding / changing a secret (Doppler)

- Open the Doppler `prd` config, add or edit the secret. Name it as the config path with the levels the way
  Doppler's JSON import writes them — a single `_` between levels, e.g. `Google:Maps:ApiKey` → `GOOGLE_MAPS_APIKEY`.
- For a **sensitive string list** (`RateLimiting:BlockPatterns` / `BlockIpAddresses` / `BlockPaths`), set the
  secret's value to a **newline-delimited plain-text list** — one entry per line, **no quotes, brackets, or
  escaping**. Regexes go in raw:
  ```
  .*/admin\.aspx
  .*\.php$
  wp-login
  ```
- For **structured** sensitive config (an array of objects, e.g. `Logging:IgnoreExceptions`), set the value to
  **JSON** (`[…]`/`{…}`). JSON strings must be validly escaped — every backslash doubled.
- Changes take effect on the **next deploy** (the app doesn't read Doppler live). Re-run Deploy to apply.

### Adding / changing public config

- Edit `ODK.Web.Razor/config.production.json` and commit to `master`. It ships with the next build/deploy.

### Adding / changing per-site non-sensitive config (GitHub Variables)

- Settings → Environments → the environment (`prod-odk` / `prod-gs`) → *Variables*. Values are viewable and
  editable here. `LOGGING_PATH` is wired up already; it applies on the next deploy of that environment.
- To add a *new* per-site key, extend `deploy.yml`'s "Build appsettings.Production.json" step: add
  `<KEY>: ${{ vars.<KEY> }}` to the step `env` and a matching `if ($env:<KEY>) { … 'Config:Key' … }` inject
  line, mirroring `LOGGING_PATH` → `Logging:Path`.

### The one rule

A key must live in **exactly one** of `config.production.json`, Doppler, or a GitHub environment Variable —
never two, or the merged `appsettings.Production.json` has a duplicate key and fails to load. The empty
defaults in the committed `appsettings.json` are fine (different file/provider); the deploy-time sources
override them.

## 6. Deploying (routine)

1. Merge your change to `master`. **Build** runs automatically: tests, publish, uploads the `app` artifact.
2. Actions → **Deploy** → **Run workflow** → choose the target environment (`prod-odk` / `prod-gs` / …) → Run.
3. Deploy downloads the latest successful `master` build, builds `appsettings.Production.json` (public config +
   Doppler), and Web Deploys to that site. Repeat step 2 for each site you want to update — the same artifact
   ships to all.

Web Deploy uses `-enableRule:DoNotDeleteRule` (won't delete server files missing from the publish — logs,
uploads, `App_Data`) and `-enableRule:AppOffline` (drops `app_offline.htm` during the copy so IIS releases the
DLL lock, then removes it).

## 7. Troubleshooting

- **"No successful Build run found on master to deploy."** There's no green build to deploy — push to `master`,
  or run **Build** manually (`Run workflow`), then deploy.
- **Deploy can't find the artifact / it expired.** Build artifacts are kept 7 days (`retention-days` in
  `build.yml`). If you haven't deployed in over a week, run **Build** first to produce a fresh one.
- **`A duplicate key '…' was found`** on the site after deploy. A key is in *both* `config.production.json` and
  Doppler. Remove it from one.
- **A sensitive list deployed as a single string.** For a **newline list** (`BlockPatterns` etc.), the key must
  be in `$stringListKeys` in `deploy.yml` and each entry on its own line. For a **JSON** value it must start with
  `[`/`{` and be valid JSON (backslashes doubled) — an invalid escape (e.g. a lone `\.`) fails to parse and
  silently falls back to a string.
- **`msdeploy` auth / certificate errors.** Check the `HOSTING_*` values against the site's current publish
  settings; the workflow already passes `-allowUntrusted` for the hosting provider's certificate.
- **`msdeploy`: "Unrecognized argument … All arguments must begin with -".** PowerShell mangled the quoted
  `-dest:` provider string before msdeploy saw it. The Web Deploy step avoids this by calling msdeploy through
  the stop-parsing token `--%`, which passes everything after it verbatim. Two consequences to preserve if you
  edit that step: (1) after `--%` the line **can't** use `$env:X` or backtick continuations — variables are
  expanded cmd-style as `%VAR%` from the step `env`, and the whole invocation stays on **one line**; (2) `--%`
  does cmd-style `%…%` expansion, so a **`HOSTING_PASSWORD` containing a literal `%`** would be misread. If a
  password ever contains `%`, drop `--%` and build the arguments as a PowerShell array instead (each `-key:value`
  as its own quoted string element), which passes them without cmd expansion.
- **Target dropdown is empty when running Deploy.** The environment doesn't exist yet — create `prod-<platform>`
  (§4 step 3).
```
