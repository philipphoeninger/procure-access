# ProcureAccess — Figma ↔ Code Mapping (step 7h)

> **Status:** v1.11 — output of Phase 2, step 7h ("Connect Figma components to code") of `ai_development_process.md`; updated 2026-10-06 for the BIT inklusiv rating scale (K1, K6, entry row) and the new S10/S34a frames; `pa-rating-legend` and S34 Überblick added; critique 2: K5 `Show why button`, K1 `Scope=Workspace`, S10-M; 2026-10-12: Layout=Narrow|Wide for `pa-entry-card` and K3, K6 compact form, mobile-first frames; page tags mobile-first (360 px frame first), references to TESTING_v3; v1.6 (2026-10-12, consistency audit): K1 Scope adds `workspace`, K6 `kind` (procedure criteria, Ja/Nein) and allowed-ratings union, entry context on `pa-entry-card`, references to ARCHITECTURE_v6 / TESTING_v4 / UX copy v8, Angular 22; v1.7: procedure criteria derived (no K6 kind), references to ARCHITECTURE_v7 / TESTING_v5 / UX copy v9, `pa-product-result` property "Show use case line"; v1.8 (2026-10-13): references to ARCHITECTURE_v8 / TESTING_v6 / UX copy v10; new screen frames S34a-L, S55-1, S55-2 (see handoff §10); v1.9: references to ARCHITECTURE_v9 / TESTING_v7 / UX copy v11; S32 and S32-M are pages without new components; v1.10 (2026-10-13, review of the main documents `pruefung_hauptdokumente_v1.md`): K2 and K4 take a list of test evidences (`testEvidences[]`); component counts corrected (14 Figma components, 13 files in the CI map, because both header components are one Angular class); the CI script lives at `scripts/check-figma-links.mjs` in the repository root; references to ARCHITECTURE_v10 / TESTING_v8 / UX copy v12 / handoff 1.12. v1.11 (2026-10-29, sprint 01, `TECH_DEBT_v1.md` TD-11): §4 no longer copies the script; it describes the script that exists since task S1-21 (exact node match, `PENDING` list) and points to `scripts/check-figma-links.mjs` as the source.
> **Figma file:** "ProcureAccess" (`rPeSyK8NNe3d8AnUEXW9mt`), page "Komponenten"
> **Repository:** https://github.com/philipphoeninger/procure-access (branch `main`), Angular app in `CLIENT/CLIENT.ProcureAccess/`
> **Related:** `design_handoff_procureaccess_v1.md` (1.12; component interfaces §3–§4), `ux_copy_procureaccess_v12.md` (strings), `ARCHITECTURE_v10.md`, `TESTING_v8.md`

---

## 1. Why this is not Figma Code Connect (yet)

Step 7h names Figma Code Connect as the tool. The Figma account on this file is on the **Professional plan**, and Code Connect needs an **Organization or Enterprise plan** with the components published to a team library. The Figma MCP tools refuse every Code Connect call on this plan. In addition, the domain components do not exist in the code yet. They are built during the migration steps (ARCHITECTURE_v10 §12) on Angular 22.

Decision (2026-10-02): build the **two-way bridge without Code Connect**, with what the Professional plan offers:

| Direction | Mechanism | Where |
|---|---|---|
| Figma → Code | **Documentation link** on each component (Dev Mode "Documentation") pointing to the component folder on GitHub, plus a **Markdown description** with path, class, selector, an Angular usage example, the Figma-property → input mapping and the accessibility rules | Figma, page "Komponenten", 14 components |
| Code → Figma | **`@figma` JSDoc tag** with the node URL at the top of every mapped component class (§3), checked in CI (§4) | Angular source |
| Overview | The table in §2 (this document) | Project docs / later `docs/` in the repo |

**Upgrade path:** if the project later moves to an Organization plan, Code Connect templates (`*.figma.ts`, parserless `figma.code` templates) can be generated from §2 without design changes, because the Figma properties already map 1:1 to the component inputs. Prerequisites then: publish the "Komponenten" page as a team library, then run step 7h again with the skill `Figma/figma-code-connect`.

**Note on the links:** the documentation links point to the planned folders on `main`. They return 404 until the component is created. That is intended: the link becomes valid the moment the folder is pushed under the agreed path.

---

## 2. Mapping table

Base path in the repo: `CLIENT/CLIENT.ProcureAccess/src/app/`. Folder and file naming follows the existing convention (`shared/components/markdown-editor/markdown-editor.ts|html|scss`, class without suffix, selector prefix `pa` from `angular.json`).

