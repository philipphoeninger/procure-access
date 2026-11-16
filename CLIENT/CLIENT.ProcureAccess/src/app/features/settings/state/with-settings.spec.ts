// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { patchState, signalStore, withState } from '@ngrx/signals';

import { AppearanceService, provideAppearance } from '@app/core/a11y/appearance';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { SessionUser } from '@app/features/identity/models/session';
import {
  DARK_SCHEME,
  FakeMediaMatcher,
  MORE_CONTRAST,
  provideFakeMediaMatcher,
  REDUCED_MOTION,
} from '@app/testing/fake-media-matcher';
import { UserPreferences } from '../models/user-preferences';
import { PreferencesApiService } from '../services/api/preferences-api.service';
import { LanguageService } from '../services/language.service';
import { withSettings } from './with-settings';

// The session of withIdentity() as the probe would report it, set from outside the store.
const SettingsTestStore = signalStore(
  { protectedState: false },
  withState<{ session: SessionUser | null }>({ session: null }),
  withSettings(),
);

const DEFAULTS: UserPreferences = { language: 'de', appearance: 'system', motion: 'system', contrast: 'system' };
const DARK_REDUCED: UserPreferences = { ...DEFAULTS, appearance: 'dark', motion: 'reduced' };

const userOne: SessionUser = { id: 'u1', email: 'one@example.test', roles: [], permissions: [] };
const userTwo: SessionUser = { id: 'u2', email: 'two@example.test', roles: [], permissions: [] };

let media: FakeMediaMatcher;
let get: ReturnType<typeof vi.fn<() => Promise<UserPreferences>>>;
let showInfo: ReturnType<typeof vi.fn>;
let languageInit: ReturnType<typeof vi.fn>;

function setUp(system: Record<string, boolean> = {}) {
  media = new FakeMediaMatcher(system);
  get = vi.fn<() => Promise<UserPreferences>>();
  showInfo = vi.fn();
  languageInit = vi.fn();
  TestBed.configureTestingModule({
    providers: [
      provideFakeMediaMatcher(media),
      provideAppearance(),
      SettingsTestStore,
      { provide: PreferencesApiService, useValue: { get } },
      { provide: LanguageService, useValue: { init: languageInit } },
      { provide: SnackbarService, useValue: { showInfo } },
    ],
  });
  const store = TestBed.inject(SettingsTestStore);
  TestBed.tick();
  return store;
}

/** Signs `user` in or out as the probe would, and waits until the store has read the settings. */
async function sessionOf(store: InstanceType<typeof SettingsTestStore>, user: SessionUser | null) {
  patchState(store, { session: user });
  TestBed.tick();
  await new Promise((resolve) => setTimeout(resolve));
  TestBed.tick();
}

const html = () => document.documentElement.classList;
const appearance = () => TestBed.inject(AppearanceService).appearance();

