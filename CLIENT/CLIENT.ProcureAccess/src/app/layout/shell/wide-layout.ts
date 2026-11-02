// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { BreakpointObserver } from '@angular/cdk/layout';
import { inject, Signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

/** The 600 px viewport breakpoint of Handoff §2.2: compact header below, full header from here. */
export const WIDE_LAYOUT_QUERY = '(min-width: 37.5em)';

/** `true` from 600 px viewport width. Call in an injection context. */
export function injectWideLayout(): Signal<boolean> {
  const observer = inject(BreakpointObserver);
  return toSignal(observer.observe(WIDE_LAYOUT_QUERY).pipe(map((state) => state.matches)), {
    initialValue: observer.isMatched(WIDE_LAYOUT_QUERY),
  });
}
