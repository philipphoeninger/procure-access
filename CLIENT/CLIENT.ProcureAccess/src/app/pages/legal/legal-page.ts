// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Notice } from '@app/shared/components/notice/notice';

export type LegalPageKind = 'imprint' | 'privacy' | 'accessibility';

/**
 * S71 Rechtliche Seiten — placeholder until GP.3 (R-SHL-07) renders the Markdown content of
 * `content/legal/*.de.md` and fails the production build while a page is missing. The route data
 * `legalPage` names the page.
 *
 * @spec docs/design_handoff_procureaccess_v1.md §5.3 (S71)
 */
@Component({
  selector: 'pa-legal-page',
  imports: [TranslatePipe, Notice],
  templateUrl: './legal-page.html',
  styleUrl: './legal-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LegalPage {
  protected readonly kind = inject(ActivatedRoute).snapshot.data['legalPage'] as LegalPageKind;
}