describe('withSettings', () => {
  afterEach(() => html().remove('theme-light', 'theme-dark', 'motion-reduced', 'contrast-more'));

  it('INV:A11Y-06 SignedInUserWithDarkAndReducedMotion_OverridesTheLightSystemOnThePage', async () => {
    const store = setUp({ [DARK_SCHEME]: false, [REDUCED_MOTION]: false });
    get.mockResolvedValue(DARK_REDUCED);

    await sessionOf(store, userOne);

    expect(store.preferences()).toEqual(DARK_REDUCED);
    expect(appearance()).toBe('dark');
    expect(html().contains('theme-dark')).toBe(true);
    expect(html().contains('motion-reduced')).toBe(true);
    expect(TestBed.inject(MATERIAL_ANIMATIONS).animationsDisabled).toBe(true);
  });

  it('INV:A11Y-06 SignedInUserWithSystemSettings_FollowsTheSystemAndItsChanges', async () => {
    const store = setUp({ [DARK_SCHEME]: false });
    get.mockResolvedValue(DEFAULTS);
    await sessionOf(store, userOne);
    expect(appearance()).toBe('light');

    media.set(DARK_SCHEME, true);
    TestBed.tick();

    expect(appearance()).toBe('dark');
    expect(html().contains('motion-reduced')).toBe(false);
  });

  it('INV:A11Y-06 SignOut_FollowsTheSystemAgain', async () => {
    const store = setUp({ [DARK_SCHEME]: false });
    get.mockResolvedValue(DARK_REDUCED);
    await sessionOf(store, userOne);

    await sessionOf(store, null);

    expect(store.preferences()).toBeNull();
    expect(appearance()).toBe('light');
    expect(html().contains('motion-reduced')).toBe(false);
  });

  it('HighContrastAnswered_SetsContrastMore', async () => {
    const store = setUp({ [MORE_CONTRAST]: false });
    get.mockResolvedValue({ ...DEFAULTS, contrast: 'high' });

    await sessionOf(store, userOne);

    expect(html().contains('contrast-more')).toBe(true);
  });

  it('SessionOfAUser_ReadsTheirSettingsOnceWhileTheSessionIsRenewed', async () => {
    const store = setUp();
    get.mockResolvedValue(DEFAULTS);

    patchState(store, { session: userOne });
    TestBed.tick();
    patchState(store, { session: { ...userOne } }); // the next probe answers a new object
    TestBed.tick();
    await Promise.resolve();

    expect(get).toHaveBeenCalledTimes(1);
  });

  it('ReadingFails_FollowsTheSystemWithoutShowingAnything', async () => {
    const store = setUp({ [DARK_SCHEME]: true });
    get.mockRejectedValue(new Error('request failed'));

    await sessionOf(store, userOne);

    expect(store.preferences()).toBeNull();
    expect(appearance()).toBe('dark');
    expect(showInfo).not.toHaveBeenCalled();
  });

  it('AnswerForAUserWhoSignedOutMeanwhile_IsDropped', async () => {
    const store = setUp({ [DARK_SCHEME]: false });
    let answer: (preferences: UserPreferences) => void = () => undefined;
    get.mockReturnValueOnce(new Promise<UserPreferences>((resolve) => (answer = resolve)));
    const reading = store.loadPreferences(userOne.id);

    await store.loadPreferences(null);
    answer(DARK_REDUCED);
    await reading;
    TestBed.tick();

    expect(store.preferences()).toBeNull();
    expect(appearance()).toBe('light');
  });

  it('AnotherUserSignsIn_TheirSettingsReplaceThePrevious', async () => {
    const store = setUp({ [DARK_SCHEME]: false });
    get.mockResolvedValueOnce(DARK_REDUCED).mockResolvedValueOnce(DEFAULTS);
    await sessionOf(store, userOne);

    await sessionOf(store, userTwo);

    expect(appearance()).toBe('light');
    expect(html().contains('motion-reduced')).toBe(false);
  });

  it('Anonymous_NoRequestAndGerman', () => {
    setUp();

    expect(get).not.toHaveBeenCalled();
    expect(languageInit).toHaveBeenCalledWith('de');
  });

  // TD-25 (#234): the legacy store wrote to localStorage for signed-out users, threw when storage
  // was blocked, and opened an English snackbar on a contrast change.
  describe('without browser storage (R-SET-02, TD-25)', () => {
    afterEach(() => vi.restoreAllMocks());

    it('StartAndSignInAndOut_NeverTouchStorage', async () => {
      const touched = [
        vi.spyOn(Storage.prototype, 'getItem'),
        vi.spyOn(Storage.prototype, 'setItem'),
        vi.spyOn(Storage.prototype, 'removeItem'),
      ];
      const store = setUp();
      get.mockResolvedValue(DARK_REDUCED);

      await sessionOf(store, userOne);
      await sessionOf(store, null);

      touched.forEach((spy) => expect(spy).not.toHaveBeenCalled());
    });

    it('StorageBlocked_StartsAndAppliesTheSettings', async () => {
      const blocked = () => {
        throw new DOMException('blocked', 'SecurityError');
      };
      vi.spyOn(Storage.prototype, 'getItem').mockImplementation(blocked);
      vi.spyOn(Storage.prototype, 'setItem').mockImplementation(blocked);
      const store = setUp({ [DARK_SCHEME]: false });
      get.mockResolvedValue(DARK_REDUCED);

      await sessionOf(store, userOne);

      expect(appearance()).toBe('dark');
    });

    it('SystemContrastChanges_FollowedWithoutSnackbar', async () => {
      const store = setUp({ [MORE_CONTRAST]: false });
      get.mockResolvedValue(DEFAULTS);
      await sessionOf(store, userOne);

      media.set(MORE_CONTRAST, true);
      TestBed.tick();

      expect(html().contains('contrast-more')).toBe(true);
      expect(showInfo).not.toHaveBeenCalled();
    });
  });
});
