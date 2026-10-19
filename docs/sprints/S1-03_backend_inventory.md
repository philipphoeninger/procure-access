# S1-03 — Backend inventory and upgrade notes (.NET 8 → 10)

> **Sprint:** 01, task S1-03 (milestone M0.1). **Status:** done (2026-10-19). **Input for:** S1-04 – S1-06.
> **Scope:** inventory only — no code changed. Target versions are the latest stable releases on nuget.org as of **2026-10-19**; re-check them when S1-04 starts.

## 1. Target frameworks and tooling

| Item | Current | Target | Note |
|---|---|---|---|
| `API.ProcureAccess` | `net8.0` (Web SDK) | `net10.0` | |
| `SERVICES.ProcureAccess` | `net8.0` (+ `FrameworkReference Microsoft.AspNetCore.App`) | `net10.0` | |
| `DAL.ProcureAccess` | `net8.0` | `net10.0` | Migrations live here; snapshot `ProductVersion` 8.0.21 |
| `MODELS.ProcureAccess` | `net8.0` | `net10.0` | |
| API Dockerfile | `sdk:8.0` / `aspnet:8.0` | `sdk:10.0` / `aspnet:10.0` | S1-06. Publishes with `-c Debug` (existing TODO in the file) |
| Local SDK (developer machine) | 8.0.131 only | .NET 10 SDK | **Install before S1-04** |
| Solutions | `ProcureAccess.sln` + leftover `DAL/DAL.ProcureAccess.sln`, `SERVICES/SERVICES.ProcureAccess.sln` | root solution only | Clean-up belongs to S1-12 |
| `global.json`, central package management | none | – | Not required by the sprint; no change proposed |

## 2. NuGet packages

Licences come from each package's nuspec on nuget.org. ✅ = meets CLAUDE §10 (OSI licence, no commercial-use restriction).

| Package | Used in | Current | Target | Licence | Note |
|---|---|---|---|---|---|
| Asp.Versioning.Mvc | API | 8.1.0 | 10.2.1 | MIT ✅ | |
| Asp.Versioning.Mvc.ApiExplorer | API | 8.1.0 | 10.2.1 | MIT ✅ | |
| **AutoMapper** | API, SERVICES, DAL, MODELS | **15.0.1** | **14.0.0 (interim), then replace** | **RPL-1.5 or commercial ❌** | See §4 and issue #31 |
| Microsoft.AspNetCore.Authentication.JwtBearer | API, SERVICES | 8.0.21 | 10.0.13 | MIT ✅ | Stays until cookie auth in M3 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | all four | 8.0.21 | 10.0.13 | MIT ✅ | |
| Microsoft.AspNetCore.Identity.UI | SERVICES | 8.0.25 | 10.0.13 | MIT ✅ | Only used for the `IEmailSender` interface (`GlobalUsings.cs`); replace with Identity's `IEmailSender<TUser>` later (M3), not in S1-04 |
| Microsoft.AspNetCore.OpenApi | API | 8.0.20 | 10.0.13 | MIT ✅ | **No usage found** (only a commented-out `WithOpenApi`); removal candidate |
| Microsoft.EntityFrameworkCore | API, DAL, MODELS | 8.0.21 | 10.0.13 | MIT ✅ | |
| Microsoft.EntityFrameworkCore.Design | API, DAL | 8.0.21 | 10.0.13 | MIT ✅ | |
| Microsoft.EntityFrameworkCore.Tools | API | 8.0.21 | 10.0.13 | MIT ✅ | |
| Microsoft.Extensions.Hosting.Abstractions | SERVICES | 8.0.1 | – (remove) | MIT ✅ | Provided by the `Microsoft.AspNetCore.App` framework reference → NU1510 on .NET 10 |
| Microsoft.Extensions.Options | SERVICES | 8.0.2 | – (remove) | MIT ✅ | Same as above |
| Microsoft.IdentityModel.JsonWebTokens | API | 8.14.0 | 8.23.0 | MIT ✅ | No 10.x line; 8.x is current |
| Microsoft.IdentityModel.Tokens | API | 8.14.0 | 8.23.0 | MIT ✅ | |
| Microsoft.VisualStudio.Threading.Analyzers | API, SERVICES | 17.14.15 | 18.12.1 | MIT ✅ | Analyzer only; may raise new warnings |
| Npgsql.EntityFrameworkCore.PostgreSQL | API, DAL, MODELS | 8.0.11 | 10.0.3 | PostgreSQL ✅ | |
| Swashbuckle.AspNetCore | API | 6.8.1 | 10.3.0 | MIT ✅ | Meta-package; already contains the four below |
| Swashbuckle.AspNetCore.Annotations | API | 6.8.1 | 10.3.0 | MIT ✅ | `[SwaggerResponse]` in controllers |
| Swashbuckle.AspNetCore.Swagger | API | 6.8.1 | 10.3.0 (or drop) | MIT ✅ | Redundant next to the meta-package |
| Swashbuckle.AspNetCore.SwaggerGen | API | 6.8.1 | 10.3.0 (or drop) | MIT ✅ | Redundant |
| Swashbuckle.AspNetCore.SwaggerUI | API | 6.8.1 | 10.3.0 (or drop) | MIT ✅ | Redundant |
| MailKit | SERVICES | 4.15.1 | 4.18.1 | MIT ✅ | |
| Serilog.AspNetCore | SERVICES | 8.0.3 | 10.0.0 | Apache-2.0 ✅ | |
| Serilog.Enrichers.Environment | SERVICES | 3.0.1 | 3.0.1 | Apache-2.0 ✅ | Already current |
| Serilog.Settings.Configuration | SERVICES | 8.0.4 | 10.0.1 | Apache-2.0 ✅ | |
| Serilog.Sinks.Console | SERVICES | 6.0.0 | 6.1.1 | Apache-2.0 ✅ | |
| Serilog.Sinks.File | SERVICES | 7.0.0 | 7.0.0 | Apache-2.0 ✅ | Already current |
| Serilog.Sinks.MSSqlServer | SERVICES | 8.2.2 | – (remove) | Apache-2.0 ✅ | **Unused** (only commented-out settings); pulls in Microsoft.Data.SqlClient although the database is PostgreSQL |
| System.Text.Json | API, SERVICES, MODELS | 8.0.6 | – (remove) | MIT ✅ | Part of the .NET 10 runtime → NU1510 |

