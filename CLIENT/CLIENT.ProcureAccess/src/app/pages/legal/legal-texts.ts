// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { InjectionToken } from '@angular/core';

// The files are bundled as text at build time (`loader` in angular.json), into the lazy chunk of
// the legal pages; the release check reads the same files (scripts/check-release-config.mjs).
import accessibility from '../../../../content/legal/barrierefreiheit.de.md';
import privacy from '../../../../content/legal/datenschutz.de.md';
import imprint from '../../../../content/legal/impressum.de.md';

export type LegalPageKind = 'imprint' | 'privacy' | 'accessibility';

/** The Markdown source of each legal page (O-S6-6 (a): `content/legal/*.de.md` in the client). */
export const LEGAL_TEXTS = new InjectionToken<Readonly<Record<LegalPageKind, string>>>(
  'LEGAL_TEXTS',
  { providedIn: 'root', factory: () => ({ imprint, privacy, accessibility }) },
);
