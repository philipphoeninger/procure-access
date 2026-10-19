# ProcureAccess — Design Handoff (step 7g)

> **Status:** v1.14 — output of Phase 2, step 7g; amended 2026-10-02 with the screens designed after 7h (S35 step 2, S37, S40, S42, S53) and decision E13 (experience reports); amended 2026-10-06 with the design review points 1–6 (four-step S10 with filter layers and exclusions, Prüfverfahren, BIT inklusiv rating scale, criterion detail view) and later the same day with S34 "Überblick", the rating-scale explanation `pa-rating-legend`, the S40 filter order and the E13 follow-up; amended again with the second design critique (`design_critique_procureaccess_v2.md`, U28–U34); 2026-10-12: §2 rewritten **mobile-first** with 360 px base frames; accessibility review round 2 (`accessibility_review_procureaccess_v2.md`); all UI-string keys referenced here checked against `ux_copy_procureaccess_v7.md` ("Hand off to implementation") of `ai_development_process.md`; v1.8 (2026-10-12): consistency audit before `SPEC_v1.md` — procedure criteria in K6, allowed-ratings union, entry context (variant A) in S40/S41/S55, experience reports without K1, K1 workspace with seven rows, Angular 22, string keys checked against `ux_copy_procureaccess_v9.md`. v1.9 (2026-10-12): architecture review — several test evidences in S34a/S55, procedure criteria shown as derived values (no K6), consent for every experience report, locked criteria list, Angular 22 / .NET 10; keys checked against `ux_copy_procureaccess_v9.md`. v1.10 (2026-10-13): Figma frames S34a-L (several evidences), S55-1 and S55-2, derived procedure rows in S34b, S51 consent always visible; Identity cookie session (ARCHITECTURE_v8 C50); editing a locked list (C51); references updated to the latest documents. v1.11 (2026-10-13): S32 project overview designed (S32, S32-M); session timeouts (ARCHITECTURE_v9 C52). v1.12 (2026-10-13): review of the main documents (`pruefung_hauptdokumente_v1.md`) — K2/K4 take a list of test evidences, S32, S34a, S34b, S40-V, S51 and S55 per the shared content decisions, vendor entries without assessments (D2), project copy of the criteria list (D1), login overlay on 401, keys checked against `ux_copy_procureaccess_v12.md`, references to ARCHITECTURE_v10 / TESTING_v8. v1.13 (2026-10-15): follow-ups of `SPEC_v1.md` §14 — **dark theme in v1** (§1.1, SPEC SD11), field limits confirmed (§8.2, H6 closed), staleness threshold 18 months, screens without design for settings (appearance), legal pages, administration, e-mail confirmation, organisation members and removing candidates (§5.3); keys checked against `ux_copy_procureaccess_v13.md`; references to ARCHITECTURE_v11 / TESTING_v9. v1.14 (2026-10-19): the administration screen S81 and removing a candidate are confirmed (`SPEC_v2.md` SD21, SD22), *proposal* markers removed; references to ARCHITECTURE_v12, TESTING_v10 and UX copy v14. Input for step 8 (`SPEC_v1.md`); step 7h is replaced by `figma_code_mapping_procureaccess_v1.md` (no Code Connect on the Figma plan in use).
> **Sources:**
> - Figma design file "ProcureAccess" (`rPeSyK8NNe3d8AnUEXW9mt`), state 2026-10-13. The frames S32, S32-M, S34a-L, S34b (derived rows), S40-V, S51, S55-1 and S55-2 follow the decisions of `pruefung_hauptdokumente_v1.md` (amended 2026-10-13: K4 shows an evidence list, K2 lists all procedures, S55-1 carries a Development annotation for the chip multi-select); where a frame still differs, this document describes the intended behaviour
> - `ux_copy_procureaccess_v14.md` — all UI strings and translation keys
> - `kriterienkatalog_bit_inklusiv_v1.md` — filter layers, exclusion rules, rating scale, criterion display (German)
> - `design_critique_procureaccess_v1.md` and `design_critique_procureaccess_v2.md` (7e, rounds 1 and 2)
> - `accessibility_review_procureaccess_v1.md` and `accessibility_review_procureaccess_v2.md` (7f, rounds 1 and 2)
> - `ARCHITECTURE_v12.md` (esp. §5.1 filter layers, procedure criteria and allowed ratings, §5.5 entry context, §8 frontend, §8.4 UI rule, §9.4 accessibility)
> - `TESTING_v10.md` (invariants SIG-*, CAT-*, CFG-*, DIR-*, PUB-*, SEC-*, WSP-*, A11Y-*)
> - `pruefung_hauptdokumente_v1.md` (review of the main documents, 2026-10-13, German; decisions D1–D7)
>
> **Language:** English, because this is a developer-facing technical document (project convention). UI strings stay German and live only in `ux_copy_procureaccess_v14.md` / `de.json`; this document references them by key and never duplicates them.
> **Stack assumed:** Angular 22 (zoneless, standalone components, signal inputs), Angular Material 22 (M3), NgRx Signals, ngx-translate; backend .NET 10.

---

## 0. How to use this document

1. **Read §1 (foundations) and §2 (layout) once.** They apply everywhere and replace per-screen pixel values.
2. **Build the shared and domain components (§3, §4) before any screen.** Six of them (K1–K6) carry product rules (ARCHITECTURE §8.4). If a screen needs to show signals, it must use these components, never ad-hoc markup.
3. **Build screens from §5.** Each screen row points to its Figma node. The Figma annotations of the category **"Barrierefreiheit"** (visible in Dev Mode) hold the per-screen accessibility spec; §7 summarises them.
4. **Screens without a Figma design** (§5.3) are built from Angular Material defaults plus the patterns in §6. Do not invent new visual patterns for them.

**Principle:** if this document, the UX copy and Figma disagree, the order of precedence is **UX copy v14 (wording) → this document (behaviour, tokens) → Figma (visual reference)**. Report disagreements as an issue rather than guessing.

### 0.1 Figma map

| Page | What is there | Node |
|---|---|---|
| Cover | Status and reading guide | `2008:33` |
| Komponenten | Domain components K1–K6, entry card and row, notice, headers, focus spec | page `2008:10` |
| Screens | Desktop screens (1440), error states (`-E`), 320 px versions (`-M`) | page `2008:11` |

Flow diagrams (flows A–E) live in the FigJam board `4JmdtppBCOiAKzYPzzCLkv`.

---

## 1. Foundations

### 1.1 Theme

The Figma file uses the **Material 3 Design Kit** baseline scheme (source colour `#6750A4`), light mode, density 0.

**Implementation:**

```scss
// styles.scss — single global theme
@use '@angular/material' as mat;
@use './theme-colors' as pa; // generated, see below

html {
  color-scheme: light dark;        // v1.13: both appearances (SPEC SD11)
  @include mat.theme((
    color: (primary: pa.$primary-palette, tertiary: pa.$tertiary-palette, theme-type: color-scheme),
    typography: (plain-family: 'Roboto', brand-family: 'Roboto'),
    density: 0,
  ));
  @include mat.strong-focus-indicators((border-style: solid, border-width: 3px));
}
```

- Generate the palettes with `ng generate @angular/material:theme-color` (primary `#6750A4`, secondary `#625B71`, tertiary `#7D5260`, neutral `#605D62`, neutral-variant `#605D66`, error `#B3261E`).
- **Check the result:** after generating, compare the resolved `--mat-sys-*` values in the browser with the table in §1.2. If any colour that carries text or state differs, pin it with `mat.theme-overrides(...)`. The contrast results of 7f were measured with the Figma values and only hold for them.
- **Dark theme (changed in v1.13, SPEC SD11, ARCHITECTURE_v11 C73):** part of v1. `theme-type: color-scheme` makes Angular Material emit light and dark values through `light-dark()`; the active appearance is forced with a class on `html` (`theme-light` / `theme-dark` → `color-scheme: light` / `dark`) from the user setting `settings.appearance.*`, otherwise `prefers-color-scheme` applies. Components use only `--mat-sys-*` and `--pa-*` tokens (§1.2), so no component needs theme-specific code. **Before release** (H20): dark mode for the Figma variables and the designed screens, an accessibility review of the dark values (every role in §1.2, focus ring, `outline-variant` decorative only) and pinning of deviations with `mat.theme-overrides`. The table in §1.2 lists the light values; the dark values are added to it after the review. All rules of §1.2 (no product colour coding, both channels identical) apply unchanged.
- **High contrast setting** (`settings.contrast.high`, A11Y-06): the visual design is still open (§9). For now, honour `prefers-contrast: more` only by keeping Material's defaults; do not ship a half-designed theme.

### 1.2 Colour tokens

Only these roles are used. Use the CSS custom property, never the hex value.

| Figma variable | CSS (Angular M3) | Value | Used for |
|---|---|---|---|
| Schemes/Surface | `--mat-sys-surface` | `#FEF7FF` | Page background |
| Schemes/Surface Container Lowest | `--mat-sys-surface-container-lowest` | `#FFFFFF` | Cards (criterion card, entry card, entry row, K1/K2 cards, fieldset in S55), K5 rationale panel |
| Schemes/Surface Container Low | `--mat-sys-surface-container-low` | `#F7F2FA` | Excluded criterion card (S11) |
| Schemes/Surface Container | `--mat-sys-surface-container` | `#F3EDF7` | Header (`pa-app-header`), "damals nicht im Katalog" tag and block (K1, `pa-entry-row`) |
| Schemes/Surface Container High | `--mat-sys-surface-container-high` | `#ECE6F0` | Sticky action bar (S11), dialog, `pa-notice` Info |
| Schemes/Surface Container Highest | `--mat-sys-surface-container-highest` | `#E6E0E9` | (Material internals only) |
| Schemes/On Surface | `--mat-sys-on-surface` | `#1D1B20` | Body text, headings |
| Schemes/On Surface Variant | `--mat-sys-on-surface-variant` | `#49454F` | Secondary text, hints, design-note text, `pa-notice` Info icon |
| Schemes/Primary | `--mat-sys-primary` | `#6750A4` | Links, text buttons, filled buttons, active-nav indicator, `pa-notice` Attention accent and icon |
| Schemes/On Primary | `--mat-sys-on-primary` | `#FFFFFF` | Text on filled buttons, icon glyphs |
| Schemes/Primary Container | `--mat-sys-primary-container` | `#EADDFF` | K5 level button background |
| Schemes/On Primary Container | `--mat-sys-on-primary-container` | `#4F378A` | K5 level button text |
| Schemes/Secondary | `--mat-sys-secondary` | `#625B71` | **Focus ring** (kit focus indicator) |
| Schemes/Secondary Container | `--mat-sys-secondary-container` | `#E8DEF8` | Active nav item, `pa-notice` Attention |
| Schemes/On Secondary Container | `--mat-sys-on-secondary-container` | `#4A4459` | `pa-notice` Attention title and text |
| Schemes/Outline | `--mat-sys-outline` | `#79747E` | Text-field borders (Material default), `pa-notice` Info accent bar |
| Schemes/Outline Variant | `--mat-sys-outline-variant` | `#CAC4D0` | Card borders, dividers — **decorative only, never for text or state** (1.7 : 1) |
| Schemes/Error | `--mat-sys-error` | `#B3261E` | `mat-error` text, error icon, error accent |
| Schemes/Error Container | `--mat-sys-error-container` | `#F9DEDC` | `pa-notice` Error background |
| Schemes/On Error Container | `--mat-sys-on-error-container` | `#852221` | `pa-notice` Error text |
| Schemes/Scrim | `--mat-sys-scrim` | `#000000` at 32 % | Dialog backdrop (Material default) |

**Hard rules:**
- **No product-related colour coding.** No green/amber/red for counts, tested status or channels. Both channels use the same colours (PRD §7, UX copy G7/G9).
- Colour never carries meaning alone. Every state has text, and `pa-notice` additionally has an icon.

