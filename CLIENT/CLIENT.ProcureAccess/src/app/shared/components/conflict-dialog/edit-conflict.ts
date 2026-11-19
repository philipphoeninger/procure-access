// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { problemDetails, problemTypeName } from '@app/core/errors/problem-details';

/** One changed field of a form as „Meine Änderungen kopieren“ copies it (UX copy U189). */
export interface ChangedField {
  /** The field's label as the form shows it. */
  label: string;
  /** The input; for a choice the text of the chosen option. */
  value: string;
}

/** A group of changed fields under a heading, e.g. one evidence card („Prüfnachweis 2: …“). */
export interface ChangedGroup {
  heading?: string;
  fields: readonly ChangedField[];
}

/**
 * The user's unsaved input as plain text (UX copy U189, owner decision S6-21 D2 (a)): one line
 * „label: value“ per changed field, a group's heading above its fields, groups apart by an empty
 * line. Groups without a field are left out.
 */
export function changesText(groups: readonly ChangedGroup[]): string {
  return groups
    .filter((group) => group.fields.length > 0)
    .map((group) => [...(group.heading === undefined ? [] : [group.heading]), ...group.fields.map((f) => `${f.label}: ${f.value}`)].join('\n'))
    .join('\n\n');
}

/**
 * The conflict of a form (R-CAN-11, Handoff §5.1 S37): after a 409 `conflict` the form saves
 * nothing until it has read the resource again — „saving stays blocked until reload“ —, so a
 * second „Speichern“ opens S37 again without a request instead of overwriting the other change
 * with a tag read after it.
 */
export class EditConflict {
  private editor: string | null = null;
  private conflicted = false;

  /** True between a conflict and the next read of the resource. */
  get blocked(): boolean {
    return this.conflicted;
  }

  /** The other editor's display name of the last conflict; null when the server named nobody. */
  get editorDisplayName(): string | null {
    return this.editor;
  }

  /** Remembers the conflict of `error` and returns true; false (and nothing changes) for any other error. */
  caught(error: unknown): boolean {
    if (problemTypeName(error) !== 'conflict') return false;
    const name = problemDetails(error)?.editorDisplayName;
    this.editor = typeof name === 'string' && name.trim() !== '' ? name : null;
    this.conflicted = true;
    return true;
  }

  /** The resource was read again: saving works again. */
  clear(): void {
    this.conflicted = false;
    this.editor = null;
  }
}
