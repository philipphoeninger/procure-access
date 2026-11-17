// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { Subject } from 'rxjs';

import { ProcureAccessStore } from '@app/core/state/app.store';
import type { SessionCheck } from '@app/features/identity/state/with-identity';
import type { SessionWarningData } from '@features/identity/dialogs/session-warning-dialog/session-warning-dialog';
import { LoginOverlayService } from './login-overlay.service';
import { SessionWarningService } from './session-warning.service';

const START = Date.parse('2026-11-17T10:00:00Z');
const MINUTE = 60_000;
const at = (minutes: number) => new Date(START + minutes * MINUTE).toISOString();

describe('SessionWarningService (session warning, R-AUTH-06; A11Y-04)', () => {
  let session: ReturnType<typeof signal<{ id: string } | null>>;
  let expiresAt: ReturnType<typeof signal<string | null>>;
  let absoluteExpiresAt: ReturnType<typeof signal<string | null>>;
  let refreshSession: ReturnType<typeof vi.fn<() => Promise<SessionCheck>>>;
  let requestSignIn: ReturnType<typeof vi.fn>;
  let open: ReturnType<typeof vi.fn>;
  let close: ReturnType<typeof vi.fn>;

  /** Signed in at START: idle limit after 30 minutes, absolute after `absoluteMinutes`, 5 minutes lead. */
  function start(absoluteMinutes = 480, idleMinutes = 30) {
    session = signal<{ id: string } | null>({ id: 'u1' });
    expiresAt = signal<string | null>(at(idleMinutes));
    absoluteExpiresAt = signal<string | null>(at(absoluteMinutes));
    refreshSession = vi.fn<() => Promise<SessionCheck>>().mockResolvedValue('signedIn');
    requestSignIn = vi.fn().mockResolvedValue(true);
    close = vi.fn();
    open = vi.fn(() => {
      const closed = new Subject<void>();
      close.mockImplementation(() => closed.next());
      return { close, afterClosed: () => closed.asObservable() };
    });
    const store = {
      session,
      sessionExpiresAt: computed(() => (session() === null ? null : expiresAt())),
      sessionAbsoluteExpiresAt: computed(() => (session() === null ? null : absoluteExpiresAt())),
      sessionWarningLeadSeconds: computed(() => (session() === null ? null : 300)),
      refreshSession,
    };
    TestBed.configureTestingModule({
      providers: [
        { provide: ProcureAccessStore, useValue: store },
        { provide: MatDialog, useValue: { open } },
        { provide: LoginOverlayService, useValue: { requestSignIn } },
      ],
    });
    TestBed.inject(SessionWarningService).start();
    TestBed.tick();
  }

  /** The state the dialog was opened with. */
  function shownWith(call = 0): SessionWarningData {
    return ((open.mock.calls[call] as unknown[])[1] as { data: SessionWarningData }).data;
  }

  async function advance(minutes: number): Promise<void> {
    await vi.advanceTimersByTimeAsync(minutes * MINUTE);
    // The dialog's code loads with its first use.
    await vi.dynamicImportSettled();
  }

  beforeEach(() => vi.useFakeTimers({ now: START }));
  afterEach(() => vi.useRealTimers());

  it('Warning_IdleLimitNext_ShowsLeadTimeBeforeItAfterTheProbe INV:A11Y-04', async () => {
    start();

    await advance(25 - 1 / 60);
    expect(refreshSession).not.toHaveBeenCalled();
    expect(open).not.toHaveBeenCalled();

    await advance(1 / 60);

    expect(refreshSession).toHaveBeenCalledOnce();
    expect(shownWith().kind()).toBe('idle');
    expect(shownWith().minutes()).toBe(5);
  });

  it('Warning_AbsoluteLimitNext_ShowsTheAbsoluteWarning INV:A11Y-04', async () => {
    start(20);

    await advance(15);

    expect(shownWith().kind()).toBe('absolute');
    expect(shownWith().minutes()).toBe(5);
  });

  it('Warning_Open_CountsTheMinutesDownOnceAMinute INV:A11Y-04', async () => {
    start();
    await advance(25);
    const data = shownWith();

    await advance(59 / 60);
    expect(data.minutes()).toBe(5);
    await advance(1 / 60);
    expect(data.minutes()).toBe(4);
    await advance(3);
    expect(data.minutes()).toBe(1);
  });

  it('Warning_AnswerMovesTheLimit_ClosesTheWarning INV:A11Y-04', async () => {
    start();
    await advance(26);
    expect(open).toHaveBeenCalledOnce();

    // „Angemeldet bleiben“ (or any request) renewed the session: the header moved the idle limit.
    expiresAt.set(at(56));
    TestBed.tick();

    expect(close).toHaveBeenCalled();
    await advance(24);
    expect(open).toHaveBeenCalledOnce();
    await advance(1);
    expect(open).toHaveBeenCalledTimes(2);
  });

  it('shows no warning when the probe finds the session extended in another tab, and warns at the new time', async () => {
    start();
    refreshSession.mockImplementationOnce(() => {
      expiresAt.set(at(40));
      return Promise.resolve('signedIn');
    });

    await advance(25);
    TestBed.tick();
    expect(open).not.toHaveBeenCalled();

    await advance(10);
    expect(open).toHaveBeenCalledOnce();
  });

  it('still warns when the probe fails (offline is not signed out)', async () => {
    start();
    refreshSession.mockResolvedValue('unknown');

    await advance(25);

    expect(open).toHaveBeenCalledOnce();
  });

  it('LimitPassed_SessionEnded_ReplacesTheWarningByTheLoginOverlay INV:A11Y-04', async () => {
    start();
    await advance(25);
    expect(open).toHaveBeenCalledOnce();
    refreshSession.mockImplementation(() => {
      session.set(null);
      return Promise.resolve('signedOut');
    });

    await advance(5);
    TestBed.tick();

    expect(close).toHaveBeenCalled();
    expect(requestSignIn).toHaveBeenCalledExactlyOnceWith('u1');
  });

  it('opens the login overlay instead of the warning when the session already ended (signed out in another tab)', async () => {
    start();
    refreshSession.mockImplementation(() => {
      session.set(null);
      return Promise.resolve('signedOut');
    });

    await advance(25);

    expect(open).not.toHaveBeenCalled();
    expect(requestSignIn).toHaveBeenCalledExactlyOnceWith('u1');
  });

  it('asks for the login overlay once when the warning and the limit both find the session ended', async () => {
    start();
    // Both probes are on their way before the first answers (the page's timers ran late, past both times).
    let answer: (check: SessionCheck) => void = () => undefined;
    const pending = new Promise<SessionCheck>((resolve) => (answer = resolve));
    refreshSession.mockReturnValue(pending);
    await advance(30);
    expect(refreshSession).toHaveBeenCalledTimes(2);

    answer('signedOut');
    await advance(0);

    expect(requestSignIn).toHaveBeenCalledExactlyOnceWith('u1');
  });

  it('only closes the warning at the limit when the probe fails; the next request decides', async () => {
    start();
    await advance(25);
    refreshSession.mockResolvedValue('unknown');

    await advance(5);

    expect(close).toHaveBeenCalled();
    expect(requestSignIn).not.toHaveBeenCalled();
  });

  it('plans nothing for a limit that has already passed, so a wrong clock never starts a probe loop', async () => {
    start(480, -1);

    await advance(60);

    expect(refreshSession).not.toHaveBeenCalled();
    expect(open).not.toHaveBeenCalled();
  });

  it('closes the warning on sign-out and plans nothing while nobody is signed in', async () => {
    start();
    await advance(26);

    session.set(null);
    TestBed.tick();
    await advance(60);

    expect(close).toHaveBeenCalled();
    expect(refreshSession).toHaveBeenCalledOnce();
    expect(open).toHaveBeenCalledOnce();
  });
});