### 1.3 Typography

Roboto, **self-hosted** (`@fontsource/roboto` 400 and 500, latin + latin-ext). Do not load it from Google Fonts or any other external CDN (ARCHITECTURE §9.3: no third-party requests). Sizes are in px in Figma and become rem in code (16 px = 1 rem). Never set font sizes in px, so browser text zoom keeps working (WCAG 1.4.4).

| Figma text style | CSS (Angular M3) | Size / line height | Weight | Used for |
|---|---|---|---|---|
| headline/large | `--mat-sys-headline-large` | 32 / 40 | 400 | H1 on every screen (desktop) |
| headline/medium | `--mat-sys-headline-medium` | 28 / 36 | 400 | Doc pages only |
| headline/small | `--mat-sys-headline-small` | 24 / 32 | 400 | H2 section headings (S41), dialog titles, step titles (S55); **H1 below 600 px** |
| title/large | `--mat-sys-title-large` | 22 / 28 | 400 | App name in the header |
| title/medium | `--mat-sys-title-medium` | 16 / 24 | 500 | Card titles (criterion title, K1/K2 region titles) |
| title/small | `--mat-sys-title-small` | 14 / 20 | 500 | Channel label (K3), notice title, row labels |
| body/large | `--mat-sys-body-large` | 16 / 24 | 400 | Intro text, radio and checkbox labels, input text |
| body/medium | `--mat-sys-body-medium` | 14 / 20 | 400 | Default body text, **all rule-carrying text** (counts, "damals nicht im Katalog", tested status), breadcrumb |
| body/medium-emphasized | `--mat-sys-body-medium` + `font-weight: 500` | 14 / 20 | 500 | Progress text ("12 von 48 …"), step indicator below 600 px |
| body/small | `--mat-sys-body-small` | 12 / 16 | 400 | Field hints, `mat-error`, metadata (date · catalog release) |
| label/large | `--mat-sys-label-large` | 14 / 20 | 500 | Buttons (Material default) |
| label/medium | `--mat-sys-label-medium` | 12 / 16 | 500 | Column labels in `pa-entry-row` ("Geprüft", "Nachweise", "Abgleich …") |
| label/small | `--mat-sys-label-small` | 11 / 16 | 500 | Not used for content (Material internals only) |

**Rule from 7e:** text that carries a product rule (the eight counts, "damals nicht im Katalog", "Auf Barrierefreiheit geprüft: Ja/Nein", the source label) is never smaller than **body/medium (14 px)**.

### 1.4 Spacing

Spacing is the only custom token set. Figma collection: "ProcureAccess / Spacing" (scope GAP/padding; web code syntax already set).

```scss
:root {
  --pa-space-xxs: 0.25rem; //  4
  --pa-space-xs:  0.5rem;  //  8
  --pa-space-sm:  0.75rem; // 12
  --pa-space-md:  1rem;    // 16
  --pa-space-lg:  1.5rem;  // 24
  --pa-space-xl:  2rem;    // 32
  --pa-space-xxl: 3rem;    // 48
}
```

| Context | Token |
|---|---|
| Page top and bottom padding (`main`) | `--pa-space-xl` |
| Page side padding below 1072 px viewport | `--pa-space-md` |
| Gap between page sections | `--pa-space-lg` |
| Gap between cards in a list | `--pa-space-md` |
| Card padding (criterion card, fieldset, dialog) | `--pa-space-lg`; **`--pa-space-md` below 600 px** |
| Gap inside a card | `--pa-space-sm` |
| Entry row padding / column gap | `--pa-space-md` / `--pa-space-lg` |
| Label ↔ value, hint ↔ field | `--pa-space-xxs` |
| Gap between buttons in an action row | `--pa-space-sm` |
| Header side padding / item gap | `--pa-space-lg` / `--pa-space-lg` |

### 1.5 Shape and elevation

| Figma variable | CSS | Value | Used for |
|---|---|---|---|
| Corner/Extra-small | `--mat-sys-corner-extra-small` | 4 px | Text fields (Material default) |
| Corner/Small | `--mat-sys-corner-small` | 8 px | `pa-notice`, K5 level button, "damals nicht im Katalog" tag and block, top corners of active nav item |
| Corner/Medium | `--mat-sys-corner-medium` | 12 px | `pa-entry-row`, K1 and K2 cards inside the entry card |
| Corner/Large | `--mat-sys-corner-large` | 16 px | Criterion card, entry card, S55 fieldset |
| Corner/Extra-large | `--mat-sys-corner-extra-large` | 28 px | Dialogs (Material default) |
| Corner/Full | `--mat-sys-corner-full` | pill | Buttons, nav items (Material default) |

**Elevation:** the design is flat. Cards use a 1 px `--mat-sys-outline-variant` border and **no shadow**. Dialogs keep Material's default elevation. Do not add shadows to cards. It keeps both channels visually equal and avoids "highlighting" one entry.

### 1.6 Focus

Figma: Komponenten → "Doc / Fokus und Zustände (7f)" (`2039:515`).

- Material components: `mat.strong-focus-indicators` (see §1.1). This gives a 3 px ring in `--mat-sys-secondary`, 2 px offset, shown on `:focus-visible` only.
- Own components and plain elements (skip link, `pa-entry-row` toggle, K5 level button, links):
  ```scss
  :where(a, button, [tabindex]):focus-visible {
    outline: 3px solid var(--mat-sys-secondary);
    outline-offset: 2px;
  }
  ```
- Text fields show focus through Material's 2 px primary border. Do not add a ring around them.
- **Never** `outline: none` without a replacement.
- Sticky elements must not cover the focused element (WCAG 2.4.11): `html { scroll-padding-bottom: <sticky bar height> }` on S11, and `scroll-padding-top: <header height>` if the header is ever made sticky (it is not in v1).

### 1.7 Motion

No custom animation in v1. Use Material defaults only: expansion panel, dialog, menu, ripple, stepper.
- Respect `prefers-reduced-motion: reduce` **and** the user setting `settings.motion.reduced`. When either is active, provide `provideAnimationsAsync('noop')`-equivalent behaviour (e.g. a global class that sets `--mat-*-animation-duration: 0ms`, or `MATERIAL_ANIMATIONS` with `animationsDisabled: true`).
- Nothing moves by itself: no carousels, no auto-dismissing snackbars for important content, no animated counters.

### 1.8 Icons

Material Symbols (Outlined, weight 400), **self-hosted** (`material-symbols` package), used via `mat-icon`. Icons are always `aria-hidden="true"`. The meaning is always also in text.

| Icon | Where |
|---|---|
| `info` | `pa-notice` Info |
| `warning` | `pa-notice` Attention |
| `error` | `pa-notice` Error, text field in error state (Material) |
| `arrow_drop_down` | K5 level button, selects (Material) |
| `calendar_today` | Datepicker toggle (S55, S34) |
| `check` | Checkbox (Material) |

No images or illustrations are used. There are no assets to export.

---

## 2. Layout and responsive behaviour (mobile-first)

**Implementation principle (owner decision, 2026-10-12): mobile-first.** The narrow layout is the base style of every component and page; wider layouts are added with `min-width` media or container queries. Figma reference widths:

| Width | Role | Frames |
|---|---|---|
| **360 px** | **Base** (reference smartphone) | S42-M, S40-M, S10-M1, S34-M, S13-M, S34b-M, S51-M, S51-M2 (row at y ≈ 8000 on page "Screens") |
| 320 px | Zoom check: 1280 px at 400 % (WCAG 1.4.10) — must work without horizontal scrolling | S41-M, S11-M, S55-M, S10-M |
| 1440 px | Desktop | all other frames |

Screens without a 360 px frame are built from the base rules below plus the patterns of the frames above.

### 2.1 Page structure

```
<a class="pa-skip-link" href="#main">           shell.skipLink
<header> pa-app-header-compact (base) | pa-app-header (≥ 600 px)
<main id="main" tabindex="-1">
  <div class="pa-content">                      full width, padding-inline 16 px; max-width 1040 px from 1072 px
    breadcrumb → h1 → intro → sections …
  </div>
</main>
<footer>                                        (not designed, see §9)
[action bar — S11: in flow (base), sticky from 600 px width AND 600 px height]
```

| Element | Base (360 px) | From breakpoint |
|---|---|---|
| Content column | full width, `padding-inline: var(--pa-space-md)` (16 px) | `max-width: 65rem; margin-inline: auto` |
| `main` | `padding-block: var(--pa-space-lg)` (24 px) | `--pa-space-xl` from 600 px |
| Header | `pa-app-header-compact` (app name + "Menü") | `pa-app-header` from 600 px |
| H1 | `headline/small` | `headline/large` from 600 px |
| Cards (forms, overview) | padding 16 px, radius `corner-large` | padding 24 px from 600 px |
| Dialogs | **full screen** (`width/height: 100%`, `maxWidth: 100vw`): title bar (h2) at the top, content scrolls, actions fixed at the bottom, stacked in DOM order (S13-M) | 600 px wide centred dialog from 600 px |

### 2.2 Breakpoints

All rules are written **upwards** (`min-width`). Two kinds of breakpoints:

| Breakpoint | Type | Applies to | Base (below) | From the breakpoint |
|---|---|---|---|---|
| **600 px** | viewport (`min-width: 37.5em`) | header, H1, page paddings, title rows, forms, steppers, dialogs, tabs, tables, filters, K6 | see table rows below | desktop arrangement |
| **840 px** | **container** (`container-type: inline-size`) | `pa-entry-row`, `pa-entry-card`, K3, filter row in S40 | Narrow | Wide |

| Pattern | Base (mobile-first) | ≥ breakpoint | Frame |
|---|---|---|---|
| Header | compact; navigation, organisation and account in a `mat-sidenav` (`mode="over"`) opened by "Menü" | full navigation (600 px) | all -M |
| Title rows (S11, S34) | H1, then the button below, left-aligned; hint text left-aligned | side by side (600 px) | S34-M |
| Stepper (S10, S35, S55) | text line `criteria.select.stepLabel` / `vendor.stepIndicator` instead of the step header | `mat-stepper` horizontal (600 px) | S10-M, S10-M1 |
| Actions (forms, steps, dialogs) | stacked, full width, **in DOM order** (secondary first, primary last — the primary ends up next to the thumb) | in one row, right-aligned, same order (600 px) | S10-M, S13-M, S51-M |
| Forms | one column; long questions as a visible `<label>` above the field, short `mat-label` inside (otherwise the floating label is truncated); field supporting texts only on focus where marked | rows of two fields allowed (600 px); S51 keeps the visible labels above the fields on every width (v1.12) | S51-M |
| Combobox (S10 step 1, S40 Verwendungszweck) | on focus the field scrolls to the top of the visual viewport (`scroll-margin-top` = header height + 8 px); the listbox opens below with `max-height` = `visualViewport.height` − field bottom, scrolls internally; no full-screen search, no bottom sheet | unchanged | S10-M1 |
| Tabs (S34) | `mat-tab-nav-bar`, `stretchTabs=false`, short label `candidate.tabs.assessment.short` ("Einschätzung"); if still too wide, the bar scrolls horizontally (pagination), never the page | full labels (600 px) | S34-M, S34b-M |
| Filters (S40) | search field visible; filters, origin, report checkbox and sort inside a disclosure "Filter und Sortierung ({{n}} aktiv)" (`aria-expanded`, not a dialog); default collapsed; filters apply immediately; result count is a live region | filter row open (840 px container) | S40-M |
| Tables (S42 "Einschätzung je Kriterium", S14, S30) | `<ul>` of items: criterion as `h3`, then `<dl>` (Einschätzung, Begründung; empty rationale omitted) | `mat-table` (600 px) | S42-M |
| Description lists (S34 overview, legend) | term above value | term column 160–200 px left (600 px) | S34-M |
| K6 assessment | **compact:** legend = criterion title, `mat-select` "Einschätzung" with the allowed values, note behind "Begründung hinzufügen/bearbeiten" (disclosure) | radio group (600 px) | S34b-M |
| `pa-rating-legend` | collapsed by default everywhere (also S34b) | S34b expanded until the first assessment (600 px) | S34b-M |
| `pa-entry-card`, K3, `pa-entry-row` | Layout=Narrow: everything stacked; K2 above K1, both own regions; counts as a vertical list | Layout=Wide (840 px container) | S42-M, S41-M |
| S11 action bar | in the flow after the list | sticky at the bottom from 600 px width **and** 600 px height | S11-M |
| On-screen keyboard | nothing is fixed to the bottom while an input has focus; the focused field is scrolled into view (`scrollIntoView({block: 'center'})`); textarea grows to 8 lines, then scrolls | – | S51-M2 |

