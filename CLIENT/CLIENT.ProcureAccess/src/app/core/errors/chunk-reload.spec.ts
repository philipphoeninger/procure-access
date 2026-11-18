// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Location } from '@angular/common';
import { provideLocationMocks } from '@angular/common/testing';
import { Component, Injectable } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, withNavigationErrorHandler } from '@angular/router';

import {
  CHUNK_RELOAD_STATE_KEY,
  chunkFailureAction,
  DocumentReload,
  handleChunkLoadError,
  isChunkLoadError,
} from './chunk-reload';
import { ConnectionStatus } from './system-errors';

@Component({ template: '' })
class Blank {}

/** The real guard with the browser's reload replaced by a spy. */
@Injectable()
class TestDocumentReload extends DocumentReload {
  readonly reloads = vi.fn();
  protected override reloadDocument(): void {
    this.reloads();
  }
}

/** Lets the deferred mark and reload run. */
const nextTask = () => new Promise((resolve) => setTimeout(resolve));

const CHROMIUM = 'Failed to fetch dynamically imported module: https://example.test/chunk-ABC.js';
const FIREFOX = 'error loading dynamically imported module: https://example.test/chunk-ABC.js';
const WEBKIT = 'Importing a module script failed.';

describe('isChunkLoadError', () => {
  it.each([CHROMIUM, FIREFOX, WEBKIT])('recognises the TypeError of a failed chunk: %s', (message) => {
    expect(isChunkLoadError(new TypeError(message))).toBe(true);
  });

  it('ignores every other error', () => {
    expect(isChunkLoadError(new TypeError('Cannot read properties of undefined'))).toBe(false);
    expect(isChunkLoadError(new Error(CHROMIUM))).toBe(false);
    expect(isChunkLoadError(CHROMIUM)).toBe(false);
    expect(isChunkLoadError(undefined)).toBe(false);
  });
});

describe('chunkFailureAction (the guard against a reload loop)', () => {
  it('reloads a target the history entry was not reloaded for yet', () => {
    expect(chunkFailureAction('/kriterien', null, false)).toBe('reload');
    expect(chunkFailureAction('/kriterien', '/konto', false)).toBe('reload');
  });

  it('shows the error page when the entry was already reloaded for the same target, never a second reload', () => {
    expect(chunkFailureAction('/kriterien', '/kriterien', false)).toBe('errorPage');
  });

  it('stays while offline, also after a reload', () => {
    expect(chunkFailureAction('/kriterien', null, true)).toBe('stay');
    expect(chunkFailureAction('/kriterien', '/kriterien', true)).toBe('stay');
  });
});

// The mock location is the history the router writes to, so these tests see the order of the
// router's restore of the previous address and the mark of the reload.
describe('the guarded reload in the router (#296)', () => {
  let failure: Error;
  let reload: TestDocumentReload;
  let location: Location;
  let router: Router;

  beforeEach(() => {
    failure = new TypeError(CHROMIUM);
    TestBed.configureTestingModule({
      providers: [
        provideLocationMocks(),
        provideRouter(
          [
            { path: '', component: Blank },
            { path: 'lazy', loadComponent: () => Promise.reject(failure) },
            { path: 'error', component: Blank },
          ],
          withNavigationErrorHandler(handleChunkLoadError),
        ),
        { provide: DocumentReload, useClass: TestDocumentReload },
      ],
    });
    reload = TestBed.inject(DocumentReload) as TestDocumentReload;
    location = TestBed.inject(Location);
    router = TestBed.inject(Router);
  });

  /** Navigates to `/lazy`; the router rejects the navigation when the handler keeps the error. */
  async function toLazy(): Promise<void> {
    await router.navigateByUrl('/lazy').catch(() => false);
    await nextTask();
  }

  it('reloads the target once, marked in its own history entry, after the router restored the previous address', async () => {
    await router.navigateByUrl('/');

    await toLazy();

    expect(reload.reloads).toHaveBeenCalledTimes(1);
    expect(location.path()).toBe('/lazy');
    expect(location.getState()).toEqual({ [CHUNK_RELOAD_STATE_KEY]: '/lazy' });
    expect(reload.reloadedFor()).toBe('/lazy');
  });

  it('shows the error page in place in the reloaded entry, instead of reloading again', async () => {
    // The reloaded document: the address shows the target, its entry carries the mark.
    location.replaceState('/lazy', '', { navigationId: 1, [CHUNK_RELOAD_STATE_KEY]: '/lazy' });

    await toLazy();

    expect(reload.reloads).not.toHaveBeenCalled();
    expect(router.url).toBe('/error');
    expect(location.path()).toBe('/lazy');
  });

  it('reloads a target other than the one the entry was reloaded for', async () => {
    location.replaceState('/', '', { [CHUNK_RELOAD_STATE_KEY]: '/other' });

    await toLazy();

    expect(reload.reloads).toHaveBeenCalledTimes(1);
    expect(reload.reloadedFor()).toBe('/lazy');
  });

  it('does not reload while offline', async () => {
    TestBed.inject(ConnectionStatus).noAnswer();

    await toLazy();

    expect(reload.reloads).not.toHaveBeenCalled();
    expect(router.url).toBe('/');
  });

  it('leaves every other navigation error to the router', async () => {
    failure = new Error('a bug in a guard');

    await toLazy();

    expect(reload.reloads).not.toHaveBeenCalled();
    expect(router.url).toBe('/');
  });
});

describe('DocumentReload', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideLocationMocks(), { provide: DocumentReload, useClass: TestDocumentReload }],
    });
  });

  it('marks the current entry when the address already shows the target (back/forward), keeping its state', async () => {
    const location = TestBed.inject(Location);
    location.replaceState('/kriterien', '', { navigationId: 3 });
    const reload = TestBed.inject(DocumentReload) as TestDocumentReload;

    reload.reload('/kriterien');
    expect(reload.reloads).not.toHaveBeenCalled();
    await nextTask();

    expect(location.getState()).toEqual({ navigationId: 3, [CHUNK_RELOAD_STATE_KEY]: '/kriterien' });
    expect(reload.reloads).toHaveBeenCalledTimes(1);
  });

  it('reads no target from a state without the key', () => {
    TestBed.inject(Location).replaceState('/', '', { navigationId: 1 });

    expect(TestBed.inject(DocumentReload).reloadedFor()).toBeNull();
  });
});
