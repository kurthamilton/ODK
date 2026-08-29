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
| `.github/workflows/build.yml` | automatic, on push to `master` (or manual) | installs the npm client-side libraries, runs unit tests, publishes the app (artifact `app`), and creates a self-contained EF migration bundle (artifact `efbundle`) |
| `.github/workflows/deploy.yml` | automatic, when a `master` Build **succeeds** (or manual `Run workflow`) | applies migrations once, then deploys the same build to **both** sites |

```
push to master ──▶ Build (test + publish + migration bundle) ──▶ artifacts "app" + "efbundle"
                                                                         │  Build succeeded on master
                                                                         ▼
                    Deploy:  migrate (efbundle → prod DB, once)  ──▶  deploy matrix ──▶  prod-odk site
                                                                                   └──▶  prod-gs  site
                             (each site: merge config + Doppler secrets → Web Deploy)
```

The pipeline is **fully automatic**: a push to `master` builds, and a successful build deploys — migrating the
shared prod database once, then shipping the identical artifact to both sites in parallel. `Run workflow` on
Deploy re-runs the same flow manually against the latest successful `master` build. Everything except the
artifact-resolution step runs on `windows-latest` because Web Deploy (`msdeploy`) and the migration bundle are
Windows binaries.

> **Migration safety.** Migrations run *before* the app deploys, so during that window the still-running old
> code sees the new schema — only ever apply **backward-compatible (expand-then-contract)** migrations. The
> migrate step runs in its own `prod-migrate` environment; add a required reviewer there (Settings →
> Environments) if you later want a manual approval gate before prod migrations, with no YAML change.

## 2. Where each value lives

At deploy time the pipeline builds `appsettings.Production.json` from two sources, which layer over the
committed base `appsettings.json`:

| Kind of value | Home | Example |
|---|---|---|
| Public, and identical in every environment | committed `ODK.Web.Razor/appsettings.json` | `Platforms:*:Name` |
| Anything environment-specific, sensitive or not | Doppler (leaf secret) | `Platforms:Default:Urls`, `ConnectionStrings:Default`, `Payments:Stripe:Platforms:Default:WebhookSecretV1` |
| Non-sensitive config that differs per site | GitHub **environment Variable** (`vars.*`, viewable, scoped per environment) | `Logging:Path` |
| **String list** | Doppler (secret whose value is a **newline-delimited list**) | `RateLimiting:BlockPatterns` |
| **Structured** config, and any **array** | Doppler (secret whose **value is JSON**) | `Platforms:Default:Urls`, `Logging:IgnoreExceptions` |
| **Dictionary** (keys are data, not a config path) | Doppler (**one** secret for the whole dictionary, **value is a JSON object**) | `Instagram:Client:Cookies` |

**Decision rule for any value:** does it differ per site? **Yes → a GitHub environment Variable** (`vars.*`),
and only if it is non-sensitive. **No →** is it public and the same in *every* environment, production and local
alike? **Yes → the committed `appsettings.json`; otherwise → Doppler**, which is the single source of everything
the deploy injects, so there is one place to look and one place to change.

> **`Platforms` is split between the two on purpose.** A platform's `Name` is the same everywhere, so it is
> committed. Its `Urls` are not — production has the live domains, local has `localhost` ports — so the
> committed file declares `"Urls": []` and each environment states its own: Doppler for production, the
> git-ignored `appsettings.<env>.json` for everything else. Leaving base empty also matters mechanically: see
> the array-merge note below.

> **Variables vs secrets.** GitHub *secrets* are write-only (see §3). GitHub *Variables* are plaintext and
> **viewable/editable in the UI**, so they're the right home for non-sensitive per-site config — you can read
> them back, unlike a secret. Never put a credential in a Variable.

Key points:

- **The app never calls Doppler.** Doppler is read once, at deploy time, by the pipeline. A Doppler outage
  can't affect a running or restarting site.
- **The app is unchanged** — it still reads `appsettings.json` + `appsettings.Production.json`. The latter is
  gitignored and only ever built inside the deploy runner (never committed, never hand-placed on the server).
- **A key must have exactly one home** across Doppler and the GitHub Variables — if the same key comes from
  both the merged file contains a duplicate key and .NET's JSON provider throws on load.

### How the merge works (reference)

- Doppler stores flat keys. Its JSON import delimits config levels with a single underscore, so
  `Payments:Stripe:Platforms:Default:WebhookSecretV1` becomes
  `PAYMENTS_STRIPE_PLATFORMS_DEFAULT_WEBHOOKSECRETV1`. The pipeline converts `_` → `:`. Case doesn't matter
  for a *settings* key — .NET matches config keys case-insensitively, so
  `PAYMENTS:STRIPE:PLATFORMS:DEFAULT:WEBHOOKSECRETV1` overrides the nested key from the base
  `appsettings.json`. The platform name is a config *level* here, not data, which is why it survives the
  upper-casing intact - see the dictionary rule below for the case that does not.