All packages except AutoMapper meet the licence rule.

## 3. Breaking changes that touch this code (.NET 9 and 10)

Only items that apply to the current code are listed. Everything else on Microsoft's lists (SQL Server, SQLite, Cosmos DB, WinForms/WPF, multi-targeting, enum mapping via `MapEnum`, array-column `Contains`) was checked and does not apply.

| # | Area | Change | Where it hits us | What to do (S1-04 / S1-05 / S1-06) |
|---|---|---|---|---|
| 1 | EF Core 9 (high) | `Migrate()` **throws** if the model has pending changes (`PendingModelChangesWarning` is now an error) | `Program.cs`: `db.Database.Migrate()` inside `try/catch` that only logs. A model drift would let the API start **without** migrations applied | S1-05 checks `has-pending-model-changes`. Note for S1-04: the swallowed exception hides this; do not change the behaviour in M0 but keep it in mind when testing (OPS-03) |
| 2 | EF Core 9 (low) | All pending migrations are applied in **one transaction**; `Migrate()` in an explicit transaction throws | No explicit transactions found; 4 migrations | None expected; verify in S1-05 on an empty PostgreSQL 17 |
| 3 | EF Core 10 (low) | Parameterized collections (`list.Contains(x.Id)`) are translated as multiple parameters; SQL parameter names simplified | `CriterionService` (`criteriaFilterIds.Contains`), `UserService` | Behaviour the same; matters for CAT-10 only if tests compare SQL text (they should not) |
| 4 | Npgsql EF 9/10 | Enum mappings must be configured at the EF level; array `Contains` now `= ANY(...)` | Enums (`ProposalStatus`, `Language`) are stored as integers, no PostgreSQL enums, no array columns | None |
| 5 | Swashbuckle 10 / Microsoft.OpenApi 2.x | Object model rewritten: `OpenApiReference` removed (use `OpenApiSecuritySchemeReference`), `AddSecurityRequirement` takes a `Func<OpenApiDocument, …>`, `Microsoft.OpenApi.Any` / `OpenApiAnyFactory` removed (use `JsonNode`), namespace `Microsoft.OpenApi.Models` → `Microsoft.OpenApi` | `Swagger/SwaggerConfiguration.cs` (Bearer scheme + requirement), `Swagger/SwaggerDefaultValues.cs` (`OpenApiAnyFactory.CreateFromJson`), `GlobalUsings.cs` | **Compile errors expected** in S1-04; rewrite these two files. Alternative: stay on Swashbuckle 9.x (Microsoft.OpenApi 1.x) for M0 — decide in S1-04 |
| 6 | .NET 10 SDK | **NU1510**: direct references to packages that the framework already provides are pruned and warned about | `System.Text.Json`, `Microsoft.Extensions.Options`, `Microsoft.Extensions.Hosting.Abstractions` | Remove the references (S1-04 requires no unexplained warnings) |
| 7 | .NET 10 SDK | `dotnet restore` **audits transitive packages** by default | AutoMapper (any version, see §4) will raise NU1902/NU1903 | Suppress with `NuGetAuditSuppress` + link to issue #31 |
| 8 | ASP.NET Core 10 | Cookie authentication no longer redirects for API endpoints (returns 401/403) | Not yet: the app still uses JWT | Relevant for M3, not M0 |
| 9 | ASP.NET Core 10 | `WithOpenApi()` is obsolete | Only in commented-out code (`Program.cs`) | None |
| 10 | Containers (.NET 10) | Default `10.0` image tags are **Ubuntu 24.04** instead of Debian | API Dockerfile | S1-06: check the image builds and runs; no Debian-specific commands are in the Dockerfile |
| 11 | .NET 10 Extensions | `null` values in configuration are preserved instead of becoming empty strings | Config binding of options classes | Watch in the S1-06 smoke test; no specific risk found |
| 12 | Migrations | `ProductVersion` annotation in the model snapshot changes to 10.x on the next migration | `ApplicationDBContextModelSnapshot.cs` | Expected noise; no new migration needed for the upgrade itself (S1-05 confirms) |

