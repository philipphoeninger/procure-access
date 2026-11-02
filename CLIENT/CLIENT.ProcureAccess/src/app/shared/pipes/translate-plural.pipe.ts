// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Pipe, PipeTransform } from '@angular/core';
import { getValue, InterpolationParameters, TranslateService } from '@ngx-translate/core';

/**
 * Plural forms (SPEC §13 UX-O9, decision O-S2-3): a text with a count has up to three keys.
 *
 * - `<key>`      the plural form, used for every count without a more specific key
 * - `<key>.one`  the singular form (CLDR category `one`, chosen with `Intl.PluralRules`)
 * - `<key>.zero` optional, only for count 0 (e.g. „Keine Einträge …“)
 *
 * In the nested de.json, `.one` and `.zero` are property names with a dot next to the base key
 * (`"title": "…", "title.one": "…"`), because a JSON property cannot be a text and an object at
 * once. ngx-translate's lookup finds such names; `translate-plural.pipe.spec.ts` pins that.
 * Languages with more categories (e.g. `few`, `many`) can add `<key>.few` and so on.
 */
export function pluralKey(translate: TranslateService, key: string, count: number): string {
  // Only the language that has the base text decides, so singular and plural come from the
  // same language even when the current language falls back to another one.
  const lang = [translate.getCurrentLang(), translate.getFallbackLang()].find(
    (candidate) => candidate && hasText(translate, candidate, key),
  );
  if (!lang) {
    return key;
  }

  const variant = count === 0 && hasText(translate, lang, `${key}.zero`)
    ? 'zero'
    : pluralRules(lang).select(count);
  return variant !== 'other' && hasText(translate, lang, `${key}.${variant}`)
    ? `${key}.${variant}`
    : key;
}

/**
 * Translates `key` with the plural form for `count`; `count` is passed to the text as `{{count}}`.
 *
 * `{{ 'errors.summary.title' | translatePlural: errors().length }}`
 *
 * Impure like ngx-translate's `translate` pipe: it reads the translations and the current
 * language, both signals in ngx-translate 18, so the view updates when either changes (zoneless).
 */
@Pipe({ name: 'translatePlural', pure: false })
export class TranslatePluralPipe implements PipeTransform {
  private readonly translate = inject(TranslateService);

  transform(key: string, count: number, params?: InterpolationParameters): string {
    return this.translate.instant(pluralKey(this.translate, key, count), { ...params, count });
  }
}

function hasText(translate: TranslateService, lang: string, key: string): boolean {
  return typeof getValue(translate.getTranslations(lang), key) === 'string';
}

const rulesByLang = new Map<string, Intl.PluralRules>();

function pluralRules(lang: string): Intl.PluralRules {
  let rules = rulesByLang.get(lang);
  if (!rules) {
    rules = new Intl.PluralRules(lang);
    rulesByLang.set(lang, rules);
  }
  return rules;
}
