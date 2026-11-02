// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { render, screen } from '@testing-library/angular';
import { firstValueFrom } from 'rxjs';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { TranslatePluralPipe } from './translate-plural.pipe';

// The example key comes from the real de.json (UX copy §6.1); keys that only these tests need are
// added to the loaded German texts, as ngx-translate merges them.

async function renderCount(count: number, key = 'errors.summary.title') {
  const { fixture } = await render(`<p>{{ key | translatePlural: count() }}</p>`, {
    imports: [TranslatePluralPipe],
    providers: [provideTestTranslations()],
    componentProperties: { key, count: signal(count) },
  });
  return { fixture, translate: TestBed.inject(TranslateService) };
}

describe('TranslatePluralPipe', () => {
  it('uses the singular key `.one` for 1', async () => {
    await renderCount(1);

    expect(screen.getByText('1 Angabe muss noch korrigiert werden')).toBeTruthy();
  });

  it('uses the base key with the count for more than 1 and for 0 without a `.zero` key', async () => {
    const { fixture } = await renderCount(3);
    expect(screen.getByText('3 Angaben müssen noch korrigiert werden')).toBeTruthy();

    fixture.componentInstance.count.set(0);
    await fixture.whenStable();
    expect(screen.getByText('0 Angaben müssen noch korrigiert werden')).toBeTruthy();
  });

  it('switches between singular and plural when the count changes', async () => {
    const { fixture } = await renderCount(2);

    fixture.componentInstance.count.set(1);
    await fixture.whenStable();

    expect(screen.getByText('1 Angabe muss noch korrigiert werden')).toBeTruthy();
  });

  it('uses the `.zero` key for 0 when the text has one', async () => {
    const { fixture, translate } = await renderCount(0, 'test.entries');
    translate.setTranslation(
      'de',
      { test: { entries: '{{count}} Einträge', 'entries.one': '1 Eintrag', 'entries.zero': 'Keine Einträge' } },
      true,
    );
    await fixture.whenStable();
    expect(screen.getByText('Keine Einträge')).toBeTruthy();

    fixture.componentInstance.count.set(1);
    await fixture.whenStable();
    expect(screen.getByText('1 Eintrag')).toBeTruthy();
  });

  it('falls back to the base key when a text has no `.one` key', async () => {
    const { fixture, translate } = await renderCount(1, 'test.matches');
    translate.setTranslation('de', { test: { matches: '{{count}} Treffer' } }, true);
    await fixture.whenStable();

    expect(screen.getByText('1 Treffer')).toBeTruthy();
  });

  it('takes singular and plural from the same language when the current one falls back', async () => {
    const { fixture, translate } = await renderCount(1);
    translate.setFallbackLang('de');
    await firstValueFrom(translate.use('en'));
    // English has the base text only; the German `.one` must not be mixed in.
    translate.setTranslation('en', { errors: { summary: { title: '{{count}} entries to correct' } } });
    await fixture.whenStable();

    expect(screen.getByText('1 entries to correct')).toBeTruthy();
  });

  // The plural keys rely on ngx-translate finding a property name with a dot next to the base key
  // (`"title": "…", "title.one": "…"`). Its README does not promise this, so a library update that
  // drops it must fail here.
  it('pins the ngx-translate lookup of a dotted property name next to a text', async () => {
    const { translate } = await renderCount(1);

    expect(translate.instant('errors.summary.title', { count: 2 })).toBe(
      '2 Angaben müssen noch korrigiert werden',
    );
    expect(translate.instant('errors.summary.title.one')).toBe('1 Angabe muss noch korrigiert werden');
  });
});