Also noticed, out of scope for M0: `DateTime.Now`/`UtcNow` in 5 files (CLAUDE §7 requires `TimeProvider`, S1-13 registers it); `RebuildDatabase` is not guarded by environment (SEC-06, migration step 1).

## 4. AutoMapper licence check (sprint risk R3)

**Finding.** The installed **AutoMapper 15.0.1** ships a `LICENSE.md` that offers two options: the **Reciprocal Public License 1.5 (RPL-1.5)** or a **commercial Lucky Penny Software licence**. RPL-1.5 is OSI-approved, but it is strongly reciprocal: it reaches the source of software built with the library, including deployed use. That conflicts with an MIT release and with the intent of CLAUDE §10 (and ARCHITECTURE D12, TESTING §3, which excluded EPPlus and FluentAssertions v8 for the same reason). **15.0.1 does not meet the licence rule.**

**Security.** CVE-2026-32933 / GHSA-rvv3-g6hj-g44x (High, CVSS 7.5): mapping deeply nested object graphs can cause a stack overflow that ends the process. Affected: **all versions below 15.1.1**, including 15.0.1 and **14.0.0** (the last MIT version). Fixed versions 15.1.1 / 16.1.1 are RPL/commercial only, and the maintainer does not plan a 14.x patch.

| Option | Licence | Advisory | Assessment |
|---|---|---|---|
| Keep 15.0.1 pinned | RPL-1.5 / commercial ❌ | affected | Worst of both |
| Update to 16.x / 15.1.x | RPL-1.5 / commercial ❌ | fixed | Licence rule broken |
| Pin 14.0.0 | MIT ✅ | affected, no fix | Acceptable **interim**: exposure is low — incoming JSON is capped by System.Text.Json's default `MaxDepth` (64) and no attacker-controlled deep graphs are mapped |
| Replace (Mapperly, Apache-2.0, or hand-written mapping) | ✅ | n/a | Long-term fix; small effort (1 profile, 10 `CreateMap`, `IMapper` in 9 files, `ProjectTo` in 5 places) |

**Recommendation: replace** — tracked in **issue #31**.
- **S1-04:** pin AutoMapper **14.0.0** in all four projects (a downgrade from 15.0.1; check the `AddAutoMapper(cfg => {}, …)` overload still compiles), suppress the advisory with `NuGetAuditSuppress` and a comment linking #31. ARC-06 (S1-14) tests this interim setup.
- **M1:** replace AutoMapper after an ADR (new dependency, CLAUDE §11), and decide in TESTING what replaces ARC-06.
- **Owner decision (2026-10-19): accepted.** Interim pin to 14.0.0 with the suppressed advisory in S1-04; AutoMapper is replaced in M1.

## 5. Sources

- AutoMapper 15.0.1 `LICENSE.md` (NuGet package); AutoMapper 14.0.0 nuspec: `MIT`
- [GHSA-rvv3-g6hj-g44x / CVE-2026-32933](https://advisories.gitlab.com/nuget/automapper/GHSA-rvv3-g6hj-g44x/)
- [Ed-Fi advisory on AutoMapper (2026-03-31)](https://docs.ed-fi.org/reference/roadmap/advisories/automapper-2026-03-31)
- [Breaking changes in EF Core 9](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes), [EF Core 10](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes)
- [Npgsql EF 9.0 release notes](https://www.npgsql.org/efcore/release-notes/9.0.html), [10.0 release notes](https://www.npgsql.org/efcore/release-notes/10.0.html)
- [Breaking changes in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- nuget.org flat-container API for versions and nuspec licences (queried 2026-10-19)
