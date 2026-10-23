# ProcureAccess — E2E and accessibility tests

Playwright tests with axe-core against the docker-compose E2E stack (`TESTING_v*.md` §3.3, §5.4, §7.1).

## Run

```bash
nvm use                                   # Node from .nvmrc
npm install
npx playwright install chromium firefox webkit   # once per Playwright version
npm run stack:up                          # needs ../.env like the dev stack; stop the dev stack first (same ports 4200/5000)
npx playwright test                       # all browser projects; e.g. --project=chromium
npm run report                            # HTML report of the last run (traces of failed tests)
npm run stack:down
```

`npm run stack:up` starts `docker-compose.yml` plus `docker-compose.e2e.yml`: a fresh in-memory database on every start, a stack-local network, SMTP credentials cleared, and `--wait` until client and API answer. If the stack is not reachable, the run stops at once (`support/global-setup.ts`).

Browser projects: `chromium`, `firefox`, `webkit`, `mobile-chrome` (Pixel 7), `mobile-safari` (iPhone 15). WebKit needs the Ubuntu/Debian libraries Playwright lists (`npx playwright install-deps`); on other Linux distributions run the Chromium and Firefox projects locally and leave WebKit to CI.

## Layout

| Path | Content |
|---|---|
| `support/axe.ts` | `expectNoAxeViolations(page, testInfo, state)`: call it after every significant state change. WCAG 2.0/2.1 A/AA blocks; WCAG 2.2 AA is only reported. Results are attached as `axe-<state>.json` |
| `support/axe-suppressions.ts` | The **only** place to suppress an axe violation: one rule on one selector, with an issue link and a reason. Reviewed at the end of every migration step; must be empty before the pilot |
| `support/axe-helper.spec.ts` | Self-test of the helper and the suppression rules |
| `smoke/` | Smoke test of the stack |
| `journeys/` | Journeys J1–J7 (from S1-19) |

Tests that cover an invariant carry `INV:<ID>` in the `describe` or `test` title (TESTING §8.1).
