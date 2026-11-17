// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { Account } from '../../models/account';
import { AccountApiService } from '../../services/api/account-api.service';
import { AccountLoad } from './account-load';
import { accountResolver } from './account.routing';

describe('accountResolver (S61)', () => {
  let get: ReturnType<typeof vi.fn>;
  let incrementLoadingCount: ReturnType<typeof vi.fn>;
  let decrementLoadingCount: ReturnType<typeof vi.fn>;

  function resolve(): Promise<string> {
    TestBed.configureTestingModule({
      providers: [
        AccountLoad,
        { provide: AccountApiService, useValue: { get } },
        { provide: ProcureAccessStore, useValue: { incrementLoadingCount, decrementLoadingCount } },
      ],
    });
    return TestBed.runInInjectionContext(
      () => accountResolver({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot) as Promise<string>,
    );
  }

  beforeEach(() => {
    incrementLoadingCount = vi.fn();
    decrementLoadingCount = vi.fn();
  });

  it('reads the account with the loading bar and titles the page', async () => {
    const account: Account = { email: 'anna@example.test', emailConfirmed: true };
    get = vi.fn().mockResolvedValue(account);

    await expect(resolve()).resolves.toBe('pageTitle.account');

    expect(TestBed.inject(AccountLoad).account).toEqual(account);
    expect(incrementLoadingCount).toHaveBeenCalledTimes(1);
    expect(decrementLoadingCount).toHaveBeenCalledTimes(1);
  });

  it('a failed read titles the S80 server text and tells the page', async () => {
    get = vi.fn().mockRejectedValue(new Error('request failed'));

    await expect(resolve()).resolves.toBe('pageTitle.server');

    expect(TestBed.inject(AccountLoad).account).toBeNull();
    expect(decrementLoadingCount).toHaveBeenCalledTimes(1);
  });
});