- **A Doppler key name cannot carry a literal underscore, hyphen, or lower-case letter into a config segment.**
  The `_` → `:` conversion is unconditional, so an underscore *within* a name splits it into another level, and
  Doppler names are upper-snake-case, so nothing lower-case survives. That's invisible for ordinary settings
  keys — they have no underscores and are matched case-insensitively — but it silently corrupts a
  **dictionary-shaped** setting, where the key is *data* rather than a path. See the JSON rule below.
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
  - **Dictionaries must use the JSON form too** — one secret for the whole dictionary, not one per entry. A
    dictionary's keys are *values* (`Instagram:Client:Cookies` binds to `IReadOnlyDictionary<string, string>`
    whose keys are the cookie names actually sent), so they have to survive verbatim, and a Doppler key name
    can't do that. `INSTAGRAM_CLIENT_COOKIES_DS_USER_ID` arrives as
    `Instagram:Client:Cookies:DS:USER:ID` — nested objects instead of a cookie named `ds_user_id` — and even
    `SESSIONID` is the wrong name for a case-sensitive cookie. One secret holding
    `{"ds_user_id":"…","sessionid":"…"}` binds to the exact names. Same for `Instagram:Client:Headers`, whose
    names contain hyphens that a Doppler key can't hold at all.
  - Either way the config binds to a real array, not a single string. To make a *new* key a newline list, add it
    to `$stringListKeys` in `deploy.yml`'s build step. The two forms are mutually exclusive: `$stringListKeys` is
    applied first, so a key in that list can never carry JSON, and a key not in it must be JSON to become an
    array. A plain string where an array is expected binds the property to **null** rather than to an empty
    array, and the failure lands wherever the collection is first enumerated.
- **An array in the base `appsettings.json` is merged into, not replaced.** Every provider is flattened to
  indexed keys (`Platforms:Default:Urls:0`), so a later source only overrides the indices it states and longer
  base arrays keep their tail. That is why the committed `Platforms` declares `"Urls": []` — an environment
  stating one URL against a base of two would otherwise inherit the second. The rule for any array config might
  override: **leave it empty in the base file**, so the environment's value is the whole value.
- Finally, each per-environment GitHub Variable in the step's `$environmentOverrides` map (currently
  `HANGFIRE_SCHEMANAME` → `Hangfire:SchemaName` and `LOGGING_PATH` → `Logging:Path`) is injected — but only
  when set, so it stays optional. It's added by its full config key (e.g. `Logging:Path`), which overrides the
  nested key from the base `appsettings.json` the same way the Doppler keys do.

## 3. One-time repository setup

Do this once (it already exists today; documented here for completeness / disaster recovery).

1. **Workflow files** — `.github/workflows/build.yml` and `deploy.yml` are committed on `master`. They only
   become active (triggers fire, the `Run workflow` button appears) once they're on the default branch.
2. **Shared repo secrets** (Settings → Secrets and variables → Actions → *Repository secrets*):
   - `HOSTING_USER` — the hosting provider's Web Deploy username (shared across sites on one hosting account).
   - `HOSTING_PASSWORD` — its password.
   - `DOPPLER_TOKEN` — a **read-only** Doppler service token scoped to the prod config.

   > GitHub secrets are write-only — you can never read a value back in the UI, only overwrite it. That's
   > expected. The real secret values live in Doppler where they're viewable/auditable; `DOPPLER_TOKEN` is the
   > only value you can't see, and you can regenerate it from Doppler at any time.
3. **Doppler** — create a project (e.g. `odk`), use its `prd` config, and add the secrets (see §5). Generate a
   read-only service token for `prd` and store it as `DOPPLER_TOKEN` above. The `prd` config must include
   `CONNECTIONSTRINGS_DEFAULT` (the migrate job reads it to apply migrations).
5. **Environments** (Settings → *Environments*): `prod-odk` and `prod-gs` (one per site, holding that site's
   `HOSTING_MSDEPLOY_URL` / `HOSTING_SITE` — see §4), plus **`prod-migrate`** for the migration job. `prod-migrate`
   needs no secrets of its own (it uses the repo `DOPPLER_TOKEN`); it exists so the prod-DB migration is isolated
   and can be gated with a required reviewer later without a YAML change.

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

**Step 3 — Create the GitHub environment and register it in the matrix.** Settings → *Environments* → **New
environment**, named `prod-<platform>` (existing ones are `prod-odk`, `prod-gs`, plus `prod-migrate`). Using
environments purely to scope secrets is free on private and public repos. Then add the new environment to the
deploy matrix so the pipeline ships to it: in `deploy.yml`, add `prod-<platform>` to
`jobs.deploy.strategy.matrix.environment`.

