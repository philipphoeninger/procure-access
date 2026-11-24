# ProcureAccess

ProcureAccess helps public bodies and companies in Germany, Austria and Switzerland **procure accessible software**. It is developed as a module of the publicly funded research project **WERTE.IT** and published as open source, so that it can be inspected, reused and run by others.

The user interface is in German.

## What it does

1. **Kriterien-Konfigurator** — choose the use case, the kinds of application, the product parts and optional test procedures, and get a prioritised list of accessibility criteria in plain language, based on the BIT inklusiv texts and EN 301 549 / BITV 2.0. Adjust the list and export it as XLSX or CSV for the tender documents. Works without an account.
2. **Workspace** — organisations run procurement projects: a project's own criteria list, candidate products with their test status and evidence, and an assessment per criterion. The tool never computes an overall score for a product: what a product fulfils is shown criterion by criterion.
3. **Product directory** *(planned)* — published statements about products, kept in two strictly separate channels: statements of the vendor side, and independent statements of organisations and of people with disabilities.

The tool must itself meet WCAG 2.1 AA / EN 301 549 / BITV 2.0, on small screens and at high zoom, in light and dark appearance.

## Status

**In development, not yet in production use.** The criteria configurator with export, accounts and organisations, saved and shared criteria lists, and the workspace with projects, candidates and assessments are built. The product directory, publication and moderation follow. A pilot with cooperation partners comes before the first public release.

The first real catalog release (`2026.1`) is still waiting for the project team's priorities; until it is active, local setups run with a small test catalog (`T.1`).

## Getting started

You need Docker with Compose v2. Then, from the repository root:

```bash
cp .env.example .env                      # set DB_PASSWORD (letters and digits) and SAMPLE_USER_PASSWORD (12+ characters)
docker network create dokploy-network     # once; the compose file expects this network
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build
```

- Application: http://localhost:4200 — sign in as `sample.user@example.org` with your `SAMPLE_USER_PASSWORD`
- E-mails the application sends (Mailpit): http://localhost:8025
- API documentation (Swagger): http://localhost:5000/swagger

The development stack reseeds its sample data at every start. Running the API and client from source, the tests and every check: [`docs/TECHNICAL.md`](docs/TECHNICAL.md) §2.

## Technology

ASP.NET Core 10 (C#, EF Core) as a modular monolith · PostgreSQL 17 · Angular 22 with Angular Material · xUnit, Testcontainers, Vitest, Playwright with axe-core · Docker Compose · GitHub Actions with CodeQL and a REUSE licence check.

## Documentation

| For | Read |
|---|---|
| Running, configuring, deploying, backing up and restoring; usage indicators | [`docs/TECHNICAL.md`](docs/TECHNICAL.md) |
| How the code is built and why | `docs/ARCHITECTURE_v*.md` |
| What the tool must do (requirements, acceptance criteria) | `docs/SPEC_v*.md` |
| What the tests guarantee | `docs/TESTING_v*.md` |
| What exists in the code, area by area, and every command | `docs/CODEBASE_v*.md` |
| Rules for changing the code (for people and for AI assistants) | [`CLAUDE.md`](CLAUDE.md) |
| Product scope and UI texts (German) | `docs/prd_procureaccess_werteit_v*.md`, `docs/ux_copy_procureaccess_v*.md` |

Documents carry their version in the file name; the highest version is the current one. The project is developed with AI assistance; [`docs/ai_development_process.md`](docs/ai_development_process.md) describes the process.

## Contributing

Bug reports and questions go to the GitHub issues of this repository. Before changing code, read [`CLAUDE.md`](CLAUDE.md) (rules that hold for every change) and [`docs/TECHNICAL.md`](docs/TECHNICAL.md) §2 (setup and checks). Every pull request runs the full CI pipeline: build, unit, integration and end-to-end tests with accessibility checks, lint, licence and i18n checks.

## Licence

Copyright (c) 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.

Source code and documentation are licensed under the MIT licence, see [`LICENSE`](LICENSE).

Exceptions (see [`REUSE.toml`](REUSE.toml)):

- The BIT inklusiv texts are not MIT-licensed: all rights reserved, see [`LICENSES/LicenseRef-AllRightsReserved.txt`](LICENSES/LicenseRef-AllRightsReserved.txt). They appear in the criteria catalog documents (`docs/kriterienkatalog_bit_inklusiv_v1*.md`), the criteria seed data (`DAL/MODELS.ProcureAccess/SeedData/CriteronSeed.cs`) and the EF Core migrations that carry it, and in the catalog release data (`catalog/releases/`). Detlef Girke has given permission to use them in ProcureAccess, its deployments and its exports. The project team's own sections of the criteria catalog documents are MIT.
- The WERTE.IT logo (`CLIENT/CLIENT.ProcureAccess/public/werte_it_logo.jpg`, rights holder Blinden- und Sehbehindertenverein Hamburg e.V.) is not MIT-licensed: all rights reserved, see [`LICENSES/LicenseRef-AllRightsReserved.txt`](LICENSES/LicenseRef-AllRightsReserved.txt). `favicon.ico` is the Angular CLI default icon (MIT, Google LLC).

Forks are welcome. The MIT parts may be used freely; using the BIT inklusiv texts or the WERTE.IT logo in a fork needs the rights holder's own permission.

Licence information follows the [REUSE specification](https://reuse.software); check it with `reuse lint`.
