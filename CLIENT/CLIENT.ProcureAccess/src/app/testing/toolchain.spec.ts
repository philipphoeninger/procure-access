// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { render, screen } from '@testing-library/angular';

// Smoke test of the test toolchain (Vitest, jsdom, TestBed, Angular Testing Library).
// It uses its own host component on purpose; tests of application components follow from S1-17.
@Component({
  selector: 'pa-toolchain-host',
  template: `<button type="button" (click)="count.set(count() + 1)">Clicked {{ count() }}</button>`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
class ToolchainHost {
  protected readonly count = signal(0);
}

describe('Test toolchain', () => {
  it('runs without Zone.js', () => {
    expect((globalThis as { Zone?: unknown }).Zone).toBeUndefined();
  });

  it('updates the view after a signal change without a manual detectChanges', async () => {
    const { fixture } = await render(ToolchainHost);

    // A plain DOM click: unlike the fireEvent helpers it does not trigger change detection itself.
    screen.getByRole('button', { name: 'Clicked 0' }).click();
    await fixture.whenStable();

    expect(screen.getByRole('button', { name: 'Clicked 1' })).toBeTruthy();
  });
});
