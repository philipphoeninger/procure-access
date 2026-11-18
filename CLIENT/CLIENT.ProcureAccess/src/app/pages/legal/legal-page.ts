// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { CatalogText } from '@app/shared/components/catalog-text/catalog-text';
import { Notice } from '@app/shared/components/notice/notice';
import { parseLegalText } from './legal-markdown';
import { LEGAL_TEXTS, LegalPageKind } from './legal-texts';

/**
 * S71 Rechtliche Seiten (R-SHL-07, owner decision O-S6-6 (a)): the H1 `legal.*.title`, then the
 * page's Markdown file `content/legal/*.de.md` with real headings (H2–H4) and lists
 * (`legal-markdown.ts`, `pa-catalog-text`). While the file is a placeholder the notice
 * `legal.placeholder` shows above it; the release check fails on the same files, so the notice
 * never reaches a production release. The route data `legalPage` names the page; page title and
 * H1 focus come from the route like on every page (`PAGE_TITLE_KEY`).
 *
 * @spec docs/design_handoff_procureaccess_v1.md §5.3 (S71)
 */
@Component({
  selector: 'pa-legal-page',
  imports: [TranslatePipe, Notice, CatalogText],
  templateUrl: './legal-page.html',
  styleUrl: './legal-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LegalPage {
  protected readonly kind = inject(ActivatedRoute).snapshot.data['legalPage'] as LegalPageKind;
  protected readonly text = parseLegalText(inject(LEGAL_TEXTS)[this.kind]);
}