| Figma component | Node | Angular path (folder) | Class | Selector | Handoff | Status in code |
|---|---|---|---|---|---|---|
| K1 `pa-fulfillment-breakdown` | [`2014:341`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2014-341) | `shared/components/signals/fulfillment-breakdown/` | `FulfillmentBreakdown` | `pa-fulfillment-breakdown` | §4.1 | new |
| K2 `pa-documentation-signal` | [`2013:233`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2013-233) | `shared/components/signals/documentation-signal/` | `DocumentationSignal` | `pa-documentation-signal` | §4.2 | new |
| K3 `pa-source-label` | [`2010:348`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2010-348) | `shared/components/signals/source-label/` | `SourceLabel` | `pa-source-label` | §4.3 | new |
| K4 `pa-tested-status` | [`2009:103`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2009-103) | `shared/components/signals/tested-status/` | `TestedStatus` | `pa-tested-status` | §4.4 | new |
| K5 `pa-priority-indicator` | [`2011:203`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2011-203) | `shared/components/priority-indicator/` | `PriorityIndicator` | `pa-priority-indicator` | §4.5 | new |
| K6 `pa-assessment-input` | [`2012:2430`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2012-2430) | `shared/components/assessment-input/` | `AssessmentInput` | `pa-assessment-input` | §4.6 | new |
| `pa-entry-row` | [`2033:616`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2033-616) | `features/directory/components/entry-row/` | `EntryRow` | `pa-entry-row` | §4.7 | new |
| `pa-entry-card` | [`2015:378`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2015-378) | `features/directory/components/entry-card/` | `EntryCard` | `pa-entry-card` | §4.8 | new |
| `pa-notice` | [`2032:486`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2032-486) | `shared/components/notice/` | `Notice` | `pa-notice` | §3.3 | new |
| `pa-product-result` | [`2053:593`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2053-593) | `features/directory/components/product-result/` | `ProductResult` | `pa-product-result` | §4.9 | new |
| `pa-contribution-item` | [`2056:593`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2056-593) | `features/directory/components/contribution-item/` | `ContributionItem` | `pa-contribution-item` | §4.10 | new |
| `pa-rating-legend` | [`2099:760`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2099-760) | `shared/components/rating-legend/` | `RatingLegend` | `pa-rating-legend` | §4.11 | new |
| `pa-app-header` | [`2017:650`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2017-650) | `layout/header/` | `Header` | `pa-header` | §3.1 | **exists**, to be reworked |
| `pa-app-header-compact` | [`2039:2131`](https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2039-2131) | `layout/header/` (same component, narrow layout) | `Header` | `pa-header` | §3.1 | **exists**, to be reworked |

**Why these locations:**
- K1–K4 are used in the directory (S41, S42), in contribution previews (S55 step 5) and in "Meine Beiträge" (S53). They cross feature boundaries, so they live in `shared/components/signals/`. Grouping them keeps the four rule-carrying components together for review (ARCHITECTURE §8.4).
- K5 and K6 are used by more than one feature (configurator and shared list; workspace and vendor flow), so they also go in `shared/`.
- `pa-entry-row` and `pa-entry-card` are only used by the directory feature.
- The header keeps the existing selector `pa-header`. The Figma name `pa-app-header` stays as it is; renaming either side would only create churn.

### 2.1 Figma properties → component inputs

These are the same rules that are written into each Figma description (section "Figma → Code").

