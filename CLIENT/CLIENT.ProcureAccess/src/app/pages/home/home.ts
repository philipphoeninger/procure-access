// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { Notice } from '@app/shared/components/notice/notice';
import { CONFIGURATOR_PATH } from '@app/features/configurator/guards/configurator-guards';

/**
 * S04 Startseite (R-SHL-05): H1, intro, entry cards, disclaimer. Only cards whose target exists
 * are shown: „Kriterien zusammenstellen“ opens S10 (or, without an active catalog release, the
 * legacy filter through the guard of S10, O-S4-3) and „Produkte nachschlagen“ the legacy list; the
 * cards for projects (S30) and contributions (S53) and the vendor link (S55) come with their
 * migration steps (owner decision S2-17, 1A). No card needs an account.
 *
 * @spec docs/SPEC_v2.md R-SHL-05; docs/design_handoff_procureaccess_v1.md §5.3 (S04, no Figma frame)
 */
@Component({
  selector: 'pa-home',
  imports: [MatButtonModule, MatCardModule, RouterLink, TranslatePipe, Notice],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  protected readonly criteriaPath = CONFIGURATOR_PATH;
}
