// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { SignOutService } from './sign-out.service';

describe('SignOutService (R-AUTH-08; S5-30 D3 (b))', () => {
  let endSession: ReturnType<typeof vi.fn>;
  let clearUserData: ReturnType<typeof vi.fn>;
  let navigateByUrl: ReturnType<typeof vi.spyOn>;
  let focusPageHeading: ReturnType<typeof vi.spyOn>;

  function setUp(signOut: Promise<void> = Promise.resolve()) {
    endSession = vi.fn(() => signOut);
    clearUserData = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideTestTranslations(),
        { provide: ProcureAccessStore, useValue: { endSession, clearUserData } },
      ],
    });
    navigateByUrl = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    focusPageHeading = vi.spyOn(TestBed.inject(PageTitleStrategy), 'focusPageHeading').mockImplementation(() => undefined);
    return TestBed.inject(SignOutService);
  }

  it('signs out and shows the start page with the focus on its H1, also when the request fails', async () => {
    const service = setUp(Promise.reject(new Error('offline')));

    await service.signOut();

    expect(endSession).toHaveBeenCalledOnce();
    expect(navigateByUrl).toHaveBeenCalledWith('/');
    expect(focusPageHeading).toHaveBeenCalledOnce();
  });

  it('for another account it clears the previous user\'s data, keeps the session and shows the start page', async () => {
    const service = setUp();

    await service.anotherUserSignedIn();

    expect(clearUserData).toHaveBeenCalledOnce();
    expect(endSession).not.toHaveBeenCalled();
    expect(navigateByUrl).toHaveBeenCalledWith('/');
    expect(focusPageHeading).toHaveBeenCalledOnce();
  });
});