**Step 4 — Add the environment's deploy secrets** (inside that environment, *Add secret*):
- `HOSTING_MSDEPLOY_URL` — the Service URL from step 2.
- `HOSTING_SITE` — the site name from step 2.

  If this site uses different credentials, also add `HOSTING_USER` / `HOSTING_PASSWORD` here — an environment
  secret overrides the shared repo secret of the same name. Otherwise leave them at repo level.

**Step 5 — Map the domain to the platform.** The platform's live domains live in Doppler, in a secret named
for its config path — `PLATFORMS_DEFAULT_URLS` for `Platforms:Default:Urls`. Its value must be a **JSON array**:

```json
[ "https://<group-squirrel-domain>", "https://www.<group-squirrel-domain>" ]
```

A platform may list several URLs (an apex domain and its `www.`); every one of them resolves to the platform, and
the **first** is the one the app builds links against, so put the canonical domain first. Its `Name` comes from
the committed `appsettings.json`, which is the only part of `Platforms` that lives there. The change ships with
the next deploy — no commit needed.

> **It has to be JSON, not a newline-delimited list.** A newline list would bind `Urls` to **null** rather than
> to an array, and `PlatformProvider.GetPlatform` enumerates it on every request — so the whole site would 500.
> The newline form is only for the keys named in `$stringListKeys` in `deploy.yml`, and it is deliberately not
> available here: that handling runs before the JSON detection, so a key in that list can never carry JSON.

**Step 6 — Platform-specific config/secrets.**

- **A Hangfire schema — required.** Every site shares the one prod database, and a Hangfire server runs
  whatever it finds in its own storage, so two sites pointed at one schema each run the other's background
  jobs. Set `HANGFIRE_SCHEMANAME` as an **environment Variable** in `prod-<platform>` to a name no other site
  uses. The committed default is `Hangfire`, which `prod-odk` keeps; `prod-gs` uses `Hangfire2`. The names are
  deliberately numbered rather than named after a platform, so renaming a platform never strands a schema.
  Hangfire creates the schema and its tables on first start, so the SQL login needs rights to do so.
- **Other non-sensitive values** (e.g. a distinct `Logging:Path`): add them as environment Variables the same
  way (Settings → Environments → the environment → *Variables*), and ensure the same key isn't also in Doppler
  (one home per key). To wire up a *new* per-site key, add it to the env block and to `$environmentOverrides`
  in `deploy.yml`'s "Build appsettings.Production.json" step, mirroring `LOGGING_PATH`.
- **Sensitive**: create a Doppler **branch config** (e.g. `prd_<platform>`) with the differing secrets, generate
  a service token for it, and add that token as a `DOPPLER_TOKEN` **environment** secret in `prod-<platform>`
  (it overrides the repo one). If the platform shares its secrets (the default today), skip this — it uses the
  shared `prd` Doppler config and the repo `DOPPLER_TOKEN`.

Nothing else in the workflows changes.

**Step 7 — Ship it.** Merge to `master`. Build runs, and on success Deploy automatically migrates the shared DB
once and deploys every site in the matrix — including the new one. (Or trigger Deploy manually via **Run
workflow**; see §6.)

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
- For **structured** config and for any plain array (an array of objects like `Logging:IgnoreExceptions`, or a
  list of strings like `Platforms:Default:Urls` → `PLATFORMS_DEFAULT_URLS`), set the value to **JSON**
  (`[…]`/`{…}`). JSON strings must be validly escaped — every backslash doubled.
- For a **dictionary** (`Instagram:Client:Cookies`, `Instagram:Client:Headers`), use **one secret for the whole
  dictionary**, with a JSON object as its value — never one secret per entry:
  ```
  INSTAGRAM_CLIENT_COOKIES = {"ds_user_id":"…","sessionid":"…","csrftoken":"…"}
  ```
  A dictionary's keys are data, not a config path, so they must arrive character-for-character. A per-entry
  secret can't manage that: `_` becomes `:` (so `ds_user_id` splits into three levels) and Doppler names are
  upper-case (so a case-sensitive cookie or a hyphenated header name is unreachable). The failure is silent —
  the config loads fine and the request just goes out with the wrong names.
