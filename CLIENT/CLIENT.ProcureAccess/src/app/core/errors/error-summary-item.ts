// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/** A translated text: UX key and its parameters. */
export interface TextKey {
  key: string;
  params?: Record<string, unknown>;
}

/**
 * One line of the error summary (Handoff §6.4). `message` is the field error exactly as shown at
 * the field (E12); `field` links the line to the input with that id, labelled by `labelKey`.
 */
export interface ErrorSummaryItem {
  message: TextKey;
  /**
   * `group` names the part of the form a repeated label belongs to, e.g. „Prüfnachweis 2“ in S34a:
   * the line then reads „Prüfnachweis 2 – Wer hat geprüft?: …“.
   */
  field?: { id: string; labelKey: string; group?: TextKey };
}