**General rules:**
- **Touch:** every interactive target ≥ 48 × 48 px (padding or pseudo-element; text links in running text excepted, WCAG 2.5.8 allows 24 px), at least 8 px apart. No hover-only information; disclosures instead of tooltips.
- No horizontal page scrolling at 320 px CSS width. Only tab bars may scroll horizontally inside themselves.
- Buttons never use `white-space: nowrap` if their label can exceed the width. Long actions are **links** that wrap (K2 links, `criteria.list.suggest`).
- **DOM order = visual order** at every breakpoint. No CSS `order`, no `row-reverse`. Stacked actions therefore show the primary action last.
- Respect `prefers-reduced-motion` (§1.7) and safe-area insets (`env(safe-area-inset-bottom)` for fixed dialog action bars).
- Test devices for the manual pass (TESTING §7.2): one Android phone (Chrome + TalkBack), one iPhone (Safari + VoiceOver), each at default and largest system font size.

## 3. Shared components

### 3.1 `pa-app-header` and `pa-app-header-compact`

Figma: `pa-app-header` set `2017:650` (variants Auth × Active), `pa-app-header-compact` `2039:2131`.

| Aspect | Spec |
|---|---|
| Implementation | **One** Angular component — the existing `Header` in `src/app/layout/header/` (selector `pa-header`) — that switches its template at 600 px (`BreakpointObserver`). The two Figma components are its two layouts. The Figma name `pa-app-header` is kept; the code keeps `pa-header` (see `figma_code_mapping_procureaccess_v1.md`) |
| Inputs | none; reads `withIdentity()` (signed in, organisations, active organisation) and the router |
| Figma Auth → state | `Anonymous`: nav Kriterien, Produkte, Beitragen + button "Anmelden". `SignedIn`: + "Projekte", organisation display, menu "Konto". `SignedInNoOrg`: like SignedIn, but no "Projekte" and no organisation display |
| Active item | `routerLinkActive` → `aria-current="page"`, background `--mat-sys-secondary-container`, **3 px bottom border `--mat-sys-primary`**, top corners `--mat-sys-corner-small`. The border is required (7f P1); the background alone is 1.13 : 1 |
| Nav items | `mat-button` (text) links, 48 px high |
| Account | `mat-button` with `[matMenuTriggerFor]`, `aria-haspopup="menu"`, name `shell.userMenu.label` |
| Compact | "Menü" `mat-button` with `aria-expanded` and `aria-controls="main-nav"`. Opening moves focus to the first nav item. Esc and `shell.nav.menu.close` close the menu and return focus to "Menü" |
| Strings | `shell.*` |

### 3.2 Skip link

- First focusable element on every page. Visually hidden until focused, then shown top-left above the header as a filled button (Figma: focus doc, example "Sprunglink").
- Target `<main id="main" tabindex="-1">`.
- String: `shell.skipLink`.

### 3.3 `pa-notice`

Figma: set `2032:486`, doc `2032:471`.

```ts
@Component({ selector: 'pa-notice', … })
export class PaNotice {
  kind  = input.required<'info' | 'attention' | 'error'>();
  title = input<string>();                                   // required when kind === 'error' (assert in dev mode)
  live  = input<'off' | 'polite' | 'assertive'>('off');      // only for notices that appear dynamically
  focusOnShow = input(false);                                // error summary only
}
// content projection: <ng-content> = text (may contain links)
```

| Kind | Background | Accent (4 px left border) | Icon | SR prefix (visually hidden) | Text colour |
|---|---|---|---|---|---|
| info | surface-container-high | outline | `info`, on-surface-variant | `common.notice.info` | on-surface |
| attention | secondary-container | primary | `warning`, primary | `common.notice.attention` | on-secondary-container |
| error | error-container | error | `error`, error | `common.notice.error` | on-error-container |

- Layout: row = icon 20 px + content column (title `title/small`, text `body/medium`), padding `--pa-space-sm` `--pa-space-md`, gap `--pa-space-sm`, radius `corner-small`.
- **ARIA:** static notices have no live region (role `note` is acceptable for info). If a notice appears after an action: `live="polite"` → `role="status"`, error → `role="alert"`.
- **As error summary** (§6.4): `kind="error"`, `focusOnShow`, the title is rendered as `<h2>`, the host gets `tabindex="-1"`, and each line is a link to the field.

### 3.4 Focus, live announcements, page titles (services)

| Service | Responsibility |
|---|---|
| `A11yAnnouncerService` (exists per ARCHITECTURE §9.4) | Wraps `LiveAnnouncer`. `announce(key, params, { politeness = 'polite', throttleMs })`. Throttle per key (autosave: 5 s, counters: 1 s). All announcement strings are keys in UX copy v13 |
| `TitleStrategy` (custom) | Builds `<title>` from route data `pageTitleKey` + params → `shell.pageTitle`. After a failed submit, the form sets the error prefix (`shell.pageTitle.error`) and removes it when valid. On navigation, focus moves to the H1 (`tabindex="-1"`) and the title is announced (A11Y-03) |

---

## 4. Domain components

These carry the product rules of ARCHITECTURE §8.4 and the invariants in TESTING_v10. They are linked to Figma through documentation links and `@figma` JSDoc tags (`figma_code_mapping_procureaccess_v1.md`; Code Connect is not available on the Figma plan in use). The **interfaces below are proposals** that follow the Figma properties 1:1 so that the mapping stays direct. Adjust the names to the final DTOs in SPEC_v1, but keep the variant → input correspondence.

**Mobile-first:** each component is specified with its narrow (base) layout first; wide layouts are enhancements (§2.2). Components with layout variants in Figma: `pa-entry-row`, `pa-entry-card`, K3 `pa-source-label` (Layout = Narrow | Wide, container query 840 px). K6 has a compact form below 600 px (§4.6). K2 actions are links, not text buttons, so they wrap.

Shared types (proposal):

```ts
export type Channel = 'vendor' | 'independent';
export type ProfileKind = 'vendor' | 'auditor' | 'institution' | 'individual';
// BIT inklusiv rating scale (ARCHITECTURE_v11 §16.4.5) plus Unknown
export type AssessmentValue = 'Unknown' | 'Fulfilled' | 'SlightRestriction' | 'Restriction' | 'Barrier' | 'Blockade' | 'NotApplicable';
// "Geprüft nach …" criteria (ARCHITECTURE_v11 C30, C46): derived from test evidences, never entered, never counted
export type DerivedProcedureValue = 'yes' | 'no' | 'notStated';
export type CriterionKind = 'requirement' | 'testProcedure';
export interface FulfillmentCounts {          // directory: all eight present (SIG-07/08)
  fulfilled: number; slightRestriction: number; restriction: number; barrier: number;
  blockade: number; notApplicable: number; unknown: number;
  notCovered?: number;                         // absent in the workspace scope (SIG-13)
}
export type ContextSource = 'projectSelection' | 'vendorDeclared' | 'contributorDeclared';
export interface EntryContext { source: ContextSource; appTypes: string[]; useCases: string[]; }
export type TestTypeKey = 'softwareTest' | 'appTest' | 'pdfTest' | 'bikWeb' | 'bikApp';
export type TestKind = { kind: 'procedure'; testType: TestTypeKey } | { kind: 'other'; name: string } | { kind: 'own' };
// one test evidence as delivered by the API (ARCHITECTURE_v11 §7.3 documentation.testEvidences[]); testType is the display name
export interface TestEvidenceSummary { testType: string; testedBy?: string; testDate?: string; testedVersion?: string; evidenceUrl?: string; }
```

### 4.1 K1 `pa-fulfillment-breakdown` — Abgleich mit Kriterien

Figma `2014:341` · variants Scope = Configuration | Public, NotCovered = Some | None · 460 px wide in the card.

| Input | Type | Figma |
|---|---|---|
| `counts` | `FulfillmentCounts` (required) | Count Fulfilled, Count SlightRestriction, Count Restriction, Count Barrier, Count Blockade, Count NotApplicable, Count Unknown, Count NotCovered |
| `scope` | `'configuration' \| 'public' \| 'workspace'` | Scope |
| `configurationName` | `string` (when scope = configuration) | Region title |
| `entryRelease` / `configurationRelease` | `string` | release note line |

- **Not used for experience reports** (ARCHITECTURE_v11 C33): a report card never contains K1; the former `reportOnly` input is removed.
- **Procedure criteria** („Geprüft nach …“) are never part of the counts (C30).
- **Markup:** `<section aria-labelledby="…">` with `<h3>` (`signals.fulfillment.title.*`), intro, then `<dl>` with exactly eight (Scope Public/Configuration) or seven (Scope Workspace) `<dt>/<dd>` pairs in the fixed order fulfilled → slightRestriction → restriction → barrier → blockade → notApplicable → unknown → **notCovered** (visually separated: own block on `--mat-sys-surface-container`, radius `corner-small`, with `signals.fulfillment.notCovered.explain` directly below when > 0).
- **Forbidden:** sum, percentage, bar, chart, stars, traffic light, colour per category, sorting the categories by value, and a "konform / nicht konform" grouping (BIT inklusiv counts "Leichte Einschränkung" as conformant; ProcureAccess shows no verdict). The component has **no input** that could produce any of these (enforce by type; review rule).
- Zero values are shown ("0 Kriterien"). Never hide a row.
- `signals.fulfillment.release` only when the releases differ.
- Invariants: SIG-07, SIG-08, SIG-09, CAT-04.
- **No assessments (D2, PUB-14):** when the API returns `fulfillment: null` (vendor or auditor entry published without criterion assessments), K1 is not rendered. The entry card keeps the region (same `<section>` and heading `signals.fulfillment.title.*`) and shows the text `signals.fulfillment.none` instead of the list; `pa-entry-row` shows the same text in its Abgleich column.
- **Scope "workspace"** (S34 Überblick): the organisation's own assessment of a candidate. Figma variant `Scope=Workspace, NotCovered=None` (`2101:3180`). Seven rows only (no notCovered — the assessment is pinned to the list's release), no release line, no details button; title `candidate.overview.status.breakdownTitle`, intro `candidate.overview.status.breakdownIntro`. All count texts are bound to the component properties in every variant.

### 4.2 K2 `pa-documentation-signal` — Nachweise

Figma `2013:233` · variant Report = Yes | No.

| Input | Type |
|---|---|
| `hasReport` | `boolean` |
| `reportFormat`, `reportDate`, `ageMonths` | `string`, `Date`, `number` |
| `testEvidences` | `TestEvidenceSummary[]` — all evidences of the version (0..n); replaces the former single `testType` |
| `reportUrl`, `reportSizeBytes` | `string?`, `number?` (P1 upload) |

