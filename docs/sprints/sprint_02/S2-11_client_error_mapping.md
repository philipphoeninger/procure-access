# S2-11 — Client mapping of Problem Details and field errors

> **Sprint:** 02, P0 task S2-11 (roadmap M1.3, 1 point planned, about 2 used; ARCHITECTURE §12 step 1). **Issues:** none; closes S2-10 open point P7. **Requirements / invariants:** SPEC_v2 §7.1, §8.6; Handoff §3.3, §6.4; UX copy v14 §6.1 (E11, E12); **API-02** → `active` (`TESTING_v19.md`, V80). **Status:** done by Claude Code (2026-11-02), pull request open.
> **Scope:** problem type → UX key; server field errors bound to the matching form control (`aria-describedby`) and announced; one existing form as the example; API-02. Not in scope: the page-title prefix `shell.pageTitle.error` (S2-16), `A11yAnnouncerService` (S2-16), the conflict dialog S37 (API-03, step 5), the error pages S80 (S2-17), rate limiting (API-05), the snackbar as such (UX copy: never for errors; replaced with the system states of S2-17 and later).
> **Owner decisions 2026-11-01:** **1** Handoff §6.4 / E11 rather than the task text: the error summary (`pa-notice` error with `focusOnShow`) takes the focus after a failed submit, and that is the announcement; no extra `A11yAnnouncerService` announcement (it would make screen readers read the errors twice). **2** The summary title uses the plural approach of O-S2-3, so S2-12 ran first. **3** Only problem types with a UX key are mapped; the others keep the generic text until their step; missing keys are listed (P1), no wording invented.
> **Owner decisions 2026-11-02:** **Q1** example form: the legacy product proposal, the only form that gets real server field errors today. **Q2** API-02 `active` with its FE part; the E2E part follows with the first rebuilt form, as for the deferred part of ARC-03 in S2-08. **Q3** the whole summary line is the link, because `errors.summary.item` („{{field}}: {{message}}“) is one string.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The client had no `pa-notice`, no error summary, no `A11yAnnouncerService`; the error interceptor shows a snackbar for every error and reads only the legacy `code`, which CRUD failures no longer send since S2-10 | `pa-notice` and `pa-error-summary` built; interceptor reads the problem type |
| F2 | SPEC §8.6 says the client maps the problem types "to the UX keys named in the feature areas", but only `email-unconfirmed` → `auth.confirm.required` is named (ARCHITECTURE §7.1, UX copy U72, Handoff §5.3). `conflict` → dialog S37, `not-found` → page S80, `rate-limit` → `errors.rateLimit` with `{{minutes}}` belong to screens of later steps; `antiforgery`, `locked-criteria-list`, `test-status-unknown`, `no-criteria-list`, `profile-mismatch`, `entry-withdrawn`, `selection-excluded`, `release-mismatch` have no key | Decision 3; P1 |
| F3 | No form got server field errors from the API, except the product proposal: `CreateProductDto` has `[Url]` and `[StringLength]`, so an invalid link returns `errors["product.link"]`. The identity endpoints still send their ad-hoc objects (S2-10), the update-user dialog only returns its value, the proposal store did not catch failures and left the loading counter up | Q1; store fixed with `finally` for upsert and review |
| F4 | The API sends English messages ("The Link field is not a valid fully-qualified http, https, or ftp URL.") and no error codes, so the client can only show them through the fallback `errors.field.server` = „{{message}}“ (UX copy §6.1) | P2 |
| F5 | `--pa-space-*` (Handoff §1.4) and `.cdk-visually-hidden` did not exist in the global styles | Added to `styles/main.scss`, needed by `pa-notice` |
| F6 | TESTING §9 retires the proposal tests in step 5 | The API-02 test moves to a rebuilt form before then (V80, P5) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `CLIENT/…/src/app/core/errors/problem-details.ts` (new) | `ProblemDetails`; `problemTypeName()` reads `<name>` from `…/problems/<name>` under any base URI; `PROBLEM_TYPE_KEYS` (`email-unconfirmed` → `auth.confirm.required`); `FORM_SHOWS_FIELD_ERRORS` (`HttpContextToken`); `applyServerErrors(error, resolveField)` sets the error `server` on the control of each known path, marks it touched and returns the summary lines; unknown paths and `$` → lines without a link | SPEC §8.6, API-02, decision 3 |
| `CLIENT/…/src/app/core/errors/error-summary-item.ts` (new) | `ErrorSummaryItem`: field error as key + params, optional field id and label key | Shared by form and summary; keeps `core` independent of `shared` |
| `CLIENT/…/src/app/shared/components/notice/` (new) | `pa-notice` per Handoff §3.3: `kind`, `title`, `live`, `focusOnShow`; icon, hidden prefix `common.notice.*`, role `note`/`status`/`alert`, title as `<h2>` and `tabindex="-1"` with `focusOnShow`, focus on show; colours and spacing from tokens; `@figma` tag (node `2032-486`); the native `title` attribute is removed (review R1) | Handoff §3.3, §6.4 |
| `CLIENT/…/src/app/shared/components/error-summary/` (new) | `pa-error-summary`: `pa-notice` error, title `errors.summary.title` with `translatePlural`, one line per error (`errors.summary.item`, or `errors.field.server` without a field); the whole line links to the field and moves the focus there; focus on the summary whenever a failed submit gives it new lines | Handoff §6.4, E11, E12, decision 1, Q3 |
| `CLIENT/…/src/app/core/interceptors/error.interceptor.ts` | Problem type → key before the legacy code; no snackbar for a `validation` problem of a request marked `FORM_SHOWS_FIELD_ERRORS` | Decision 3; no message twice |
| `CLIENT/…/src/app/core/models/error-translation.map.ts` | `CreateFailure`, `UpdateFailure`, `DeleteFailure`, `UICustomizationNotFound` removed | S2-10 P7 |
| `CLIENT/…/src/app/features/products/proposal/product-proposal.{ts,html}` | Submit awaits the store with `FORM_SHOWS_FIELD_ERRORS`; failure → `applyServerErrors` with the map `product.name/link/description` → field; `pa-error-summary` at the top of the form; inputs get ids; each field shows `errors.field.server` while it has the error `server` | Q1 |
| `CLIENT/…/src/app/features/proposal/{services/api/proposal-api.service.ts,state/with-proposal.ts}` | `HttpContext` passed through upsert and review; loading counter decremented in `finally` | F3 |
| `CLIENT/…/public/i18n/de.json` | `common.notice.info/attention/error`, `auth.confirm.required`, `errors.summary.item`, `errors.field.server`, verbatim from UX copy v14 | UI strings only by key |
| `CLIENT/…/src/styles/main.scss` | `--pa-space-*` (Handoff §1.4) and `@include cdk.a11y-visually-hidden()` | F5 |
| Specs (new) | `product-proposal.spec.ts`: **`Submit_ServerFieldErrors_BoundToFieldsAndSummaryFocused INV:API-02`**, field error goes on edit and the summary takes the focus again on the next failed submit, no summary after success; `problem-details.spec.ts`; `error.interceptor.spec.ts`; `notice.spec.ts` | API-02 (FE) |
| `docs/TESTING_v19.md` (new) | Copy of v18: status line; V80; §4.6 API-02 `active`; §9 step 3 (E2E part) and step 5 (test moves before the proposal tests go) | Prompt: flip in the next TESTING version |
| `docs/figma_code_mapping_procureaccess_v1.md` | v1.12: `pa-notice` built | Figma mapping |
| `CLAUDE.md` | Header; §3 (client error mapping as built); §4 (`core/errors/`, `notice/`, `error-summary/`); §9 rule "Server field errors" | DoD |

