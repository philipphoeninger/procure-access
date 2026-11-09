// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { CRITERIA_SUGGESTION_CHANNEL, parseSuggestionChannel, suggestionHref } from './criteria-suggestion';

describe('criteria suggestion channel', () => {
  it('reads a mailto: address as a mail channel', () => {
    expect(parseSuggestionChannel('mailto:vorschlag@werte-it.example')).toEqual({
      kind: 'mail',
      address: 'vorschlag@werte-it.example',
    });
  });

  it('reads an https URL as a web channel', () => {
    expect(parseSuggestionChannel('https://werte-it.example/kriterium')).toEqual({
      kind: 'web',
      url: 'https://werte-it.example/kriterium',
    });
  });

  it.each([
    'vorschlag@werte-it.example',
    'mailto:',
    'mailto:vorschlag@werte-it.example?subject=x',
    'mailto:kein-at-zeichen',
    'http://werte-it.example/kriterium',
    'javascript:alert(1)',
    '',
  ])('refuses %j', (value) => {
    expect(() => parseSuggestionChannel(value)).toThrow(/mailto:<address>" or an https URL/);
  });

  it('builds a mailto: link with the encoded subject', () => {
    const channel = parseSuggestionChannel('mailto:vorschlag@werte-it.example');
    expect(suggestionHref(channel, 'Vorschlag für den Kriterienkatalog')).toBe(
      'mailto:vorschlag@werte-it.example?subject=Vorschlag%20f%C3%BCr%20den%20Kriterienkatalog',
    );
  });

  it('uses the configured URL of a web channel as it is', () => {
    const channel = parseSuggestionChannel('https://werte-it.example/kriterium?quelle=procureaccess');
    expect(suggestionHref(channel, 'ignored')).toBe('https://werte-it.example/kriterium?quelle=procureaccess');
  });

  it('defaults to the environment value, which the parser accepts', () => {
    const value = TestBed.inject(CRITERIA_SUGGESTION_CHANNEL);
    expect(value).toBe(environment.criteriaSuggestion);
    expect(parseSuggestionChannel(value).kind).toBe('mail');
  });
});
