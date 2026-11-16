// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, ElementRef, inject, input } from '@angular/core';

/** Handoff §2.1: the action bar is sticky from 600 px width AND 600 px height (same query as the SCSS). */
export const STICKY_ACTION_BAR_QUERY = '(min-width: 37.5em) and (min-height: 37.5em)';

/**
 * The action bar of a criteria list (R-LST-10; Handoff §2.1, §5.1): the status text, then the
 * buttons the page projects, in DOM order. In the flow below 600 px width or height, sticky at the
 * bottom from there. Used by S11 and the page of a saved list (S5-21b), so both keep the same
 * rule for the focus:
 *
 * While the bar is sticky, its height is the page's `scroll-padding-bottom` (WCAG 2.4.11), so the
 * browser scrolls a focused element above it. The page scrolls, not the sidenav content, so the
 * padding goes on `<html>`; the height follows the bar (it wraps when the window narrows) and the
 * query. Firefox ignores the padding when focus moves — it scrolls only an element outside the
 * viewport — so when the focus lands on an element the bar overlaps, the page scrolls it above the
 * bar, as far as its top stays in view.
 *
 * @spec docs/SPEC_v2.md R-LST-10
 */
@Component({
  selector: 'pa-criteria-action-bar',
  template: `
    <p class="pa-criteria-action-bar__status">{{ status() }}</p>
    <ng-content />
  `,
  styleUrl: './criteria-action-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriteriaActionBar {
  /** The status text, already translated (`criteria.list.status.count`). */
  readonly status = input.required<string>();

  constructor() {
    const document = inject(DOCUMENT);
    const bar = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    const window = document.defaultView;
    const root = document.documentElement;
    if (!window?.ResizeObserver || !window.matchMedia) return;
    const sticky = window.matchMedia(STICKY_ACTION_BAR_QUERY);
    effect((onCleanup) => {
      const update = () => {
        root.style.scrollPaddingBottom = sticky.matches ? `${bar.getBoundingClientRect().height}px` : '';
      };
      const reveal = (event: FocusEvent) => {
        const focused = event.target;
        if (!sticky.matches || !(focused instanceof HTMLElement) || bar.contains(focused)) return;
        const box = focused.getBoundingClientRect();
        // An element taller than the space above the bar keeps its top in view.
        const distance = Math.min(box.bottom - bar.getBoundingClientRect().top, box.top);
        if (distance > 0) window.scrollBy({ top: distance });
      };
      const observer = new window.ResizeObserver(update);
      observer.observe(bar);
      sticky.addEventListener('change', update);
      document.addEventListener('focusin', reveal);
      onCleanup(() => {
        observer.disconnect();
        sticky.removeEventListener('change', update);
        document.removeEventListener('focusin', reveal);
        root.style.scrollPaddingBottom = '';
      });
    });
  }
}
