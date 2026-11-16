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
 * A signed-in user's settings, the answer of `GET /api/v1/account/preferences` (SPEC §8.12). The
 * language is German in v1 (SD12): there is nothing to apply.
 */
export interface UserPreferences {
  language: 'de';
  appearance: AppearanceSetting;
  motion: MotionSetting;
  contrast: ContrastSetting;
}

/** The settings of a user who never saved any (SPEC §8.12): German, and everything as the system says. */
export const SYSTEM_PREFERENCES: UserPreferences = { language: 'de', appearance: 'system', motion: 'system', contrast: 'system' };
