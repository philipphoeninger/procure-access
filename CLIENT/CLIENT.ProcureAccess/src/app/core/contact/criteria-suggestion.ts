// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { InjectionToken } from '@angular/core';

import { environment } from '../../../environments/environment';

/**
 * The configured channel of „Kriterium vorschlagen“ (R-SUG-01; SPEC `Contact:CriteriaSuggestion`,
 * ARCHITECTURE §16.4.2 option C: an external link, no data model, no endpoint). The value comes
 * from the environment file of the build; it is a 🔧 placeholder until O-6 is decided.
 */
export const CRITERIA_SUGGESTION_CHANNEL = new InjectionToken<string>('CRITERIA_SUGGESTION_CHANNEL', {
  providedIn: 'root',
  factory: () => environment.criteriaSuggestion,
});

/** A mail address (`mailto:`) or a web page (https URL). */
export type SuggestionChannel = { kind: 'mail'; address: string } | { kind: 'web'; url: string };

const MAILTO = /^mailto:([^\s?#@/]+@[^\s?#@/]+)$/;

/**
 * Reads the configured value. Throws for anything that is not `mailto:<address>` (without
 * query: the subject comes from the UX copy) or an https URL, so a wrong value fails at once.
 */
export function parseSuggestionChannel(value: string): SuggestionChannel {
  const address = MAILTO.exec(value)?.[1];
  if (address) return { kind: 'mail', address };
  if (URL.canParse(value) && new URL(value).protocol === 'https:') return { kind: 'web', url: value };
  throw new Error(`Criteria suggestion channel must be "mailto:<address>" or an https URL: "${value}"`);
}

/** The link target: the URL as configured, or `mailto:` with the subject `suggest.mailSubject`. */
export function suggestionHref(channel: SuggestionChannel, mailSubject: string): string {
  return channel.kind === 'web'
    ? channel.url
    : `mailto:${channel.address}?subject=${encodeURIComponent(mailSubject)}`;
}
