// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { ProcureAccessStore } from '@app/core/state/app.store';

import { CriterionCard } from '../../components/criterion-card/criterion-card';
import { CONFIGURATOR_PATH, LEGACY_FILTER_PATH } from '../../guards/configurator-guards';
import { catalogNames } from '../../state/catalog-names';
import { decisionOf } from '../../state/criteria-list-rules';

/**
 * S11 Kriterienliste at `/kriterien/ergebnis` (R-LST-01, R-LST-02). S10 opens it after the server
 * accepted the selection (R-CON-10); the guard leads back to S10 when the selection is gone.
 *
 * On every entry it loads the criteria of the draft with the pinned release and shows them as
 * cards in catalog order (`pa-criterion-card`). While the request runs the shell's loading bar
 * shows (`withLoading()`). A 404 opens the legacy filter, like the guard of S10 (O-S4-3); any
 * other failure keeps H1 and „Auswahl ändern“: the error interceptor shows the server page (5xx),
 * the offline notice (no answer) or the generic snackbar (O-S4-34 (a)).
 *
 * Summary, notices, search and filter, the empty states, the action bar and „Kriterium
 * vorschlagen“ follow with S4-16 – S4-18. Not `pa-criteria-list`: the legacy list has that
 * selector.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2020-70
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2039-2546 (320 px)
 * @spec docs/SPEC_v2.md R-CON-10, R-LST-01, R-LST-02 (the rest of the list: R-LST-03 – R-LST-13 with S4-16 – S4-18)
 * @invariants CAT-02
 */
@Component({
  selector: 'pa-criteria-list-page',
  imports: [CriterionCard, MatButtonModule, RouterLink, TranslatePipe],
  templateUrl: './criteria-list.html',
  styleUrl: './criteria-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriteriaListPage {
  private readonly store = inject(ProcureAccessStore);
  private readonly router = inject(Router);

  protected readonly configuratorPath = CONFIGURATOR_PATH;
  protected readonly loaded = computed(() => this.store.configurator.list.status() === 'loaded');
  protected readonly criteria = this.store.visibleCriteria;
  protected readonly names = computed(() => catalogNames(this.store.configurator.options()));

  private destroyed = false;

  constructor() {
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));
    void this.load();
  }

  protected decision(key: string) {
    return decisionOf(this.store.configurator.list.decisions(), key);
  }

  private async load(): Promise<void> {
    // A user who left S11 while the request ran stays where they went.
    if ((await this.store.loadCriteriaList()) === 'noRelease' && !this.destroyed) {
      await this.router.navigateByUrl(LEGACY_FILTER_PATH);
    }
  }
}