- `<section aria-labelledby>` with `<h3>` `signals.documentation.title`. **Never in the same container as K1** (SIG-09). In the entry card, K1 and K2 are sibling `<section>`s.
- **Art der Prüfung:** `signals.documentation.testType` with the `testType` of all evidences joined by ", " in the order delivered by the API; omitted when the list is empty.
- The report link text includes format and size (`signals.documentation.report.open`). Opens in the same tab (download via `Content-Disposition`).
- "Was sagt ein Konformitätsbericht aus?" (`signals.documentation.explain.toggle`) is a **link-styled disclosure** (`<button>` styled as a link so it wraps, `aria-expanded`) that shows `signals.documentation.explain` inline. No tooltip.

### 4.3 K3 `pa-source-label` — Herkunft

Figma `2010:348` · variants Channel × Expanded.

| Input | Type |
|---|---|
| `channel` | `Channel` |
| `displayName` | `string` |
| `profileKind` | `ProfileKind` |

- The channel label is **plain text** (`title/small`), not a chip, not interactive (7e U8).
- "Was bedeutet das?" = `mat-button` disclosure (`aria-expanded`, `aria-controls`); the internal `expanded` state shows `signals.source.*.explain`. Add the display name to the button's name via `aria-describedby` (7f V5).
- Both channels look identical apart from the text (PRD §7).
- **Base = Layout=Narrow** (Figma variant): label, "veröffentlicht von …" and "Was bedeutet das?" stacked. **Layout=Wide** from 840 px container width: one wrapping row.

### 4.4 K4 `pa-tested-status` — Auf Barrierefreiheit geprüft

Figma `2009:103` · variant Tested = Yes | No.

| Input | Type |
|---|---|
| `tested` | `boolean` — **not** a tri-state; the directory never shows "Nicht bekannt" (SIG-11) |
| `testEvidences` | `TestEvidenceSummary[]` — used only when `tested` is true; one line per evidence |

- Always rendered for every **test-result** entry (Flows B and E), in both channels, also for "Nein" (SIG-03, SIG-11). No collapse, no hide.
- **Never** rendered for experience reports (E13, SIG-12): the report card shows `contribute.report.typeNote` instead.
- Text: `signals.tested.label`: `signals.tested.yes|no`. The visible text is the accessible name (no diverging `aria-label`, WCAG 2.5.3).
- **Several evidences (v1.12):** with „Ja“ a `<ul>` follows with one `<li>` per evidence, text `signals.tested.evidence.item` („{{testType}} – {{testedBy}}, {{date}}“, e.g. „BIT inklusiv Software-Test – Prüfstelle Beispiel, 14.03.2026“); a missing tester or date is omitted together with its separator. With `tested = true` and no evidence (possible for workspace publications) only „Ja“ is shown. The Figma component shows one line (§9, H18); the list repeats it.
- The project screen S34 does **not** use K4. There the status is a three-option radio group (§5.1, S34a).

### 4.5 K5 `pa-priority-indicator` — Priorität mit Begründung

Figma `2011:203` · variants Level = A | B | Hinweis, Expanded = false | true · properties Show override, Override text, Rationale, Matched rule, Weight.

| Input / output | Type |
|---|---|
| `level` | `model<'A' \| 'B' \| 'Info'>()` |
| `recommendedLevel` | `'A' \| 'B' \| 'Info'` (override shown when ≠ level) |
| `weight` | `number?` (B only) |
| `rationale`, `matchedRule` | `string` |
| `criterionTitleId` | `string` — id of the criterion heading, for `aria-describedby` |
| `showRationaleToggle` | `boolean` (default `true`) — Figma property `Show why button`. **`false` in the criteria list (S11)**: there the rationale is the first section of the card's detail panel (`criteria.item.detail.priority`), so each card has only one disclosure (critique 2, U28) |
| `(reset)` | output → restores recommendation |

- **Level button:** `mat-button` with `[matMenuTriggerFor]`, `aria-haspopup="menu"`. Name `criteria.priority.button` ("Priorität: A – Mindestanforderung") plus `aria-describedby=criterionTitleId`. The visible size is 32 px; the **hit area must be ≥ 48 px high** through padding or a pseudo-element (7f N3). Background `primary-container`, text `on-primary-container`, radius `corner-small`.
- Menu items: `role="menuitemradio"` with `aria-checked`. After a choice, focus returns to the button, then `criteria.priority.changed` is announced.
- "Warum diese Priorität?" = expansion (disclosure) with `aria-describedby=criterionTitleId`. The panel shows rationale, matched rule and source. **The source line "Empfehlung des Projektteams …" is not optional** (UX copy G8).
- **Override:** shown below the level row, stacked: `criteria.priority.overridden` and then the "Empfehlung wiederherstellen" text button (layout changed in 7f so it never clips). CAT-08.
- The row wraps (level button, then "Warum …" on the next line when narrow).

### 4.6 K6 `pa-assessment-input` — Einschätzung je Kriterium

Figma `2012:2430` · variant Value = Unknown | Fulfilled | SlightRestriction | Restriction | Barrier | Blockade | NotApplicable.

| Input / output | Type |
|---|---|
| `criterionTitle` | `string` |
| `allowedValues` | `AssessmentValue[]` — **union** of the `AllowedRatings` of the procedure texts of all selected app types (ARCHITECTURE_v11 C31); `Unknown` is always added; all six when no procedure text applies. Delivered by the API, never computed in the component |
| `value` | `model<AssessmentValue>('Unknown')` — default **Unknown** |
| `note` | `model<string>('')` |

- `<fieldset>`; `<legend>` contains the criterion title (rendered as `<h2>`-styled text, `title/medium`). The question `candidate.assessment.legend` is linked via `aria-describedby`.
- **Base (< 600 px), compact form (S34b-M):** `<fieldset>` with the criterion title as legend, `mat-select` "Einschätzung" (label `candidate.assessment.select`) listing `allowedValues` in the same order, "Nicht eingeschätzt" last; the note field sits behind a text button `candidate.assessment.note.add|edit` (disclosure, focus moves into the field). Input `layout: 'compact' | 'radios'` set by a `BreakpointObserver` — the same component, not two.
- **From 600 px:** `mat-radio-group` with up to seven options in fixed order (Erfüllt … Nicht anwendbar, then Nicht eingeschätzt); only `allowedValues` are rendered (the Figma variants show all seven). On desktop they lay out horizontally and wrap; on narrow screens they stack.
- Note field: outlined `mat-form-field` + `textarea cdkTextareaAutosize`, `aria-labelledby` = own label + criterion title.
- **Autosave** on change (debounce 800 ms). Announce `candidate.assessment.saved` via the announcer at most every 5 s.
- No sliders, no stars.
- **Procedure criteria are not K6** (ARCHITECTURE_v11 C46): in S34b they render as read-only rows — one per selected procedure, in catalog order — with the title in the K6 criterion-title style, the value line `assessment.procedure.derived` (value `DerivedProcedureValue`: Ja / Nein / Nicht angegeben), `assessment.procedure.derived.hint` and the text button `assessment.procedure.derived.edit` (link semantics, navigates to S34a, 48 px target). No input, no autosave, no Figma variant needed.
- **Union note:** S34b shows `candidate.assessment.unionNote` once above the list when the project's list has several app types with different procedures.

### 4.7 `pa-entry-row` — compact entry (S41)

Figma `2033:616` · variants Channel × Expanded × Layout (Wide | Narrow) · properties Published by, Date, Tested value, Documentation.

| Input / output | Type |
|---|---|
| `entry` | `PublishedEntrySummary` (channel, displayName, profileKind, publishedAt, catalogRelease, tested, documentation summary, `counts: FulfillmentCounts \| null` — null → `signals.fulfillment.none`, D2) |
| `comparison` | `{ name: string } \| null` — null → public wording |
| `expanded` | `model<boolean>(false)` |

- **Wide** (container ≥ 840 px): row with four columns — Source (230 px), Geprüft (80 px), Nachweise (170 px), Abgleich (fill). Dividers: 1 px `outline-variant` left border on the Nachweise and Abgleich columns. Padding `--pa-space-md`, column gap `--pa-space-lg`, radius `corner-medium`, background `surface-container-lowest`, 1 px `outline-variant` border.
- **Wide counts (critique 2, U30):** the seven assessment counts form a **two-column grid** in fixed order (`grid-template-columns: 1fr 1fr`, column gap `--pa-space-md`, row gap 2 px), reading row by row: Erfüllt | Leichte Einschränkung, Einschränkung | Barriere, Blockade | Nicht anwendbar, Nicht eingeschätzt. Each item puts the label first („Erfüllt: 22“, `directory.entry.row.*`) and does not wrap inside (`white-space: nowrap`). Then the notCovered tag. No "·" separators in Wide either. Every row of the list therefore has each value at the same position.
- **Narrow:** single column; dividers become top borders; the counts become a vertical list; the toggle moves to the bottom.
- **Markup:** `<li>` inside `<ul>` (one list per channel section). Source line = `<h3>`. The three facts = `<dl>`. The eight counts = `<ul>` with strings `directory.entry.row.*` (seven assessment counts, then the notCovered tag); the "·" separators are **CSS only** (`li + li::before`), never in the text. „Damals nicht im Katalog: N“ is a separate tag (`surface-container`, `corner-small`) and is **always shown, also at 0**.
- **Toggle:** `mat-button` `directory.entry.row.showDetails|hideDetails`, `aria-expanded`, `aria-controls` = id of the `pa-entry-card` rendered directly after the row. Focus stays on the toggle. Name extended with the source (`aria-describedby`).
- The row shows **no** combined value and is never sortable by its counts (D3, `directory.sort.*`).

### 4.8 `pa-entry-card` — full entry

Figma `2015:378` · variant Channel. Composition: K3 → metadata → K4 block → [K2 | K1] side by side (stacked below 840 px) → "Eintrag ansehen".

- `<article aria-labelledby="source-heading-id">`.
- K1 and K2 are separate `<section>`s inside separate cards. Never one container (SIG-09).
- **Base = Layout=Narrow** (Figma variants `Channel × Layout`): K3 Narrow, tested status, then K2 above K1, full width; padding 16 px. **Wide** from 840 px container width: K2 and K1 side by side.
- `directory.entry.stale` appears above the sections when the entry's current version is older than 18 months (SPEC SD7).
- **Context line** (ARCHITECTURE_v11 C32): below the metadata, `directory.entry.context` with the suffix `directory.entry.context.vendorDeclared` (Flow E) or `.project` (Flow B); omitted when the entry has no context. Input `context: EntryContext | null`. Report cards use the same line with `.contributorDeclared` and app types only (D5).

### 4.9 `pa-product-result` — search result (S40)

Figma `2053:593` · properties Product name, Vendor line, Entries, Last published (TEXT). Path `features/directory/components/product-result/`.

| Input | Type |
|---|---|
| `product` | `DirectoryProductSummary` (id, name, vendor, appTypes — from its entries, vendorEntryCount, independentEntryCount, reportCount, lastPublishedAt, useCaseEntryCount? — only with an active use-case filter) |

- `<li>` in the result `<ul>`. The product name is a link inside an `<h2>`, `title/medium`, primary, underlined.
- Shows **only facts**: vendor, app types, the number of entries per origin, the last publication date (`directory.result.*`); with an active use-case filter additionally `directory.result.useCaseLine`.
- **Forbidden:** tested ratio ("2 von 3 geprüft"), any fulfilment number, stars, badges like "empfohlen", or a comparison with the user's list. The directory list must not read as a ranking (D3, §8.4). The comparison only exists on S41.
- Card: `surface-container-lowest`, 1 px `outline-variant`, `corner-medium`, padding `--pa-space-md` / `--pa-space-lg`. The whole card is **not** clickable, only the name link.

### 4.10 `pa-contribution-item` — "Meine Beiträge" (S53)

Figma `2056:593` · properties Title, Status, Meta (TEXT), Show update, Show withdraw (BOOLEAN). Path `features/directory/components/contribution-item/`.

