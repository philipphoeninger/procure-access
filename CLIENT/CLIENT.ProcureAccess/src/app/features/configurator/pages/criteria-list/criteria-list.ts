// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { CONFIGURATOR_PATH } from '../../guards/configurator-guards';

/**
 * S11 Kriterienliste at `/kriterien/ergebnis` — placeholder until S4-14 – S4-18 build the list.
 * S10 opens it after the server accepted the selection (R-CON-10); the guard leads back to S10
 * when the selection is gone. Today it shows the H1 and „Auswahl ändern“, which returns to S10
 * with the selection kept. Not `pa-criteria-list`: the legacy criteria list has that selector.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2020-70
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2039-2546 (320 px)
 * @spec docs/SPEC_v2.md R-CON-10 (the list itself: R-LST-01 – R-LST-13 with S4-14 – S4-18)
 */
@Component({
  selector: 'pa-criteria-list-page',
  imports: [MatButtonModule, RouterLink, TranslatePipe],
  templateUrl: './criteria-list.html',
  styleUrl: './criteria-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriteriaListPage {
  protected readonly configuratorPath = CONFIGURATOR_PATH;
}