| Component | Figma property | Input |
|---|---|---|
| K1 | Scope = Configuration \| Public \| Workspace | `scope: 'configuration' \| 'public' \| 'workspace'` |
| | NotCovered = Some \| None | derived from `counts.notCovered > 0` (no input) |
| | Count Fulfilled, Count SlightRestriction, Count Restriction, Count Barrier, Count Blockade, Count NotApplicable, Count Unknown, Count NotCovered | `counts: FulfillmentCounts` (eight properties, BIT inklusiv scale) |
| | Region title | `configurationName` |
| K2 | Report = Yes \| No | `hasReport` |
| | Report (text), Age | `reportFormat` + `reportDate`, `ageMonths` |
| | Test type | `testEvidences: TestEvidenceSummary[]` — the text joins the `testType` of all evidences (`signals.documentation.testType`); the Figma example shows one value (handoff H18) |
| K3 | Channel = Vendor \| Independent | `channel: 'vendor' \| 'independent'` |
| | Layout = Narrow \| Wide | no input — container query (`container-type: inline-size`), Narrow is the base, Wide from 840 px |
| | Expanded | internal state (disclosure) |
| | Published by | `displayName` + `profileKind` |
| K4 | Tested = Yes \| No | `tested: boolean` |
| | Evidence 1, Evidence 2 (TEXT), Show evidence 2 (BOOLEAN) | `testEvidences: TestEvidenceSummary[]` — one `<li>` per evidence (`signals.tested.evidence.item`); Figma shows up to two, the code renders all |
| K5 | Level = A \| B \| Hinweis | `level: model<'A' \| 'B' \| 'Info'>` |
| | Show why button | `showRationaleToggle: boolean` (false in S11) |
| | Expanded | internal state ("Warum diese Priorität?") |
| | Show override, Override text | `recommendedLevel` (override shown when ≠ `level`) |
| | Rationale, Matched rule, Weight | `rationale`, `matchedRule`, `weight` |
| K6 | Value = Unknown \| Fulfilled \| SlightRestriction \| Restriction \| Barrier \| Blockade \| NotApplicable | `value: model<AssessmentValue>` (1:1); `allowedValues` (union over the selected app types, from the API) has no Figma property (all seven shown); procedure criteria do not use K6 (derived values, ARCHITECTURE_v10 C46) |
| | (no Figma property) compact form in S34b-M | `layout: 'compact' \| 'radios'` from `BreakpointObserver` (< 600 px compact) |
| | Criterion title | `criterionTitle` |
| `pa-entry-row` | Channel | `entry.channel` |
| | Expanded | `expanded: model<boolean>` |
| | Layout = Wide \| Narrow | **no input**: container query, Wide from 840 px container width |
| | Published by, Date, Tested value, Documentation | fields of `entry` |
| `pa-entry-card` | Channel | `entry.channel` |
| | Layout = Narrow \| Wide | no input — container query, Narrow is the base, Wide from 840 px |
| `pa-product-result` | Product name, Vendor line, Entries, Last published, Use case line, Show use case line | fields of `product` (`DirectoryProductSummary`); the use-case line shows only with an active use-case filter (`useCaseEntryCount` set) |
| `pa-contribution-item` | Title, Status, Meta | fields of `contribution` |
| | Show update / Show withdraw | derived: kind = test result / state = visible |
| `pa-notice` | Kind = Info \| Attention \| Error | `kind: 'info' \| 'attention' \| 'error'` |
| | Show title + Title | `title` (empty = no title; required for `error`) |
| | Text | content projection |
| `pa-app-header` | Auth = Anonymous \| SignedIn \| SignedInNoOrg | **no input**: from `withIdentity()` |
| | Active | `routerLinkActive` |

### 2.2 Screens → pages (proposal)

Screens are not linked by Dev Mode documentation links. This table is the reference, and every page component carries the `@figma` tag (§3). **Bold** = 360 px base frame (mobile-first), first `@figma` line of the page.

| Screen | Figma node(s) | Planned page component |
|---|---|---|
| S10 Kriterien zusammenstellen (steps 1–4, S10-R, S10-E, S10-M, S10-M1) | **360: `2134:3174` (S10-M1, step 1)**; `2019:407`, `2086:2142`, `2088:2200`, `2089:2259`, `2089:2692` (reset), `2139:3332` (error), `2103:2940` (320 px) | `features/configurator/pages/criteria-select/` |
| S11 Kriterienliste | `2020:70`, `2039:2546` (320 px) | `features/configurator/pages/criteria-list/` |
| S13 Export-Dialog | **360: `2134:3825`**; `2021:225`, `2039:1944` (error) | `features/configurator/dialogs/export-dialog/` |
| S32 Projekt – Übersicht | **360: `2148:4697`**; `2148:3877` | `features/workspace/pages/project-detail/` |
| S34 Produkt im Projekt (Überblick, Prüfnachweis, Einschätzung) | **360: `2134:3184` (Überblick), `2136:3127` (Einschätzung)**; `2097:2627`, `2022:267`, `2094:2525` (S34a-J), `2144:3508` (S34a-L, several evidences), `2022:841` | `features/workspace/pages/candidate-detail/` (child routes `pruefnachweis`, `einschaetzung`) |
| S35 Eintrag veröffentlichen | `2023:633` (gesperrt), `2053:2438` (Schritt 2) | `features/workspace/pages/publish/` |
| S37 Bearbeitungskonflikt | `2056:2519` | `shared/components/conflict-dialog/` (used by workspace and configurator) |
| S40 Produktsuche | **360: `2134:2905`**; `2053:2051`, `2142:3850` (S40-V, use case selected) | `features/directory/pages/product-search/` |
| S41 Produktseite | `2025:676`, `2039:2139` (320 px) | `features/directory/pages/product-detail/` |
| S42 Eintrag – Detail | **360: `2132:3382`**; `2054:2542` | `features/directory/pages/entry-detail/` |
| S51 Erfahrungsbericht | **360: `2136:4005`, `2136:4187` (keyboard)**; `2026:1036`, `2039:1803` (error) | `features/directory/pages/experience-report/` |
| S53 Meine Beiträge | `2056:2334` | `features/directory/pages/my-contributions/` |
| S55 Prüfergebnis veröffentlichen | `2145:3653` (step 1), `2145:4312` (step 2), `2027:1125` (step 4), `2039:1644` (error), `2039:2739` (320 px) | `features/directory/pages/vendor-publication/` |

