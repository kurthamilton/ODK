# ODK.Data.EntityFramework.Migrations

EF Core migrations for `OdkContext`, kept in their own project so they can be run
independently of the web app (e.g. from a CI/CD pipeline). **Migrations never run on
app start** — they are applied explicitly via the tooling below.

## How the tooling finds the context

All commands use **`ODK.Web.Razor` as the startup project**. The EF tools boot the web
host, and `OdkContext` is resolved from its DI container — so the connection string comes
from the web app's normal configuration (appsettings / environment / user secrets) and is
never hard-coded here. `--project` points at this project because that's where the
migration files live.

`ODK.Web.Razor` references this project (so the migrations assembly is loadable) and the
`Microsoft.EntityFrameworkCore.Design` package (required on the startup project). The
`OdkContext.OnConfiguring` `MigrationsAssembly(...)` call points EF back here.

## Prerequisites

The `dotnet-ef` tool is pinned as a local tool. Restore it once per clone:

```
dotnet tool restore
```

## One-time baseline (existing databases)

Production, staging, and local dev databases already contain the full schema, so the
initial migration must be recorded as applied **without** running it. Run
[`Scripts/Baseline-MarkInitialCreateApplied.sql`](Scripts/Baseline-MarkInitialCreateApplied.sql)
once against each existing database. A brand-new / empty database skips this and gets the
schema built by applying `InitialCreate` normally.

## Naming convention

Migration names follow:

```
{TableName}[-{ColumnName}]-{Action}
```

- **TableName** (required) — the database table affected, e.g. `MemberSubscriptionLog` (the mapped
  table name, as it appears in the migration's `table:` argument — not the entity name).
- **ColumnName** (optional) — the column affected. Omit it when the migration acts on the table as a
  whole (e.g. creating a new table). When a migration adds a *group* of related columns, replace the
  column name with the feature those columns support (e.g. `GiftSubscriptions`).
- **Action** (required, PascalCase) — what the migration does. `Add` and `Remove` are unambiguous and
  can be used as-is. Every other change must be **specific** — describe the actual change so the name
  disambiguates it, since a table/column can be altered in many ways. Prefer `MakeRequired`,
  `MakeNullable`, `Rename`, `ChangeType`, `AddDefault`, `AddUniqueIndex` over a generic verb like
  `Alter` or `Update`, which don't say what changed (and won't stay unique across repeated tweaks to
  the same column).

Hyphens are preserved in the migration id and file name; EF strips them when forming the C# class
name (`MemberSubscriptionLog-InitiatorId-Add` → class `MemberSubscriptionLogInitiatorIdAdd`).

Where one migration makes the **same change to several tables** — a schema-wide sweep rather than a
feature — name it for the group of tables instead of listing them, e.g. `LookupTables-Id-Add`. Splitting
it per table would be a batch of migrations that must be applied together and could otherwise be applied
apart, which is worse than a name that names a set.

Examples:

| Change | Migration name |
|---|---|
| Add the `InitiatorId` column to `MemberSubscriptionLog` | `MemberSubscriptionLog-InitiatorId-Add` |
| Create a new `MemberReferrals` table | `MemberReferrals-Add` |
| Add several columns to `MemberSubscriptionLog` supporting gift subscriptions | `MemberSubscriptionLog-GiftSubscriptions-Add` |
| Drop the `LegacyToken` column from `Member` | `Member-LegacyToken-Remove` |
| Make `Member.EmailAddress` non-nullable | `Member-EmailAddress-MakeRequired` |

## Enum lookup tables

Some enums are mirrored by a database table so other tables can foreign key to them —
`SiteFeatureType` → `SiteFeatures`, referenced by `SiteSubscriptionFeatures.SiteFeatureId`.
These tables are **not in the EF model**, so nothing keeps them in step automatically: adding
an enum member without adding the matching row makes every insert of that value fail the
foreign key. That is exactly how `SiteFeatureType.Theme` failed in production.

`Enums/EnumTables.cs` is the registry of which enum maps to which table. Register a new one
there — an unregistered type throws rather than guessing a name.

`Enums/MigrationBuilderExtensions.cs` emits the SQL from a migration:

```csharp
migrationBuilder.CreateEnumTable<SiteFeatureType>();
migrationBuilder.InsertAllEnumValues<SiteFeatureType>();
migrationBuilder.InsertEnumValues(SiteFeatureType.Theme);
migrationBuilder.AddEnumForeignKey<SiteFeatureType>("SiteSubscriptionFeatures", "SiteFeatureId");

migrationBuilder.DeleteEnumValues(SiteFeatureType.Theme);
migrationBuilder.DropEnumTable<SiteFeatureType>();
```

Notes:

- The `Name` column holds the enum's `[Display(Name = "…")]` value, falling back to the member
  name where there is no attribute.
- Every statement is guarded (`IF OBJECT_ID … IS NULL`, `IF NOT EXISTS …`), because these tables
  already exist in databases restored from production but not in one built from the migrations
  alone. The same migration therefore has to be a no-op against the former.
- `InsertAllEnumValues` skips the zero value: `None` is the reserved unset sentinel, not a real
  value, so it is deliberately not a valid foreign key target. Pass it to `InsertEnumValues`
  explicitly if a column genuinely needs to store it.
- An existing row is left alone rather than having its name refreshed — renaming a value is a
  separate decision, and doing it implicitly would rewrite rows the migration never mentioned.
- There is no drop-foreign-key helper; `migrationBuilder.DropForeignKey(...)` already covers it.
- The SQL builders (`Enums/EnumTableSql.cs`) are pure functions and are covered by
  `ODK.Data.EntityFramework.Migrations.Tests`.

**Adding an enum member to a mirrored enum means adding a migration**, named for the table
(e.g. `SiteFeatures-Theme-Add`).

## Everyday commands

Run from the solution root.

Add a migration after changing the entity mappings (see the naming convention above):

```
dotnet dotnet-ef migrations add <Name> \
  --project ODK.Data.EntityFramework.Migrations \
  --startup-project ODK.Web.Razor
```

List migrations and their applied/pending status:

```
dotnet dotnet-ef migrations list \
  --project ODK.Data.EntityFramework.Migrations \
  --startup-project ODK.Web.Razor
```

Produce an idempotent SQL script (preferred for reviewing/deploying):

```
dotnet dotnet-ef migrations script --idempotent \
  --project ODK.Data.EntityFramework.Migrations \
  --startup-project ODK.Web.Razor \
  -o migrate.sql
```

Apply migrations directly to the configured database:

```
dotnet dotnet-ef database update \
  --project ODK.Data.EntityFramework.Migrations \
  --startup-project ODK.Web.Razor
```

`database update` connects using whatever connection string the web host resolves for the
current environment. Set `ASPNETCORE_ENVIRONMENT` (and provide the matching config / user
secrets) to target a specific database.

To migrate **production**, use `Scripts/ef-update-database-prod.bat` — it passes
`--environment Production`, so it reads the prod connection string from a local, gitignored
`ODK.Web.Razor/appsettings.Production.json`. That file only needs the connection string:

```json
{ "ConnectionStrings": { "Default": "<prod connection string>" } }
```

(Doppler is used for *deploy-time* secrets only — the app and this local script don't depend on
it. See `DEPLOYMENT.md`.)

Build a self-contained migration bundle (useful in CI/CD — no SDK needed on the target):

```
dotnet dotnet-ef migrations bundle \
  --project ODK.Data.EntityFramework.Migrations \
  --startup-project ODK.Web.Razor
# then, on the target:
./efbundle --connection "<connection string>"
```
