// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Types only and the route path, so app.routes.ts can import the path without loading a store or
// service (CLAUDE.md §3, client routing).

/** S60 „Einstellungen“ at `/einstellungen` (UX copy §4, SPEC R-SET-01). */
export const SETTINGS_PATH = 'einstellungen';

/** Light, dark, or as the system says (SPEC §7.5, R-SET-01). */
export type AppearanceSetting = 'system' | 'light' | 'dark';

/** Motion as the system says, or reduced (Handoff §1.7). */
export type MotionSetting = 'system' | 'reduced';

/** Contrast as the system says, or high; the API refuses `high` until H2 (owner decision S5-25 3). */
export type ContrastSetting = 'system' | 'high';

/**
 * The rating legend on the assessment tab S34b (SPEC R-CAN-08, Handoff §4.11; owner decisions S6-19
 * Q1 (b), S6-19b (a)): `auto` — expanded from 600 px until the user's first assessment, which
 * stores `collapsed` —, or what the user chose by opening or closing it.
 */
export type AssessmentLegendSetting = 'auto' | 'expanded' | 'collapsed';

/**
 * A signed-in user's settings, the answer of `GET /api/v1/account/preferences` (SPEC §8.12). The
 * language is German in v1 (SD12): there is nothing to apply. S60 shows the first four; the fifth
 * is S34b's (S6-19b) and travels with every save.
 */
export interface UserPreferences {
  language: 'de';
  appearance: AppearanceSetting;
  motion: MotionSetting;
  contrast: ContrastSetting;
  assessmentLegend: AssessmentLegendSetting;
}

/** The settings of a user who never saved any (SPEC §8.12): German, and everything as the system says. */
export const SYSTEM_PREFERENCES: UserPreferences = {
  language: 'de',
  appearance: 'system',
  motion: 'system',
  contrast: 'system',
  assessmentLegend: 'auto',
};