| Input / output | Type |
|---|---|
| `contribution` | `MyContribution` (entryId, productName, kind, displayName, profileKind, publishedAt, version, state, withdrawnAt?) |
| `(withdraw)` | output → opens S36 |
| `(republish)` | output → S55 in republish mode (only kind = test result) |

- `<li>` in `<ul>`, title `<h3>` (`contribute.mine.item.title`), status as text `contribute.mine.status` in a `surface-container` tag. Status is never colour only.
- "Eintrag ansehen" only while the entry is visible. "Neues Prüfergebnis veröffentlichen" only for Kanal-1 test results. "Veröffentlichung zurückziehen" only while visible.
- After withdrawal: focus moves to the item's status line; announce `publish.withdraw.done`.

---

### 4.11 `pa-rating-legend` — Erklärung der Bewertungsstufen

Figma `2099:760` · variant Expanded = false | true · 640 px (fills its container in screens).

| Input / output | Type |
|---|---|
| `expanded` | `model<boolean>(false)` |
| `values` | `AssessmentValue[]` (optional) — defaults to all seven; pass a criterion's `allowedValues` to show only those |

- Text button `assessment.legend.show|hide` with `aria-expanded` / `aria-controls`; the panel follows the button directly in the DOM. No tooltip, no dialog.
- Panel: `surface-container-low`, `corner-small`, padding `--pa-space-md`; `<dl>` with term (label/medium, 168 px column, stacks below 600 px) and explanation (`assessment.value.*.explain`, body/medium); footnote `assessment.legend.footnote` (body/small).
- **Once per page**, never inside every K6: S41 below the comparison/sort row (collapsed, also S41-M), S34b above the list (expanded until the first criterion has been assessed, then collapsed; state in `withSettings()`), S42 above the table (collapsed), S11 detail section "Wie wird bewertet?" (collapsed), S34 Überblick below the breakdown (collapsed).
- Texts are proposals pending review (Kriterienkatalog K-8, UX copy O19).

## 5. Screens

### 5.1 Designed screens