- Changes take effect on the **next deploy** (the app doesn't read Doppler live). Re-run Deploy to apply.

### Adding / changing per-site non-sensitive config (GitHub Variables)

- Settings → Environments → the environment (`prod-odk` / `prod-gs`) → *Variables*. Values are viewable and
  editable here. `HANGFIRE_SCHEMANAME` and `LOGGING_PATH` are wired up already; a change applies on the next
  deploy of that environment.
- `HANGFIRE_SCHEMANAME` is per-site by necessity, not by preference — see Step 6 in §4. `prod-gs` sets
  `Hangfire2`; `prod-odk` sets nothing and takes the committed `Hangfire`.
- To add a *new* per-site key, extend `deploy.yml`'s "Build appsettings.Production.json" step: add
  `<KEY>: ${{ vars.<KEY> }}` to the step `env` and a matching `'Config:Key' = $env:<KEY>` entry to
  `$environmentOverrides`, mirroring `LOGGING_PATH` → `Logging:Path`.

### The one rule

A key must live in **exactly one** of Doppler or a GitHub environment Variable — never both, or the merged
`appsettings.Production.json` has a duplicate key and fails to load. The empty defaults in the committed
`appsettings.json` are fine (different file/provider); the deploy-time sources override them.

## 6. Deploying (routine)

**Normal flow is hands-off:** merge to `master` → **Build** runs (tests, publish, migration bundle) → on
success, **Deploy** fires automatically: it migrates the shared DB once, then deploys the same build to every
site in the matrix. Nothing to click.

**Manual deploy** (redeploy the latest build without a new commit): Actions → **Deploy** → **Run workflow**. It
resolves the latest successful `master` Build and runs the identical migrate + deploy-both flow. There's no
per-site picker — it always does both; adjust the `deploy` matrix in `deploy.yml` if you ever need to scope it.

Under the hood, each site's deploy builds its own `appsettings.Production.json` (everything from Doppler, plus
any env `LOGGING_PATH`) and Web Deploys with `-enableRule:DoNotDeleteRule` (won't delete server files missing
from the publish — logs, uploads, `App_Data`) and `-enableRule:AppOffline` (drops `app_offline.htm` during the
copy so IIS releases the DLL lock, then removes it). The two sites deploy in parallel (`fail-fast: false`), so
one failing doesn't abort the other.

## 7. Troubleshooting

- **"No successful Build run found on master to deploy."** There's no green build to deploy — push to `master`,
  or run **Build** manually (`Run workflow`), then deploy.
- **Deploy can't find the artifact / it expired.** Build artifacts are kept 7 days (`retention-days` in
  `build.yml`). If you haven't deployed in over a week, run **Build** first to produce a fresh one.
- **`A duplicate key '…' was found`** on the site after deploy. A key is in *both* Doppler and a GitHub
  environment Variable. Remove it from one.
- **A list deployed as a single string.** For a **newline list** (`BlockPatterns` etc.), the key must
  be in `$stringListKeys` in `deploy.yml` and each entry on its own line. For a **JSON** value it must start with
  `[`/`{` and be valid JSON (backslashes doubled) — an invalid escape (e.g. a lone `\.`) fails to parse and
  silently falls back to a string.
- **`msdeploy` auth / certificate errors.** Check the `HOSTING_*` values against the site's current publish
  settings; the workflow already passes `-allowUntrusted` for the hosting provider's certificate.
- **`msdeploy`: "Unrecognized argument … All arguments must begin with -".** msdeploy re-parses the raw command
  line and expects the quoted `-dest:key="value",key2="value2"` provider syntax; PowerShell rewrites those
  embedded quotes and msdeploy rejects the result. That's why the Web Deploy step runs under **`shell: cmd`**,
  not pwsh — cmd passes the quotes through verbatim (pwsh fails this even via the `--%` stop-parsing token).
  Keep that step in `cmd`. One caveat: cmd expands `%…%`, so a **`HOSTING_PASSWORD` containing a literal `%`**
  would be misread — if a password ever contains `%`, rotate it or escape it as `%%` in the value.
- **Deploy didn't fire after a push to `master`.** Deploy triggers on `workflow_run` when **Build** *succeeds*
  on `master` — check Build went green. `workflow_run` also only fires for a `deploy.yml` that's on the default
  branch. A failed Build correctly skips deploy (the `setup` job's `if` guards on `workflow_run.conclusion`).
- **The `migrate` job failed.** The migration bundle threw — read its log. Because `deploy` `needs: migrate`,
  a failed migration **blocks the site deploys** (by design). Common causes: the migration itself errored
  against prod, or `CONNECTIONSTRINGS_DEFAULT` is missing/wrong in the Doppler `prd` config. Fix forward and
  re-run, or (if you added a reviewer) reject and investigate.
- **A site deployed but the schema looks stale, or vice-versa.** Migrate runs once *before* both deploys, so a
  green `migrate` + green `deploy` means both applied. If one site's `deploy` leg failed, only that site is
  behind — re-run Deploy (the same artifact ships) once the cause is fixed.
