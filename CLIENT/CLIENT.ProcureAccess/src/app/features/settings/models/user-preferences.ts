// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

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
