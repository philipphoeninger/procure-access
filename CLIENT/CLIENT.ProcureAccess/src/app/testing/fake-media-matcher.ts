// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { MediaMatcher } from '@angular/cdk/layout';
import { Provider } from '@angular/core';

/**
 * Test double for CDK `MediaMatcher`: media queries such as `prefers-color-scheme: dark` match
 * as set, and `set()` changes them like a change of the system setting while the page is open.
 */
export class FakeMediaMatcher {
  private readonly lists = new Map<string, FakeMediaQueryList>();

  constructor(private readonly initial: Record<string, boolean> = {}) {}

  matchMedia(query: string): MediaQueryList {
    return this.list(query) as unknown as MediaQueryList;
  }

  /** Sets whether `query` matches and notifies its `change` listeners. */
  set(query: string, matches: boolean): void {
    this.list(query).set(matches);
  }

  private list(query: string): FakeMediaQueryList {
    let list = this.lists.get(query);
    if (!list) {
      list = new FakeMediaQueryList(query, this.initial[query] ?? false);
      this.lists.set(query, list);
    }
    return list;
  }
}

class FakeMediaQueryList {
  private readonly listeners = new Set<(event: MediaQueryListEvent) => void>();

  constructor(
    readonly media: string,
    public matches: boolean,
  ) {}

  addEventListener(_type: 'change', listener: (event: MediaQueryListEvent) => void): void {
    this.listeners.add(listener);
  }

  removeEventListener(_type: 'change', listener: (event: MediaQueryListEvent) => void): void {
    this.listeners.delete(listener);
  }

  set(matches: boolean): void {
    this.matches = matches;
    const event = { matches, media: this.media } as MediaQueryListEvent;
    this.listeners.forEach((listener) => listener(event));
  }
}

export const DARK_SCHEME = '(prefers-color-scheme: dark)';
export const REDUCED_MOTION = '(prefers-reduced-motion: reduce)';

export function provideFakeMediaMatcher(matcher: FakeMediaMatcher): Provider {
  return { provide: MediaMatcher, useValue: matcher };
}