| ID | Figma (desktop / variants) | Route | Built from | Key behaviour |
|---|---|---|---|---|
| **S10** Kriterien zusammenstellen | step 1 `2019:407`, step 2 `2086:2142`, step 3 `2088:2200`, step 4 `2089:2259`, reset `2089:2692` | `/kriterien` | header, `mat-stepper` (linear, **4 steps**: Verwendungszweck, Anwendungstyp, Funktionsbereiche, Prüfverfahren (optional)), card with: step 1 `mat-autocomplete` (single choice); steps 2–4 checkbox group in `fieldset`; from step 2 a "Bisherige Auswahl" row (`surface-container-low`, text + text button "Ändern"); step 3 `mat-slide-toggle` "Erklärungen anzeigen"; actions Zurück (text) / Weiter (filled), step 4 primary "Kriterienliste anzeigen" | **Layers** (§5.4 below). Step header is display only (`aria-current="step"`). "Weiter" validates on click. Selection lives in `withConfigurator()` (works anonymously). Below 600 px: step indicator text instead of the horizontal stepper |
| **S11** Kriterienliste | `2020:70`, 320 px: `2039:2546` | `/kriterien/ergebnis`, `/kriterienlisten/:id` | header, title row, 2× `pa-notice` (info: priority note; attention: anonymous draft note), filter row (count text, search field, checkbox), criteria `<ul>` of cards (§5.5 below: checkbox + plain title, source title, short description, meta line, K5, "Details anzeigen" + "Notiz hinzufügen", expandable detail panel), "Geprüft nach …" cards for selected test types (priority B, deselectable, `criteria.item.testedBy.note` in the details), `pa-notice` info `criteria.list.noCriteriaFor` when `appTypesWithoutCriteria[]` is not empty, suggest link, sticky action bar | **Excluded card:** background `surface-container-low`, text "Nicht in Ihrer Liste" top right; never use opacity. **Search:** debounce 300 ms, client-side over the loaded release, announce `common.results.shown`. **Action bar:** status `criteria.list.status.count` + `criteria.list.unsaved`, buttons "Liste speichern" (outlined) and "Liste exportieren" (filled); sticky ≥ 600 px; `scroll-padding-bottom`. Live counter politeness polite, ≤ 1/s. "Auswahl ändern" → S10 prefilled. **Project copy (D1):** for the list of a project, `criteria.list.projectCopy` replaces "Auswahl ändern" and there is no share button. **Older release:** "Neue Liste mit derselben Auswahl" creates the copy (D6) and opens it with `criteria.list.copy.*` |
| **S13** Export dialog | `2021:225`, error: `2039:1944` | dialog | `MatDialog` 600 px, fieldset "Dateiformat" (radio), scope text, info notice (release / anonymous), attention notice (legal), actions | Initial focus: the selected radio (`cdkFocusInitial`). Esc / "Abbrechen" closes and returns focus to "Liste exportieren". **Error:** `pa-notice` error with title `criteria.export.error.title`, `role="alert"`, primary button becomes `common.actions.retry`. **429:** `errors.rateLimit` in the same notice. Successful download: `criteria.export.success` announced, dialog closes |
| **S32** Projekt – Übersicht | `2148:3877`, 360 px: `2148:4697` | `/projekte/:id` | header, breadcrumb, title row (H1 + outlined `projects.detail.edit`), meta line `projects.detail.meta`, card "Kriterienliste" (H2, text button `projects.detail.criteria.adjust`, `<dl>` list/release/scope/use case/app types/test types, text `projects.detail.criteria.copyNote`, info notice `projects.detail.criteriaLocked` once an assessment exists), card "Produkte in diesem Projekt" (H2, button `projects.detail.addProduct` with a dropdown indicator (`arrow_drop_down`) and `aria-haspopup="menu"`, opening a `mat-menu` with `.fromDirectory` / `.new`; order note; `<ul>` of products: H3 product name as the **only** link to S34, vendor line, `<dl>` tested / progress / counts / publication) | **Tested** `dd`: e.g. „Ja – 2 Prüfnachweise: BIT inklusiv Software-Test, BIK BITV-Test (Web)“. **Counts** `dd` (`projects.detail.row.counts`): label first, all seven values in fixed order („Erfüllt: 22 · Leichte Einschränkung: 3 · …“), each item `white-space: nowrap`, separators via CSS only. **Scope** `dd`: procedure criteria are named separately and never counted („46 Kriterien: 12 Mindestanforderungen (A), 28 Bewertungskriterien (B), 6 Hinweise – dazu 2 × „Geprüft nach …“ (zählen im Abgleich nicht mit)“). **Publication** `dd`: „Im Produktverzeichnis seit {{date}}“ plus the link `projects.detail.lookup`, or „Nicht veröffentlicht“. No sorting or filtering by counts (ARCHITECTURE D3). Without a list: `projects.detail.noCriteria` + `.noCriteria.cta` instead of the dl. Empty project: `projects.detail.empty`. **Below 600 px (S32-M):** products are separated by dividers (no nested outlined cards); title row, card headers and `<dl>` rows stack (dt above dd); the counts are a list with one item per line; actions follow their heading in the DOM. After adding criteria via S11, announce `projects.detail.criteriaAdded` on return. Figma example data: list „HR-Software 2027“ (46 requirement + 2 procedure criteria); BewerberPro (Ja, 2 evidences, 36 von 46 eingeschätzt, 22/3/6/2/1/2/10, published since 02.09.2026); PersonalPlus Recruiting (Nicht bekannt, 12 von 46, 8/1/2/1/0/0/34); TalentFlow (Nein, 0 von 46) |
| **S34a** Produkt im Projekt – Prüfnachweis | `2022:267`, with "Ja" and open list: `2094:2525` (S34a-J) | `/projekte/:id/produkte/:pid` (tab route `pruefnachweis`) | title row with outlined button `candidate.publish.cta` + hint, vendor meta, `mat-tab-nav-bar` (Überblick, Prüfnachweis, Einschätzung nach Kriterien), card with tri-state radio, then a list of evidence cards | Tabs are **routes** (`mat-tab-nav-bar` + `aria-current="page"`), so each tab has its own page title. Default value "Nicht bekannt". Details fields + private notes appear **after the radio group in the DOM** only when "Ja" is chosen. No focus jump, no announcement. While "Nicht bekannt": `candidate.evidence.details.hint` below the group, and `candidate.publish.cta.hint` under the publish button (`aria-describedby`). The **publish button stays enabled** and leads to S35. **Art der Prüfung** (S34a-J): `mat-select` with the `testType.*` list (five procedures, divider, "Andere Prüfung" with second line, "Eigene Prüfung ohne festes Verfahren"); "Andere Prüfung" inserts a required field `candidate.evidence.testType.otherName` directly after the select. No exclusion rules here. **Several evidences (v1.9, C46; layout v1.12):** an H2 `candidate.evidence.list.title` („Prüfnachweise“) precedes the cards. Each evidence is a card with H3 `candidate.evidence.list.item` and, in this order, Art der Prüfung, `candidate.evidence.testedBy`, `candidate.evidence.testDate` (hint `common.date.hint`), `candidate.evidence.version` (optional), `candidate.evidence.url` (optional), the private note `candidate.evidence.qualification` and the text button `candidate.evidence.list.remove` („Prüfnachweis {{n}} entfernen“; announce `.removed`, focus to the previous card's heading or the add button). Below the last card a text button `candidate.evidence.list.add` adds a card and moves focus to its "Art der Prüfung". A procedure already used is rejected with `candidate.evidence.list.duplicate`. Switching from "Ja" with evidences opens a confirmation dialog `candidate.evidence.list.statusChange`. `candidate.publish.cta.hint` shows only while the status is „Nicht bekannt“. Figma: S34a-L `2144:3508` |
| **S34** Produkt im Projekt – Überblick | `2097:2627` | `/projekte/:id/produkte/:pid` (default tab) | title row (publish button + hint), vendor line, `mat-tab-nav-bar` (Überblick active), four cards: "Produktangaben" (`<dl>`, text button Bearbeiten → S33, link to S41 when linked), "Stand der Bewertung" (two columns: Prüfnachweis status + button; Einschätzung progress + K1 scope workspace + `pa-rating-legend` + button), "Veröffentlichung" (status + explanation), "Nur für Ihre Organisation" (`surface-container-low`, private text, `<dl>`, Bearbeiten) | Facts and paths only, no verdict. Columns stack below 840 px. Private card is a separate `<section>` with H2 `candidate.overview.private.title`; nothing from it is ever part of a publication (PRV-02). When published: `candidate.published.*` replace the publication text. If the entry was published under another contributor profile than the user's, `candidate.published.otherProfile` replaces the button `candidate.published.republish`; withdrawal stays available (D3, PUB-15). `candidate.publish.cta.hint` only while the test status is „Nicht bekannt“ |
| **S34b** Einschätzung nach Kriterien | `2022:841` | tab route `einschaetzung` | intro, progress row (progress text, release info, filter checkbox), `pa-rating-legend` (expanded), list of K6 (only `allowedValues` per criterion; first card shows 11.2.1.1 without "Nicht anwendbar"), then read-only rows for „Geprüft nach …“: one per selected procedure in catalog order, title in the K6 criterion-title style, value line „Ja“ / „Nein“ / „Nicht angegeben“ (`assessment.procedure.derived`), hint `assessment.procedure.derived.hint`, text button `assessment.procedure.derived.edit` with a 48 px target and link semantics (navigates to S34a) (Figma `2144:4217`, 360 px `2144:4222`) | Procedure rows are never counted in progress or counts ("36 von 46" counts requirement criteria only). Progress text announced (throttled). Filter `candidate.assessment.filter.unassessed` hides assessed criteria and announces the count. Each K6 autosaves. Last saved state survives navigation (store) |
| **S35** Eintrag veröffentlichen – gesperrt | `2023:633` | `/projekte/:id/produkte/:pid/veroeffentlichen` | breadcrumb, H1, intro, attention notice (`publish.blocked.*`), filled button "Zum Prüfnachweis", next-steps text, text button back | Shown instead of the stepper while TestStatus = Unknown (SIG-10). "Zum Prüfnachweis" navigates to S34a and focuses the radio group. The unblocked stepper is shown in S35 step 2 (next row) |
| **S35** Eintrag veröffentlichen – Schritt 2 „Inhalt prüfen“ | `2053:2438` | same route, stepper step 2 | breadcrumb, H1, intro, `mat-stepper` linear (4 steps: Produkt zuordnen, Inhalt prüfen, Unter welchem Namen?, Bestätigen), step card (h2, two `surface-container-low` boxes "Das wird veröffentlicht" / "Das wird nicht veröffentlicht" each with h3 + `<ul>`, attention notice `publish.step2.checkNotes`, text button `publish.step2.editAssessments`), actions Zurück / Weiter | Completed steps: primary circle with check icon + hidden `common.stepper.done`. "Einschätzungen und Begründungen bearbeiten" → S34b; S34b then shows `publish.backToPublish`, which returns to step 2 with the stepper state kept (store). Steps 1, 3, 4 follow the same card pattern with the strings `publish.step1/3/4.*` |
| **S40** Produktverzeichnis – Suche | `2053:2051` | `/produkte` | header, H1, intro, search field, filter row in layer order (Verwendungszweck as `mat-autocomplete` single choice, then selects Anwendungstyp and Prüfverfahren), fieldset "Herkunft der Angaben" (2 checkboxes), checkbox "Mit Konformitätsbericht", results header (count + sort select), info notice `directory.results.orderNote`, `<ul>` of `pa-product-result` | Search debounced 300 ms, previous request cancelled; filters apply immediately and are reflected in the URL query (shareable, back button works). Count `directory.results` visible and announced. Sort only `directory.sort.*` (default Name here). With a use case selected (Figma S40-V `2142:3850`): **one** info notice with two paragraphs — `directory.results.useCaseNote` first, then `directory.results.orderNote` — instead of two notices, and each result shows its use-case line (`pa-product-result` property "Show use case line") (DIR-01); filters work on entry context, entries without a use case do not match. Empty state `directory.empty` + `directory.empty.suggest`. Pagination with `mat-paginator` only above 20 results. Filter "Prüfverfahren" uses the full `testType.*` list incl. "Andere Prüfung" / "Eigene Prüfung …" (request: `testTypeIds[]` and `testKinds[]`); experience reports never match a use-case filter (D5); exclusion rules between Verwendungszweck and Anwendungstyp/Prüfverfahren apply as in S10 |
| **S41** Produktseite | `2025:676`, 320 px: `2039:2139` | `/produkte/:id` | breadcrumb, H1, vendor + website link, entry count, info notice (no verdict), compare select + sort select, section "Angaben der Anbieterseite" (`h2`, list of `pa-entry-row`, one expanded `pa-entry-card`), section "Unabhängige Angaben" (`h2`, rows, "Erfahrungsberichte" `h3` with report cards, "Erfahrungsbericht schreiben") | Channels are **separate sections**, never interleaved, never merged (PRD §7). Compare select only when signed in with an organisation; changing it reloads the entries (configuration-aware variant), keeps focus on the select, announces `directory.product.compare.updated`. Sort select: options `directory.sort.*`, default "Zuletzt veröffentlicht", announces `directory.sort.updated`. **Never** a sort by matching (D3). Website link in the same tab. **E13:** experience reports appear **only** under "Erfahrungsberichte" as report cards (never also as `pa-entry-row`); the card shows `contribute.report.typeNote` instead of K4 and, if the author assessed criteria, a disclosure `directory.product.reports.assessment` with a plain `<dl>` (`directory.product.reports.assessment.list`) — **no K1, no counts** (ARCHITECTURE_v11 C33); the report's context line shows app types only (`directory.entry.context.contributorDeclared`, D5). Entry count `directory.product.entryCount` counts reports separately. A vendor entry without assessments shows `signals.fulfillment.none` instead of counts in its row and card (D2) |
| **S42** Eintrag – Detail | `2054:2542` | `/eintraege/:id` | breadcrumb, H1 `directory.entry.title`, full `pa-entry-card` (without its own "Eintrag ansehen" / details buttons), section h2 "Einschätzung je Kriterium" with intro (= table caption), `pa-rating-legend` (collapsed), select "Anzeigen", `mat-table` (Kriterium / Einschätzung / Begründung), text button `directory.entry.previousVersion` | Public view: K1 scope Public; the notCovered row shows 0 (projection against the entry's own scope) and the table has no „Damals nicht im Katalog“ rows. Entry without assessments (D2): `signals.fulfillment.none` instead of K1, and the section „Einschätzung je Kriterium“ is omitted. With a comparison list (signed in with organisation): K1 scope Configuration, table limited to the list's criteria, plus rows "Damals nicht im Katalog" with `directory.entry.notCovered.rationale`. Rows in catalog order, never sortable by assessment. Filter announces the row count. Empty rationale = empty cell |
| **S51** Erfahrungsbericht | `2026:1036`, error: `2039:1803` | `/produkte/:id/erfahrungsbericht` | breadcrumb, H1, intro, form card (required note, profile select, textarea, AT field, version field, app-type field `contribute.report.appType` (optional, multi), "Zusätzlich einzelne Kriterien einschätzen" → K6 list, consent checkbox (always) with its hint), actions | **Labels (v1.12):** on every width the long questions are visible labels above the fields with short `mat-label`s inside (pattern of S51-M; `contribute.report.*.short`). **App types (D5):** optional and always offered, directly after the version field; they decide the criteria and allowed ratings of the K6 list (all six levels without a statement); experience reports have no use case. **Consent (v1.9, C48; hint v1.12):** the checkbox `contribute.report.consent` is always shown, required, placed directly before the submit area with the single hint `contribute.report.consent.withdraw` (public visibility, withdrawal, deletion of text and assistive-technology details); there is no separate public-visibility notice. The consent-text version is sent with the request. Figma S51, S51-E, S51-M and S51-M2 show the consent at the end of the form (v1.10). Validation only on submit (§6.4). Submit carries a `clientRequestId` per form instance (PUB-06). Rate limit → `contribute.report.rateLimited` in an error notice |
| **S53** Beitragen – Meine Beiträge | `2056:2334` | `/beitragen` | header (SignedInNoOrg), H1 "Beitragen", intro, h2 "Meine Beiträge", `<ul>` of `pa-contribution-item`, link `contribute.mine.vendorCta` | Lists all contributions of the signed-in account across its contributor profiles; visible only to that account. Withdrawn items stay in the list with status text and without "Eintrag ansehen". Withdraw → S36 dialog. Empty state `contribute.mine.empty` + `contribute.mine.cta` |
| **S37** Bearbeitungskonflikt | `2056:2519` | dialog | `MatDialog` (`role="alertdialog"`) with title, body, attention notice `conflict.reload.hint`, actions "Neue Fassung laden" (text) and "Meine Änderungen kopieren" (filled, **initial focus**) | Opened on 409 when saving (API-03). "Meine Änderungen kopieren" copies the user's unsaved input as plain text to the clipboard, announces `conflict.copied`, dialog stays open. "Neue Fassung laden" discards, reloads, focuses H1. Esc only closes; saving stays blocked until reload |
| **S55** Prüfergebnis veröffentlichen (steps 1, 2, 4) | step 1 `2145:3653`, step 2 `2145:4312`, step 4 `2027:1125`, error: `2039:1644`, 320 px: `2039:2739` | `/beitragen/pruefergebnis` | H1, intro, `mat-stepper` linear (5 steps), step card (title `h2`, intro, required note, 2×2 fields, consent checkbox, info notice), actions | Step change moves focus to the step heading and updates the page title (`pageTitle.vendor.step`). Date field: `mat-datepicker` with free text input, hint `common.date.hint`, toggle name `common.date.openCalendar`, error `errors.field.dateFuture`. Consent data is never prefilled on republish (PUB-08). Below 600 px: step indicator text, fields stacked. **Stepper:** completed steps show a check icon (+ hidden `common.stepper.done`); only the current step label is bold. **Step 1** (S55-1), in this order: Produktname (`vendor.step1.search`) → match choice directly below it (`vendor.step1.matches` + radio list incl. `vendor.step1.new`) → Anbieter, Webseite → Verwendungszwecke (`vendor.step1.useCases`; optional, multi, chips, searchable; before the app types because a use case can exclude app types, as in S10) → Anwendungstypen (required checkbox fieldset; the question `vendor.step1.appTypes` is the `legend` inside the group; excluded types stay visible, `aria-disabled` with their reason) → info notice `vendor.step1.context.hint`. The secondary action of step 1 is „Abbrechen“ (`common.actions.cancel`), not „Zurück“. **Step 2** (S55-2): intro `vendor.step2.intro`, radio group `vendor.step2.tested`, rule hint `vendor.step2.rule` above the cards, then one card per test evidence as in S34a (H3 `candidate.evidence.list.item`; `vendor.step2.testType`, `vendor.step2.testedBy` with hint, `vendor.step2.date` with `common.date.hint`, `vendor.step2.version`, `vendor.step2.url`; remove button `candidate.evidence.list.remove`; no private note), add button `vendor.step2.add`. **Step 3:** without criteria the entry shows `signals.fulfillment.none` instead of K1 (D2); with criteria the product parts are required |

### 5.2 States shown in Figma

| State | Figma | What to take from it |
|---|---|---|
| Error after submit (form) | S51-E, S55-E | Error summary at the top of the form / step, field errors, checkbox error state. Pattern in §6.4 |
| Error in dialog | S13-E | Error notice above the actions; retry |
| 320 px / 400 % zoom | S41-M, S11-M, S55-M, S10-M | Rules in §2.2; S10-M: stepper → text line `criteria.select.stepLabel`, "Bisherige Auswahl" stacked, actions stacked with the **primary last** (DOM order = visual order, §2.2) |
| Error after "Weiter" without a selection | S10-E | Field error only (one field): focus into the field, `aria-invalid`, `mat-error` linked; page title prefix "Fehler:" |
| Reset after a higher layer changed | S10-R | Attention notice, all lower layers empty (§5.4) |
| Excluded options | S10 steps 2 and 4 | Disabled look + reason line (§5.4) |
| Focus | Komponenten → focus doc | §1.6 |
| Blocked flow | S35 | Explain why plus one clear action instead of a disabled UI |

### 5.3 Screens without a Figma design

Build these from Angular Material defaults, the foundations above and the strings in UX copy v14. Reuse the patterns named.

| Screens | Pattern to reuse |
|---|---|
| S01–S03 auth, S20 organisation, S50 profile, S52 product request, S60 settings, S61 account | Form card as in S51 (`common.requiredNote`, outlined fields, hints, error summary §6.4), actions bottom right (stacked below 600 px) |
| S04 home | Content column, H1, intro, **four** entry cards (`home.card.criteria|directory|projects|contribute.*`), then the text link `home.card.vendor.text` → S55, then `home.disclaimer` |
| S12, S15, S31, S36 dialogs | S13 dialog rules (600 px, initial focus, return focus, error notice). S31 shows `projects.create.criteria.hint` (the project gets its own copy of the list, D1) |
| S16 shared list (D7) | S11 read-only: no checkboxes, no K5 menu, no action bar; header in the anonymous state; strings `criteria.shared.*`; readable without an account (TEN-09) |
| S14, S30 lists | `mat-table` or `<ul>` of cards. Sorting only by name/date/count. Results announced (`directory.results`, `common.loaded`) |
| S33 candidate form | S34a title row and tabs; private section visually separated (`candidate.form.privateSection`) |
| S35 steps 1, 3, 4 | S35 step 2 and S55 step patterns |
| S80 error pages | H1 + text + `errors.home` button; page title = H1 |
| Footer | Not designed: simple `<footer>` with a link list (`shell.footer.*`) on `surface-container`, body/medium links, wraps on narrow screens |
| Session warning (A11Y-04) | `MatDialog` with `role="alertdialog"`, focus on `shell.session.warning.stay`; the shell reads `expiresAt` and `absoluteExpiresAt` from `GET /auth/session` at start and from the response headers `X-Session-Expires-At` / `X-Session-Absolute-Expires-At` afterwards; "Angemeldet bleiben" calls `POST /auth/session/extend` (Identity cookie session, ARCHITECTURE_v11 C50, C60). Warning 5 minutes before the earlier of the idle limit (30 min) and the absolute limit (8 h); before the absolute limit show `shell.session.warning.absolute` without the extend button (C52) |
| S60 settings (v1.13) | Form card as in S51; fieldsets „Sprache“ (only Deutsch, hint `settings.language.hint`), „Erscheinungsbild“ (`settings.appearance.*`, radio group), „Bewegung und Animationen“, „Kontrast“ (only the system option until H2); changes apply immediately and announce `settings.saved` |
| S71 legal pages (v1.13) | Content column, H1 `legal.*.title`, Markdown rendered with real headings and lists; links in the footer |
| S81 administration (v1.13, confirmed SPEC SD22) | `<ul>` of request cards (`admin.requests.*`) with accept / reject (reason dialog as S13); merge dialog as S13 with a product search like S40 |
| S21 members, invitations (v1.13) | `mat-table` (desktop) / `<ul>` (base) per §2.2; role select per member; errors `org.members.lastOwner` in an error notice above the table |
| E-mail confirmation (v1.13) | `pa-notice` Attention `auth.confirm.required` with button `auth.confirm.resend` where a gated action was refused (403 `email-unconfirmed`) and on S61; result pages `auth.confirm.done|invalid` as S80 |
| Removing a candidate (v1.13, confirmed SPEC SD21) | Text button `candidate.remove` at the end of the „Produktangaben“ card in S34; confirmation dialog as S36 with `candidate.remove.*`; afterwards S32 with H1 focus and `candidate.removed` |
| Product page additions (v1.13) | S41 title row: text buttons `directory.product.copyLink` and, for Editors/Owners, `directory.product.addToProject` (project chooser dialog as S13); S42 with `?fassung=<n>`: `pa-notice` Info `directory.entry.olderVersion` above the card |
| Login overlay after 401 (C60) | `MatDialog` with the S01 form (`auth.login.*`) and the hint `shell.session.expired.title|body`, opened by the interceptor without navigation; the page and its form input stay underneath; after sign-in the failed request is retried and `shell.session.restored` is announced; closing the dialog leaves the user on the page, signed out |

### 5.4 S10 filter layers, exclusions and reset

Source of truth: `kriterienkatalog_bit_inklusiv_v1.md` §2–§3, ARCHITECTURE_v11 §5.1. Invariants CFG-01–CFG-04, CAT-11/12.

| Layer | Control | Rules |
|---|---|---|
| 1 Verwendungszweck | `mat-form-field` + `matInput` + `mat-autocomplete` (`role="combobox"`), single choice | Filters while typing; "Keiner davon passt" is always the last option (also when filtering), separated by a divider, with second line `criteria.select.step1.none.hint`. Match count announced (`criteria.select.step1.matches`). Active option via `aria-activedescendant`, active background `secondary-container` |
| 2 Anwendungstyp | checkbox group (`fieldset`/`legend`) | Multi. Hint lines for offen/geschlossen (`criteria.select.step2.*.hint`) linked with `aria-describedby` |
| 3 Funktionsbereiche | checkbox group | Multi. `Basisfunktionalität` checked + `aria-disabled="true"`, description "Immer ausgewählt." Toggle "Erklärungen anzeigen" (`mat-slide-toggle`, default off) shows the catalog explanation under each option (body/medium, `on-surface-variant`) and links it with `aria-describedby` while visible |
| 4 Prüfverfahren (optional) | checkbox group | Multi, may stay empty. Hint line per option with link to the procedure |

**Excluded options** (catalog `FilterExclusion`): stay visible in place, checkbox `State=Disabled` look, label in `on-surface-variant`, reason line below (`criteria.select.excluded.*`). Use `aria-disabled="true"` — **not** the `disabled` attribute — so the option stays focusable and its reason is read. Toggling it does nothing. If both the use case and an app type exclude a value, the reason names both.

**Reset** (S10-R): when the user changes a higher layer (via "Ändern" or Zurück), all selections of the lower layers are cleared except `Basisfunktionalität`. Show `pa-notice` Kind=Attention with `criteria.select.reset.title` and the text of the changed layer (`criteria.select.reset.text.useCase|appType|productPart`) at the top of the step card, `role="status"` (polite), no focus move. The notice disappears as soon as the user selects something again. Never silently keep a lower selection that has become invalid.

**Links in option hints (step 4):** link text "Verfahren ansehen ({{domain}})", opens in a new tab (`common.newTab`); a separate tab stop after the checkbox.

**Data flow:** the filter-options endpoint (ARCHITECTURE_v11 §7.2, shape final in SPEC_v1) returns each layer's values with `excludedBy[]`; the component computes `unavailableOptionIds()` from the current higher-layer selection (as in the existing `filters-selection` component). The server validates the final selection again (CAT-12).

### 5.5 S11 criterion card: compact and detail

| Part | Compact (default) | Detail (expanded in place) |
|---|---|---|
| Title row | checkbox + plain-language title (`PlainLanguageDe` title), **H2** (sections in the detail panel are H3) | same |
| Source title | `criteria.item.sourceTitle` — "11.2.1.1 Ohne Maus nutzbar · BIT inklusiv", body/small | same |
| Short description | `PlainLanguageDe` | same |
| Meta line | `criteria.item.meta` — "Basisfunktionalität · EN 301 549 11.2.1.1" | same |
| Priority | K5 | same |
| Actions | `criteria.item.showDetails` + "Notiz hinzufügen" (text buttons) | `criteria.item.hideDetails` |
| Detail panel | – | `surface-container-low`, `corner-small`, padding `--pa-space-md`; sections with `<h3>` (title/medium; the criterion title is `<h2>`): **Warum diese Priorität?** (rationale, matched rule, source; K5 in the card has `showRationaleToggle=false`) · Was wird verlangt? · Wann gilt das? · Wen betrifft es? · Woran erkennt man es? (So ist es gut / So ist es nicht gut; first three examples, then `criteria.item.detail.examples.more`) · Wie wird bewertet? · So wird es geprüft (link to the BIT inklusiv anchor, opens in a new tab with `common.newTab` hint) · Gilt für · source line |

- The panel's id is the target of `aria-controls`; the toggle keeps focus. Each criterion has an anchor (`#k-11-2-1-1`) so links can point at it.
- With several matching procedures (e.g. Website + Mobile Anwendung): `mat-tab-group` "Software-Test / App-Test / PDF-Test" inside the panel; sections with identical text are shown once above the tabs.
- Texts in the panel are catalog content (`CriterionProcedureText`), verbatim, never edited in the UI.
- "Geprüft nach {{testType}}" items (from S10 step 4) use the same card with meta line `criteria.item.testedBy.meta`, priority B (K5, changeable like any other), the checkbox to deselect them, and a short detail panel with `criteria.item.testedBy.text` and `.note` only.

---

## 6. Global states and interactions

### 6.1 Interaction states

Use Material's state layers (hover 8 %, focus via strong indicators, pressed 10 %). Do not customise them.

| Element | Hover | Focus | Pressed | Disabled |
|---|---|---|---|---|
| Buttons (filled, outlined, text) | Material state layer | §1.6 ring | ripple | **Avoid.** If an action is not available yet, keep it enabled and explain on activation (S34a → S35), or use `aria-disabled="true"` + `aria-describedby` with the reason. Never `disabled` without a visible reason |
| Links | underline stays; no colour change needed | ring | – | – |
| Cards (criterion, entry) | **no hover effect**; the card itself is not clickable | – | – | – |
| Radio, checkbox | Material | ring | Material | Material |

### 6.2 Loading

- Initial page load of data: `mat-progress-bar mode="indeterminate"` at the top of the content column, announce `common.loading` once (via the shared `withLoading()` counter), then `common.loaded` with the count where a count exists. **No skeleton screens.** They read badly with screen readers and imply layout that may not come.
- Button actions (save, publish, export): the button keeps its label, shows a 20 px `mat-progress-spinner` before the label, gets `aria-disabled="true"` until done, and announces the result.
- Autosave (K6): no spinner; the announcement is the feedback.

### 6.3 Empty states

Text + one action, never an illustration. Strings: `criteria.saved.empty`, `projects.empty`, `projects.detail.empty`, `directory.empty`, `directory.product.vendor.empty`, `directory.product.independent.empty`, `contribute.mine.empty`. In S41, an empty channel section **keeps its H2** and shows the empty text. The section never disappears (both channels always visible).

### 6.4 Form errors (E11, E12)

Figma: S51-E, S55-E.

1. Validate **on submit** only (and on blur for format errors such as dates, without announcing).
2. On a failed submit:
   - field: `aria-invalid="true"`, `mat-error` (body/small, `--mat-sys-error`) linked via `aria-describedby` (Material does this);
   - error summary at the top of the form or step: `pa-notice kind="error"`, title `errors.summary.title(.one)` as `<h2>`, one line per error `errors.summary.item` = **field label (link to `#field-id`) + ": " + the identical field error text**;
   - **focus moves to the summary** (`tabindex="-1"`), and the page title gets `shell.pageTitle.error`.
3. Clicking a summary link focuses the field.
4. The summary updates on the next submit. It does not change live while the user is typing.
5. Server errors (Problem Details `errors` map, ARCHITECTURE §7.1) use the same rendering. Unknown field → summary line without link, `errors.field.server`.
6. Checkbox errors: Material checkbox "error" type + `mat-error` text below the label (S51-E, S55-E).

### 6.5 Other system states

| Situation | Behaviour | Strings |
|---|---|---|
| 409 conflict | Dialog S37 (§5.1) | `conflict.*` |
| 429 rate limit | Error notice in context (dialog or form) | `errors.rateLimit`, `contribute.report.rateLimited` |
| Offline | Attention notice at the top of `main`, `role="status"` | `errors.offline` |
| 404 / 403 / 500 | S80 pages (404 also for foreign resources, TEN-03) | `errors.*` |
| Session about to expire | Shell alertdialog | `shell.session.*` |
| 401 (session ended) | Login overlay without navigation, request retried after sign-in (§5.3) | `auth.login.*`, `shell.session.expired.*`, `shell.session.restored` |
| Catalog release changed during export | Error notice in S13 | `criteria.export.outdated` |

---

## 7. Accessibility specification (summary of 7f)

The full rationale is in `accessibility_review_procureaccess_v1.md` (round 1) and `accessibility_review_procureaccess_v2.md` (round 2). The per-screen details are in the Figma annotations (category "Barrierefreiheit").

| Topic | Rule |
|---|---|
| Landmarks | Skip link → `header` → `nav aria-label="Hauptnavigation"` → `main#main` → `footer`. One `main` per page |
| Headings | One `h1` per page (= page subject). S11: criterion titles `h2`. S41: channel sections `h2`, entries `h3`, "Erfahrungsberichte" `h3`. K1/K2 region titles `h3` in cards, `h4` inside an entry card that already has an `h3` |
| Page titles | `pageTitle.*` keys (UX copy v14 §5.16), most specific first |
| Route change | Focus H1, announce title (A11Y-03) |
| Keyboard | Everything reachable by Tab in visual order. Radio groups and tabs: one tab stop, arrow keys inside. Menus: Esc closes and returns focus. Dialogs trap focus and return it |
| Target size | ≥ 48 × 48 px for all controls (density 0). Breadcrumb links: ≥ 24 px high (body/medium + padding-block) |
| Names | Repeated controls get context via `aria-describedby` (criterion title or entry source): "Warum diese Priorität?", "Notiz hinzufügen", "Empfehlung wiederherstellen", "Details anzeigen", "Was bedeutet das?", "Eintrag ansehen" |
| Live regions | Only through `A11yAnnouncerService`. Keys: `common.results.shown`, `criteria.list.status.count`, `criteria.priority.changed`, `candidate.assessment.saved`, `directory.product.compare.updated`/`off`, `directory.sort.updated`, `criteria.export.success`, `criteria.share.copied`, `shell.session.extended`, `shell.session.restored` |
| Colour | Never the only carrier (notice icons + prefixes, active nav border) |
| Zoom / reflow | §2.2; test at 200 % and 400 % |
| Motion | §1.7 |
| Language | `<html lang>` from i18n state; English proper names (VPAT) need no `lang` change |

**Testing hooks:** the Playwright + axe run (TESTING_v10 §8.2 stage 6) covers what can be automated. These manual checks are part of TESTING_v9 §7.2:
- error summary focus with NVDA, JAWS and VoiceOver;
- `pa-entry-row` heard in Wide and Narrow;
- autosave announcement frequency;
- datepicker text input;
- sticky bar at 200 %;
- compact menu at 400 %.

---

## 8. Content specification and edge cases

### 8.1 Text handling

- **No truncation (ellipsis) anywhere in v1.** All text wraps. This applies especially to rule-carrying text (counts, source labels, tested status, criterion titles) and to product, vendor and display names.
- German strings are long. Layouts are built with the German strings; English must fit (UX copy §7).
- Numbers and dates are formatted in code with the active locale (`de-DE`: `14.05.2026`, thousands separator `.`). Counts always come with their label or noun („Erfüllt: 51 Kriterien“, „Erfüllt: 51“), never a bare number (UX copy G6).
- Pluralisation: ICU or separate keys (UX copy O9). It must be decided before the first count is rendered.

### 8.2 Limits (confirmed in SPEC_v2 §7.2)

| Field | Max | Behaviour at limit |
|---|---|---|
| Display name (contributor profile) | 60 chars | `errors.field.maxLength` on submit; counter hint from 50 chars |
| Experience report text | 5 000 chars | same; `mat-hint align="end"` counter from 4 500, announced only on blur |
| Criterion note / assessment note | 1 000 chars | same |
| Project name, list name | 120 chars | same |

Confirmed and extended in `SPEC_v2.md` §7.2 (more fields there, e.g. AT context, candidate fields, URLs); the DTO validation uses the same values.

### 8.3 Edge cases

| Case | Behaviour |
|---|---|
| Experience report without assessments | No K1 and no counts (C33); the card has no `directory.product.reports.assessment` disclosure |
| Test-result entry with all counts 0 | Show all eight with 0. In the row, show „…: 0“ for all seven plus the notCovered tag. Never hide the block |
| Vendor or auditor entry without assessments (D2) | `fulfillment: null`: no K1 and no counts; `signals.fulfillment.none` in the Abgleich region of the card and in the row (PUB-14) |
| Criterion allows only some ratings | K6 renders only `allowedValues` (union over the selected app types) + Nicht eingeschätzt; S42 table can still show any stored value |
| Only Hardware and/or Vermittlungs-/Notrufdienst selected | S11 shows `criteria.list.noCriteriaFor` (info, not an error); "Geprüft nach …" items, if selected, are still listed |
| No procedure text for a selected app type (Hardware, Vermittlungs-/Notrufdienst) | Detail panel shows only the sections from ProcureAccess (Gilt für, priority); no empty headings |
| `notCovered` = 0 | Row: tag „Damals nicht im Katalog: 0“ still shown. K1 (Scope Public/Configuration): the notCovered line shows 0, the explanation is hidden (variant NotCovered=None). K1 Scope Workspace never has the line |
| Entry release ≠ comparison list release | `signals.fulfillment.release` line in K1 |
| No entries in a channel | Section with H2 + empty text (§6.3) |
| Signed in without organisation | No compare select, no "Projekte". `directory.product.compare.login` is **not** shown (it is for anonymous users only); show nothing instead |
| Anonymous on S11 | Attention notice `criteria.list.draftNote`; "Liste speichern" → login with `auth.login.reasonSave`, selection kept |
| Very long criteria lists (48+ in S11/S34b) | No pagination, no virtual scrolling (breaks find-in-page and screen readers). Filters reduce the list |
| Slow connection | Search requests are debounced (300 ms) and the previous one is cancelled (`switchMap`). Loading pattern §6.2. Autosave retries once, then shows an error notice with retry |
| Missing optional data (tester, date, report) | Omit the part. Never print placeholders like "–" or "unbekannt" (exception: tested status, which is always Ja/Nein) |
| Product or display name with 80+ characters | Wraps; layout must not break (test with a 120-character name) |
| Double submit (publish, report) | `clientRequestId` per form instance (PUB-06); button `aria-disabled` while pending |

---

## 9. Open items for implementation

| # | Item | Owner | Before |
|---|---|---|---|
| H1 | Verify the generated Angular palette against §1.2; pin deviations with `mat.theme-overrides` | Philipp | Migration step 1 (theme) |
| H2 | Design and review a high-contrast variant for `settings.contrast.high` (A11Y-06) | Philipp | Before pilot |
| ~~H3~~ | ~~Dark theme: design + review, or out of scope~~ **decided (SPEC SD11):** in v1; follow-up work in H20 | – | – |
| H20 | Dark mode for the Figma variables and designed screens; accessibility review of the dark values; pin deviations (§1.1) | Philipp, accessibility tester | Release (SPEC O-S5) |
| H21 | Optional Figma frames for the new screens without design (§5.3: settings appearance, administration, organisation members, removal dialog) | Philipp | Optional |
| H4 | Footer and session warning are specified only in text (§5.3). Add them to Figma if a visual check is wanted | Philipp | Optional |
| H5 | ~~Conflict dialog S37~~ designed (`2056:2519`) | – | – |
| ~~H10~~ | ~~S55 steps 1 and 2 not designed~~ **done (v1.10):** S55-1 `2145:3653`, S55-2 `2145:4312`. Former text: reuse S34a-J (Art der Prüfung select incl. "Andere Prüfung"); S55 step 1 fields for app types and use cases not designed either | Philipp | Migration step 6 (Flow E is built there) |
| H11 | ~~Exclusion HR-Anwendung → Mobile Anwendung~~ decided: no exclusion; remove seed rows | – | – |
| H12 | Hardware and Vermittlungs-/Notrufdienst have no test procedure yet (open to-do, Kriterienkatalog K-2) | Projektteam | open |
| H13 | Review the rating-level explanations (`assessment.value.*.explain`; = ARCHITECTURE_v11 O-16, UX-O19) | Projektteam | SPEC_v1 |
| ~~H18~~ | ~~K4/K2 show a single evidence line~~ **done (2026-10-13):** K4 `2009:103` has an evidence list (properties `Evidence 1`, `Evidence 2`, `Show evidence 2`; the code renders all evidences); K2 default lists all procedures; S42/S42-M show two evidences | – | – |
| H19 | S55-1 use-case field: the file has no chip component; Figma shows plain text. Build as `mat-chip-grid` + autocomplete — see the Development annotation on node `2145:3813` | Implementation | Migration step 6 |
| H9 | ~~Decision E13 vs. SIG-03/SIG-11~~ done: ARCHITECTURE_v11 C28, TESTING_v9 SIG-12; since v1.8 also no counts (C33, SIG-14) | – | – |
| ~~H14~~ | ~~K6 Ja/Nein form~~ obsolete: procedure criteria are derived (v1.9) | – | – |
| ~~H15~~ | ~~Procedure answers vs. test evidence~~ decided: derived from test evidences (ARCHITECTURE_v11 C46) | – | – |
| ~~H16~~ | ~~S34a list and S51 consent not in Figma~~ **done (v1.10):** S34a-L `2144:3508`; consent moved to the end of the form in S51, S51-E, S51-M, S51-M2 | – | – |
| ~~H17~~ | **done (v1.11):** S32 `2148:3877`, S32-M `2148:4697`. Former text: Project page (S32, not designed): once assessments exist, `projects.detail.criteriaLocked` replaces "Kriterienliste wechseln"; "Kriterienliste anpassen" stays and leads to S11 for the assigned list; after adding criteria announce `projects.detail.criteriaAdded` (ARCHITECTURE_v11 C51) | Philipp | SPEC_v1 |
| ~~H6~~ | ~~Field length limits~~ **done:** SPEC_v1 §7.2 | – | – |
| H7 | Pluralisation approach (UX copy O9) | Implementation | Migration step 1 |
| H8 | ~~Code Connect mapping (step 7h)~~ Done without Code Connect (Figma Professional plan): documentation links + usage in component descriptions, see `figma_code_mapping_procureaccess_v1.md` | – | – |

---

## 10. Traceability: Figma nodes

| Item | Node |
|---|---|
| K1 `pa-fulfillment-breakdown` | `2014:341` |
| K2 `pa-documentation-signal` | `2013:233` |
| K3 `pa-source-label` | `2010:348` |
| K4 `pa-tested-status` | `2009:103` |
| K5 `pa-priority-indicator` | `2011:203` |
| K6 `pa-assessment-input` | `2012:2430` |
| `pa-entry-card` | `2015:378` |
| `pa-entry-row` | `2033:616` |
| `pa-notice` | `2032:486` |
| `pa-app-header` | `2017:650` |
| `pa-app-header-compact` | `2039:2131` |
| `pa-product-result` | `2053:593` |
| `pa-contribution-item` | `2056:593` |
| Focus spec | `2039:515` |
| S10 steps 1–4 / S10-R | `2019:407` / `2086:2142` / `2088:2200` / `2089:2259` / `2089:2692` |
| S11 / S11-M | `2020:70` / `2039:2546` |
| S10-M | `2103:2940` |
| S10-E (step 1 error) | `2139:3332` |
| Mobile-first base (360 px): S42-M / S40-M / S10-M1 (viewport) / S34-M / S13-M / S34b-M / S51-M / S51-M2 (viewport) | `2132:3382` / `2134:2905` / `2134:3174` / `2134:3184` / `2134:3825` / `2136:3127` / `2136:4005` / `2136:4187` |
| `pa-entry-card` Narrow variants / K3 Narrow variants | `2132:3174`, `2132:3249` / in set `2010:348` |
| K1 variant Scope=Workspace | `2101:3180` |
| S11 criterion detail panel | `2091:2387` |
| S13 / S13-E | `2021:225` / `2039:1944` |
| S32 / S32-M | `2148:3877` / `2148:4697` |
| S34 Überblick / S34a / S34a-J / S34a-L (several evidences) / S34b | `2097:2627` / `2022:267` / `2094:2525` / `2144:3508` / `2022:841` |
| S34b derived „Geprüft nach …“ rows (desktop / 360 px; one per procedure) | `2144:4217`, `2151:4493` / `2144:4222`, `2151:4505` |
| `pa-rating-legend` | `2099:760` |
| S35 blocked / step 2 | `2023:633` / `2053:2438` |
| S37 | `2056:2519` |
| S40 / S40-V (use case selected, v1.8) | `2053:2051` / `2142:3850` |
| S42 | `2054:2542` |
| S53 | `2056:2334` |
| S41 / S41-M | `2025:676` / `2039:2139` |
| S51 / S51-E | `2026:1036` / `2039:1803` |
| S55 step 4 / S55-E / S55-M | `2027:1125` / `2039:1644` / `2039:2739` |
| S55-1 step 1 (app types, use cases) / S55-2 step 2 (several tests) | `2145:3653` / `2145:4312` |

Link format: `https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=<id with "-" instead of ":">`.
