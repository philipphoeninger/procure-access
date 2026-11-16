// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatRadioChange, MatRadioModule } from '@angular/material/radio';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ErrorPage } from '@app/pages/error/error-page';
import { AppearanceSetting, MotionSetting, SYSTEM_PREFERENCES, UserPreferences } from '../../models/user-preferences';
import { SettingsLoad } from './settings-load';

/**
 * S60 „Einstellungen“ at `/einstellungen` (SPEC R-SET-01; UX copy §5.14; Handoff §5.3: no Figma
 * frame, form card as in S51).
 *
 * H1, intro and one card with four fieldsets in the order of the Handoff: „Sprache“ (only
 * „Deutsch“, with its hint, SD12), „Erscheinungsbild“, „Bewegung und Animationen“ and „Kontrast“
 * (only the system option until H2, R-SET-04). There is no save button: a choice applies at once,
 * without reload, and is saved; once the last change is stored, `settings.saved` is announced. A
 * failed save shows the system states of the error interceptor, and the choice stays applied for
 * this session (owner decision S5-26 D2 (a)). A failed read when the page opens shows the S80
 * server text in place.
 *
 * @spec docs/SPEC_v2.md R-SET-01, R-SET-02, §8.12; docs/design_handoff_procureaccess_v1.md §5.3, §1.7
 * @invariants A11Y-06, A11Y-08, A11Y-09
 */
@Component({
  selector: 'pa-settings-page',
  imports: [ErrorPage, MatRadioModule, TranslatePipe],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPage {
  private readonly store = inject(ProcureAccessStore);
  private readonly announcer = inject(A11yAnnouncerService);

  /** The read failed when the page opened: S80 in place. */
  protected readonly failed = inject(SettingsLoad).failed;
  /** The applied settings; the defaults only if the session ends while the page is open. */
  protected readonly preferences = computed(() => this.store.preferences() ?? SYSTEM_PREFERENCES);

  protected readonly appearances: readonly { value: AppearanceSetting; key: string }[] = [
    { value: 'system', key: 'settings.appearance.system' },
    { value: 'light', key: 'settings.appearance.light' },
    { value: 'dark', key: 'settings.appearance.dark' },
  ];
  protected readonly motions: readonly { value: MotionSetting; key: string }[] = [
    { value: 'system', key: 'settings.motion.system' },
    { value: 'reduced', key: 'settings.motion.reduced' },
  ];
  protected readonly languageHintId = 'pa-settings-language-hint';

  protected appearanceChanged(event: MatRadioChange<AppearanceSetting>): void {
    void this.change({ appearance: event.value });
  }

  protected motionChanged(event: MatRadioChange<MotionSetting>): void {
    void this.change({ motion: event.value });
  }

  private async change(part: Partial<UserPreferences>): Promise<void> {
    let saved: boolean;
    try {
      saved = await this.store.changePreferences({ ...this.preferences(), ...part });
    } catch {
      // The error interceptor showed the failure; the choice stays applied (D2 (a)).
      return;
    }
    if (saved) await this.announcer.announce('settings.saved');
  }
}
