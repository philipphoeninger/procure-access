# S1-05 — Migrations check on .NET 10 / EF Core 10 / PostgreSQL 17

> **Sprint:** 01, task S1-05 (milestone M0.1). **Status:** done (2026-10-20). **Prepares:** OPS-03 (tagged test in S1-14).
> **Scope:** manual check only — no migration, snapshot or code changed.

## 1. Setup

| Item | Version |
|---|---|
| .NET SDK | 10.0.113 |
| `dotnet-ef` (global tool) | 10.0.13 |
| EF Core / Npgsql EF provider | 10.0.13 / 10.0.3 |
| Database | `postgres:17-alpine` (PostgreSQL 17.11), throwaway container on port 55432 with a random password; removed after the check. The compose `db` volume was not used. |
| Migrations | `20260526145658_InitialCreate`, `20260602133914_AddSeedData`, `20260603144409_UpdateAssociationEntitiesNotUsingBase` (all generated with EF Core 8.0.21) |

All commands ran from `API/API.ProcureAccess` with `ConnectionStrings__ProcureAccess` pointing at the empty server (database `ProcureAccessDb` did not exist yet) and `--project ../../DAL/DAL.ProcureAccess --startup-project .`.

## 2. Results

| # | Check | Result |
|---|---|---|
| 1 | `dotnet ef migrations list` before | all 3 migrations `(Pending)` |
| 2 | `dotnet ef database update` | database created, all 3 migrations applied, `Done.`, exit 0 |
| 3 | `dotnet ef migrations list` after | all 3 listed, none pending |
| 4 | `__EFMigrationsHistory` | 3 rows, `ProductVersion` 10.0.13 |
| 5 | Seed data (`HasData`) | Criteria 127, CriteriaFilters 27, FilterTypes 4, Products 17, CriteriaFilterExclusions 9, ProductCriteriaFilters 74; 17 tables in total (incl. history) |
| 6 | `dotnet ef database update` again | nothing to apply, exit 0 |
| 7 | `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration.", exit 0 |

**Both "Done when" checks pass.**

## 3. Notes

- **Expected noise:** before the database exists, EF logs "An error occurred using the connection to database 'ProcureAccessDb'" while probing for it; it then creates the database. Not an error.
- **Migration lock:** EF Core 10 logs "Acquiring an exclusive lock for migration application" during `database update`. This is the migration lock introduced in EF Core 9, not a hang.
- **`ProductVersion`:** migration designers and the model snapshot still say `8.0.21`. No new migration is needed for the upgrade; the annotation changes with the next real migration (S1-03 §3, item 12).
- **Startup path unchanged:** `Program.cs` still swallows exceptions from `Migrate()` (S1-03 §3, item 1). Since the model has no pending changes, `Migrate()` does not throw on EF Core 10 today. OPS-03 (S1-14) makes this a regression test, so model drift is caught in CI instead of at startup.