**Deviation from the plan:** the plan named a helper that turns `items[0].note` into the control path `items.0.note`. It is not built: the form passes a resolver from server path to field, which covers arrays when a form needs them, and no form has one yet.

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review`. Security: server messages go through Angular interpolation (escaped); link targets are ids from code; no new dependency. Correctness: non-validation failures leave the summary empty and keep the interceptor's snackbar; the criterion proposal still calls the store without the marker, so it keeps its snackbar. a11y: verified in the tests (role, name, focus, `aria-invalid`, `aria-describedby`). Mutations checked by hand: without the summary's focus effect the second-submit test fails; without the server `mat-error` the API-02 test fails.

| # | Finding | Sorting |
|---|---|---|
| R1 | A static `title="…"` on `pa-notice` stays a native attribute next to the input (confirmed in a test): a tooltip that screen readers read a second time | **Fixed**: host binding `[attr.title]` = `null`; `notice.spec.ts` checks it |
| R2 | `PROBLEM_TYPE_KEYS[name]` would also find inherited object members for a type named e.g. `constructor` | Won't fix: types come from the API's 13 names (`ProblemTypes`), the pattern allows only `[a-z-]` |
| R3 | On first show the notice focuses itself and the summary focuses it again | Won't fix: same element, no second announcement |
| R4 | The 4 px accent border of `pa-notice` uses `--pa-space-xxs` | Won't fix: Handoff has no border-width token; value matches |
| R5 | `approveProposal` still leaves the loading counter up on failure | Follow-up: legacy, step 5 retires the proposal (P6) |

## 4. Checks

| Check | Result |
|---|---|
| `npx ng test` (Vitest) | 44 tests in 10 files passed (20 new) |
| `npm run lint` | All files pass |
| `npm run build:release` | Passes; initial total 1.75 MB raw / 359 KB transfer; the existing critical-CSS warning is unchanged |
| E2E, Chromium (`npm run stack:up`, `npx playwright test --project=chromium`) | 15 passed, incl. J1 with axe (run before R1, which touches only `pa-notice`); the other browsers run in CI |
| `node scripts/check-invariants.mjs` | Passed; 13 active, API-02 found |
| `node scripts/check-figma-links.mjs` | Passed; 1 checked (`pa-notice`), 11 not built yet, 1 pending |
| `reuse lint` | Compliant |
| Backend tests | Not run: no backend change |

## 5. Open points

| # | Point | Where |
|---|---|---|
| P1 | UX keys for the problem types without one: `antiforgery`, `locked-criteria-list`, `test-status-unknown` (as a non-field message; the field case is `publish.error.testStatus`), `no-criteria-list`, `profile-mismatch`, `entry-withdrawn`, `selection-excluded`, `release-mismatch` (probably `criteria.export.outdated`, but not named). SPEC §8.6 should name the key per type in its next version | Owner (UX copy, SPEC), before the step of each type |
| P2 | The API sends English validation messages without codes; the client shows them through `errors.field.server`. German messages (or codes the client maps to `errors.field.*`) come with the module validation of each step | Each migration step's validation |
| P3 | The page title prefix `shell.pageTitle.error` after a failed submit needs the `TitleStrategy` | S2-16 |
| P4 | API-02 E2E part with the first rebuilt form the API validates (step 3 at the latest; needs a signed-in E2E flow) | TESTING_v19 V80 |
| P5 | The tagged API-02 test sits on the legacy proposal form; it moves to a rebuilt form before step 5 retires the proposal tests | Step 3–5 |
| P6 | `approveProposal` leaves the loading counter up on failure (R5) | Step 5 (retired) |
| P7 | `de.json` now has lowercase roots (`common`, `auth`, `errors`) next to the legacy uppercase ones (`COMMON`, `AUTH`, `ERROR`) | S2-13 |
