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

  it('Announce_WhileAnotherIsPending_WaitsInsteadOfReplacingIt', async () => {
    let setFirst!: () => void;
    liveAnnounce.mockReturnValueOnce(new Promise<void>((resolve) => (setFirst = resolve)));
    const announcer = TestBed.inject(A11yAnnouncerService);

    const first = announcer.announce('common.loading');
    const second = announcer.announcePageTitle('Konto – ProcureAccess');
    await vi.waitFor(() => expect(liveAnnounce).toHaveBeenCalledTimes(1));
    await Promise.resolve();
    expect(liveAnnounce).toHaveBeenCalledTimes(1);

    setFirst();
    await Promise.all([first, second]);
    expect(liveAnnounce.mock.calls).toEqual([
      ['Wird geladen …', 'polite'],
      ['Konto – ProcureAccess', 'polite'],
    ]);
  });

  describe('throttle per key (Handoff §3.4)', () => {
    beforeEach(() => vi.useFakeTimers({ now: 0 }));
    afterEach(() => vi.useRealTimers());

    const matches = (count: number) =>
      TestBed.inject(A11yAnnouncerService).announce('criteria.select.step1.matches', { count }, { throttleMs: 1000 });

    it('Announce_Throttled_AnnouncesTheFirstCallAtOnce', async () => {
      await matches(3);

      expect(liveAnnounce.mock.calls).toEqual([['3 Treffer', 'polite']]);
    });

    it('Announce_ThrottledWithinTheInterval_ReadsOnlyTheNewestTextAtItsEnd', async () => {
      await matches(6);
      const replaced = matches(2);
      const newest = matches(1);
      await replaced;
      expect(liveAnnounce).toHaveBeenCalledTimes(1);

      await vi.advanceTimersByTimeAsync(1000);
      await newest;

      expect(liveAnnounce.mock.calls).toEqual([
        ['6 Treffer', 'polite'],
        ['1 Treffer', 'polite'],
      ]);
    });

    it('Announce_ThrottledAfterTheInterval_AnnouncesAtOnce', async () => {
      await matches(6);
      await vi.advanceTimersByTimeAsync(1000);

      await matches(2);

      expect(liveAnnounce.mock.calls.map(([text]) => text as string)).toEqual(['6 Treffer', '2 Treffer']);
    });

    it('Announce_Throttled_KeepsOtherKeysApart', async () => {
      await matches(6);
      await TestBed.inject(A11yAnnouncerService).announce('common.loading', undefined, { throttleMs: 1000 });

      expect(liveAnnounce.mock.calls.map(([text]) => text as string)).toEqual(['6 Treffer', 'Wird geladen …']);
    });
  });
});
