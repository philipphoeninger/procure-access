// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { LiveAnnouncer } from '@angular/cdk/a11y';
import { TestBed } from '@angular/core/testing';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { A11yAnnouncerService } from './a11y-announcer.service';

describe('A11yAnnouncerService', () => {
  let liveAnnounce: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    liveAnnounce = vi.fn().mockResolvedValue(undefined);
    TestBed.configureTestingModule({
      providers: [provideTestTranslations(), { provide: LiveAnnouncer, useValue: { announce: liveAnnounce } }],
    });
  });

  it('Announce_Key_AnnouncesItsGermanTextPolitely', async () => {
    await TestBed.inject(A11yAnnouncerService).announce('shell.pageTitle', { page: 'Impressum' });

    expect(liveAnnounce).toHaveBeenCalledWith('Impressum – ProcureAccess', 'polite');
  });

  it('Announce_Assertive_PassesThePoliteness', async () => {
    await TestBed.inject(A11yAnnouncerService).announce('legal.placeholder', undefined, { politeness: 'assertive' });

    expect(liveAnnounce).toHaveBeenCalledWith('Platzhalter: Der Inhalt dieser Seite fehlt noch.', 'assertive');
  });

  it('AnnouncePageTitle_AnnouncesTheTitlePolitely', async () => {
    await TestBed.inject(A11yAnnouncerService).announcePageTitle('Konto – ProcureAccess');

    expect(liveAnnounce).toHaveBeenCalledWith('Konto – ProcureAccess', 'polite');
  });
});
