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
  field?: { id: string; labelKey: string };
}