The feature names follow the store slices in ARCHITECTURE_v10 §8.1 (`withConfigurator`, `withWorkspace`, `withDirectory`). Flows D and E live in `directory` because their state lives in `withDirectory()`.

---

## 3. Convention in the code: `@figma` tag

Every component listed in §2 and §2.2 starts with a JSDoc block on the class:

```ts
/**
 * K1 · Abgleich mit Kriterien — all fulfilment counts of its scope (eight; seven in the workspace), never a summary value.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2014-341
 * @spec docs/design/design_handoff_procureaccess_v1.md §4.1
 * @invariants SIG-07, SIG-08, SIG-09, CAT-04
 */
@Component({
  selector: 'pa-fulfillment-breakdown',
  templateUrl: './fulfillment-breakdown.html',
  styleUrl: './fulfillment-breakdown.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FulfillmentBreakdown { … }
```

**Rules:**
- `@figma` on **components**: exactly one URL, pointing to the **component set** (not a variant, not an instance).
- `@figma` on **pages (mobile-first)**: the **first** `@figma` line points to the 360 px base frame where one exists (S42-M, S40-M, S10-M1, S32-M, S34-M, S13-M, S34b-M, S51-M; see §2.2), the **second** to the desktop frame. Pages without a 360 px frame point to the desktop frame only. Further states (errors, 320 px zoom checks) may follow as additional lines with a short label, e.g. `@figma … (error)`.
- `@spec`: the handoff section. The design documents move to `docs/design/` in the repository when the specification phase starts (step 8). Until then, the link stays as written.
- `@invariants`: only for components that carry invariants from TESTING_v8. Tests for these components carry the same IDs (`INV:SIG-08` …), see TESTING_v8 §8.1.
- If a component is renamed or moved, the Figma documentation link must be updated in the same pull request. The PR template gets a checkbox for it (step 9/10).

---

## 4. CI check

A small script keeps the bridge from rotting: **`scripts/check-figma-links.mjs`** (Node, no dependencies; since sprint 01, task S1-21). The script is the source of truth; this section only describes it. It runs in stage 1 ("Lint & licences") of the pipeline (TESTING_v15 §8.2). Run it from anywhere: `node scripts/check-figma-links.mjs`.

What it checks:

- **`MAP`** lists the mapped component files (paths relative to `CLIENT/CLIENT.ProcureAccess/`) with their Figma node IDs from §2. It has 13 entries for the 14 Figma components, because `pa-app-header` and `pa-app-header-compact` are one Angular class (`Header`).
- A mapped file that exists must contain `@figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=<node>`. The node ID must end there, so node `2014-341` does not match a tag for `2014-3410`. A missing or wrong tag fails the build.
- A mapped file that does not exist yet is skipped, so the check tightens itself as components appear during the migration.
- **`PENDING`** lists mapped paths where a file exists that is not the mapped component yet (TESTING V60). They get a notice instead of a failure, and a notice asks to remove the entry once the file carries its tag. Today: `src/app/layout/header/header.ts`, the legacy header, until the step-1 shell. A `PENDING` entry that is not in `MAP` fails the build.
- The script prints how many files it checked, skipped and kept pending.

Changes to the mapping (§6) update `MAP` in the script and §2 in the same pull request.

---

## 5. What was changed in Figma

On each of the 14 components on the page "Komponenten" (11 in step 7h; `pa-product-result`, `pa-contribution-item` and `pa-rating-legend` added afterwards):
- **Documentation link** → GitHub folder from §2. It shows in Dev Mode under "Documentation" and in the component's properties panel.
- **Description** rewritten in Markdown, with these sections:
  - purpose and product rule (German)
  - **Code (Angular)**: path, class, selector, handoff section, usage example
  - **Figma → Code** mapping
  - **Barrierefreiheit** (short; the full rules are in the Dev Mode annotations from 7f)

The accessibility annotations from 7f stay unchanged.

---

## 6. Maintenance

| Event | Do |
|---|---|
| New domain component | Create it in Figma (page "Komponenten"), set documentation link + description as in §5, add a row to §2 and to `MAP` in `scripts/check-figma-links.mjs` (§4) |
| Component renamed or moved in code | Update the Figma documentation link and description in the same PR; update §2 and `MAP` in the script |
| Figma variant or property added | Update §2.1 and the component's inputs; the description section "Figma → Code" in Figma |
| Switch to Organization plan | Publish the library; generate Code Connect templates (`*.figma.ts`) from §2.1; keep the `@figma` tags (they still serve code → Figma) |
