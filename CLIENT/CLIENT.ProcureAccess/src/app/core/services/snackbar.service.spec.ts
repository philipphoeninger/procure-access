// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { screen } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { SnackbarService } from './snackbar.service';

describe('SnackbarService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideTestTranslations()] });
  });

  afterEach(() => {
    document.querySelectorAll('.cdk-overlay-container').forEach((container) => container.remove());
  });

  it.each([
    ['showInfo', (service: SnackbarService) => service.showInfo('Liste gespeichert.')],
    ['showError', (service: SnackbarService) => service.showError('Liste gespeichert.')],
  ])('%s_ShowsTheMessageWithTheGermanCloseButton', async (_method, show) => {
    show(TestBed.inject(SnackbarService));

    const close = await screen.findByRole('button', { name: 'Schließen' });
    expect(screen.getByText('Liste gespeichert.')).toBeTruthy();
    expect(screen.queryByRole('button', { name: /close|dismiss/i })).toBeNull();

    close.click();
    await vi.waitFor(() => expect(screen.queryByText('Liste gespeichert.')).toBeNull());
  });
});
