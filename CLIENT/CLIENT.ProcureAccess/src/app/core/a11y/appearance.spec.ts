// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';

import {
  DARK_SCHEME,
  FakeMediaMatcher,
  MORE_CONTRAST,
  provideFakeMediaMatcher,
  REDUCED_MOTION,
} from '@app/testing/fake-media-matcher';
import { AppearanceService, provideAppearance } from './appearance';

// R-SET-02, SPEC §7.5: the class on <html> follows the system setting unless a choice is explicit.
describe('provideAppearance', () => {
  let media: FakeMediaMatcher;

  function setUp(system: Record<string, boolean>) {
    media = new FakeMediaMatcher(system);
    TestBed.configureTestingModule({ providers: [provideFakeMediaMatcher(media), provideAppearance()] });
    TestBed.inject(AppearanceService);
    TestBed.tick();
  }

  const html = () => document.documentElement.classList;

  afterEach(() => html().remove('theme-light', 'theme-dark', 'motion-reduced', 'contrast-more'));

  it('SystemDark_HtmlGetsThemeDark', () => {
    setUp({ [DARK_SCHEME]: true });

    expect(html().contains('theme-dark')).toBe(true);
    expect(html().contains('theme-light')).toBe(false);
  });

  it('INV:A11Y-08 SystemSettingChanges_ClassFollowsWithoutReload', () => {
    setUp({ [DARK_SCHEME]: false });
    expect(html().contains('theme-light')).toBe(true);

    media.set(DARK_SCHEME, true);
    TestBed.tick();

    expect(html().contains('theme-dark')).toBe(true);
    expect(html().contains('theme-light')).toBe(false);
  });

  it('ExplicitChoice_OverridesSystemUntilCleared', () => {
    setUp({ [DARK_SCHEME]: false });
    const appearance = TestBed.inject(AppearanceService);

    appearance.setExplicit('dark');
    media.set(DARK_SCHEME, false);
    TestBed.tick();
    expect(html().contains('theme-dark')).toBe(true);

    appearance.setExplicit(null);
    TestBed.tick();
    expect(html().contains('theme-light')).toBe(true);
  });

  it('StorageBlocked_StillFollowsSystem', () => {
    const blocked = vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new DOMException('blocked', 'SecurityError');
    });
    try {
      setUp({ [DARK_SCHEME]: true });

      expect(html().contains('theme-dark')).toBe(true);
      expect(blocked).not.toHaveBeenCalled();
    } finally {
      blocked.mockRestore();
    }
  });

  it.each([
    [true, true],
    [false, false],
  ])('ReducedMotion %s_MaterialAnimationsDisabled %s', (reduced, disabled) => {
    setUp({ [REDUCED_MOTION]: reduced });

    expect(TestBed.inject(MATERIAL_ANIMATIONS).animationsDisabled).toBe(disabled);
    expect(html().contains('motion-reduced')).toBe(reduced);
  });

  it('ExplicitReducedMotion_ClassAndMaterialAnimationsFollowWithoutReload', () => {
    setUp({ [REDUCED_MOTION]: false });
    const appearance = TestBed.inject(AppearanceService);
    const animations = TestBed.inject(MATERIAL_ANIMATIONS);
    expect(animations.animationsDisabled).toBe(false);

    appearance.setExplicitReducedMotion(true);
    TestBed.tick();
    expect(html().contains('motion-reduced')).toBe(true);
    expect(animations.animationsDisabled).toBe(true);

    appearance.setExplicitReducedMotion(false);
    TestBed.tick();
    expect(html().contains('motion-reduced')).toBe(false);
    expect(animations.animationsDisabled).toBe(false);
  });

  it('SystemReducedMotionChanges_ClassFollowsWithoutReload', () => {
    setUp({ [REDUCED_MOTION]: false });

    media.set(REDUCED_MOTION, true);
    TestBed.tick();

    expect(html().contains('motion-reduced')).toBe(true);
    expect(TestBed.inject(MATERIAL_ANIMATIONS).animationsDisabled).toBe(true);
  });

  it('SystemMoreContrast_ContrastMoreFollowsItsChangesWithoutReload', () => {
    setUp({ [MORE_CONTRAST]: true });
    expect(html().contains('contrast-more')).toBe(true);

    media.set(MORE_CONTRAST, false);
    TestBed.tick();

    expect(html().contains('contrast-more')).toBe(false);
  });

  it('ExplicitHighContrast_OverridesSystemUntilCleared', () => {
    setUp({ [MORE_CONTRAST]: false });
    const appearance = TestBed.inject(AppearanceService);

    appearance.setExplicitHighContrast(true);
    TestBed.tick();
    expect(html().contains('contrast-more')).toBe(true);

    appearance.setExplicitHighContrast(false);
    TestBed.tick();
    expect(html().contains('contrast-more')).toBe(false);
  });
});
