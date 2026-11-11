// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * Priority levels of a criterion as the catalog sends them; each picks `criteria.priority.<level>`
 * (SPEC §8.2, UX copy §5.5). Shared, because K5 shows them in the configurator and the workspace.
 */
export type PriorityLevel = 'A' | 'B' | 'Info';

export const PRIORITY_LEVELS: readonly PriorityLevel[] = ['A', 'B', 'Info'];

/**
 * The text of the rule that gave a priority (`criteria.priority.rule.*`, CAT-02), with the names
 * it needs already filled in by the caller, who knows the catalog's names.
 */
export interface PriorityRuleText {
  key: string;
  params: Readonly<Record<string, string>>;
}
