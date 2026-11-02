// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  inject,
  input,
  isDevMode,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslatePipe } from '@ngx-translate/core';

export type NoticeKind = 'info' | 'attention' | 'error';

const ICONS: Record<NoticeKind, string> = { info: 'info', attention: 'warning', error: 'error' };

/**
 * pa-notice · Hinweis — a notice with a fixed meaning: info, attention or error. The meaning is
 * never only colour or icon: a visually hidden prefix (`common.notice.*`) names it.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2032-486
 * @spec docs/design_handoff_procureaccess_v1.md §3.3, §6.4
 * @invariants API-02
 */
@Component({
  selector: 'pa-notice',
  imports: [MatIconModule, TranslatePipe],
  templateUrl: './notice.html',
  styleUrl: './notice.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': '"pa-notice pa-notice--" + kind()',
    '[attr.role]': 'role()',
    '[attr.tabindex]': 'focusOnShow() ? -1 : null',
    // A static `title="…"` would also stay a native attribute: a tooltip read a second time.
    '[attr.title]': 'null',
  },
})
export class Notice {
  readonly kind = input.required<NoticeKind>();
  /** Required for `error`. */
  readonly title = input<string>();
  /** Only for notices that appear after an action: `polite` → `status`, `assertive` → `alert`. */
  readonly live = input<'off' | 'polite' | 'assertive'>('off');
  /** Error summary only: the title becomes an `<h2>` and the notice takes the focus when shown. */
  readonly focusOnShow = input(false);

  protected readonly icon = computed(() => ICONS[this.kind()]);
  protected readonly prefixKey = computed(() => `common.notice.${this.kind()}`);
  protected readonly role = computed(() => {
    switch (this.live()) {
      case 'polite':
        return 'status';
      case 'assertive':
        return 'alert';
      default:
        return this.kind() === 'info' ? 'note' : null;
    }
  });

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  constructor() {
    afterNextRender(() => {
      if (isDevMode() && this.kind() === 'error' && !this.title()) {
        console.error('pa-notice: kind "error" needs a title (Handoff §3.3).');
      }
      if (this.focusOnShow()) {
        this.focus();
      }
    });
  }

  focus(): void {
    this.host.nativeElement.focus();
  }
}
