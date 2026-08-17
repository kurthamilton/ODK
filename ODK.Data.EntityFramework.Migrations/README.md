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

**Bringing an older lookup table into the registry.** Tables built before it exists key themselves on their
own name (`SubscriptionTypes.SubscriptionTypeId`), while everything here expects `Id`. Rename it, then run
the same statements the registry emits for a table it manages — the rename brings a production restore into
line, and the guarded create/insert/foreign key build the table in a database that only ever had the
migrations:

```csharp
migrationBuilder.RenameEnumIdColumn<SubscriptionType>("SubscriptionTypeId");
migrationBuilder.CreateEnumTable<SubscriptionType>();
migrationBuilder.InsertAllEnumValues<SubscriptionType>();
migrationBuilder.AddEnumForeignKey<SubscriptionType>("MemberSubscriptionLog", "SubscriptionTypeId");
```

The rename needs no foreign keys dropped around it: SQL Server binds a foreign key to a column by id, not
by name, so the ones pointing at the renamed column follow it. And only the lookup table's own key column
is renamed — a referencing column like `MemberSubscriptionLog.SubscriptionTypeId` keeps its name, which
states a relationship that really is there.

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

## Constraints and indexes created outside EF

The scaffolder only ever emits names it would have chosen itself, and it only knows what the model tells it.
Both of those are wrong often enough here to be worth planning around: much of this schema pre-dates EF.

**Names are guesses.** Plenty of foreign keys are named nothing like EF's convention
(`FK_Payments_Currencies`, not `FK_Payments_Currencies_CurrencyId`), so **a scaffolded `DropForeignKey` fails
against any constraint created by hand**, and takes the whole migration with it. Drop by the column instead,
which drops whatever is actually there:

```csharp
migrationBuilder.DropForeignKeys("Chapters", "CountryId");
```

EF then adds its own back under the conventional name, so a table converges on the convention as it is
migrated.

**A mapped key is not a constraint.** The same applies to primary keys, which the scaffolder also names by
convention — and it additionally assumes there is one to drop. `SentEmailEvents` was mapped with `HasKey` for
years while the table had no primary key at all, and nothing said so until a migration tried to drop it. Look
it up instead, which handles both the unexpected name and the missing constraint:

```csharp
migrationBuilder.DropPrimaryKeyIfExists("SentEmailEvents");
```

`DropConstraintIfExists` covers a **unique** constraint whose name you do know.

**Nor is an unmapped column or table.** `DropColumn` and `DropTable` assume the thing exists, which holds for
anything a migration created and fails for anything that only ever existed in a restored database — the
schema pre-dates the baseline, so a database built from the migrations alone never had it. Use
`DropColumnIfExists` and `DropTableIfExists`, which are no-ops there. `DropColumnIfExists` also clears the
column's default constraint, looked up rather than named, since a default blocks the drop and `DropColumn` only
handles it for a column EF knows about:

```csharp
migrationBuilder.DropColumnIfExists("Payments", "PaymentReconciliationId");
migrationBuilder.DropTableIfExists("PaymentReconciliations");
```

Dropping a column means clearing everything that depends on it first — a foreign key, and **any** index, not
just the kind `DropIndexes` removes. That helper is scoped to an index duplicating one EF is about to create
and passes over a unique or clustered index, so use `DropIndexIfExists` with the name for this:

```csharp
migrationBuilder.DropForeignKeys("Payments", "PaymentReconciliationId");
migrationBuilder.DropIndexIfExists("Payments", "IX_Payments_PaymentReconciliationId");
migrationBuilder.DropColumnIfExists("Payments", "PaymentReconciliationId");
```

**Adding a relationship the database already has scaffolds only the additions.** EF has no idea the
constraint and its index are already there, so it emits `AddForeignKey` and `CreateIndex` with nothing to
drop. The foreign key collides; the index either collides or quietly leaves a second index on the column.
Drop both first:

```csharp
migrationBuilder.DropForeignKeys("Events", "VenueId");
migrationBuilder.DropIndexes("Events", "VenueId");
```

`DropIndexes` is deliberately narrower than `DropForeignKeys`: it removes only an index that duplicates what
EF is about to create — non-primary-key, non-unique, nonclustered, keyed on that one column alone. A
composite index beginning with the column serves queries the migration knows nothing about, a unique index is
a constraint rather than a lookup aid, and dropping a clustered one would rewrite the table.

`ColumnSql`, `ForeignKeySql`, `IndexSql` and `PrimaryKeySql` build the SQL and are covered by
`ODK.Data.EntityFramework.Migrations.Tests`; `EnumTableSql.DropForeignKey` does the same job for the enum
lookup tables. All of them name their SQL variables after the table and column they act on, because a
migration emitting several blocks runs them in one batch and variables are scoped to the batch, not the
block — and all of them build the statement into a variable before executing it, because `EXEC` takes string
literals and variables joined by `+` and nothing else, so `EXEC(N'…' + QUOTENAME(@n))` is a syntax error.

**Find them before writing the migration** rather than one failed run at a time. For the tables a migration
touches:

```sql
SELECT fk.name, OBJECT_NAME(fk.parent_object_id) AS from_table, OBJECT_NAME(fk.referenced_object_id) AS to_table
FROM sys.foreign_keys fk
WHERE OBJECT_NAME(fk.referenced_object_id) IN (<tables>);
```

Anything that query returns which the model does not declare is a relationship to add to the mapping — and a
pair of drops to write by hand.

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
