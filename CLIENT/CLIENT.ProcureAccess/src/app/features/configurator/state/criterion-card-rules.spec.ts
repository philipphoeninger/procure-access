// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { MatchedRule } from '../models/criteria';
import { criterion, procedureText } from '../testing/criteria-fixture';
import { filterOptions } from '../testing/filter-options-fixture';
import { catalogNames, linkTo } from './catalog-names';
import {
  cardAnchor,
  metaLine,
  procedureTestType,
  requirementTitle,
  ruleText,
  sourceTitle,
} from './criterion-card-rules';

const names = catalogNames(filterOptions());

describe('criterion card rules (R-LST-01, R-LST-02)', () => {
  describe('catalogNames', () => {
    it('names every value of the filter options by key, and test types by URL', () => {
      expect(names.useCases['hr']).toBe('HR');
      expect(names.appTypes['web']).toBe('Web');
      expect(names.productParts['base']).toBe('Base');
      expect(names.testTypeUrls['softwareTest']).toBe('https://example.invalid/sw');
    });

    it('has no names before the options are loaded', () => {
      expect(catalogNames(null).appTypes).toEqual({});
    });
  });

  it('linkTo shows the domain without www., and null for a URL it cannot read', () => {
    expect(linkTo('https://www.bitvtest.de/x')).toEqual({ url: 'https://www.bitvtest.de/x', domain: 'bitvtest.de' });
    expect(linkTo('not a url')).toBeNull();
  });

  it('cardAnchor is k- and the key with hyphens for dots', () => {
    expect(cardAnchor('11.2.1.1')).toBe('k-11-2-1-1');
    expect(cardAnchor('UA1-7.10')).toBe('k-UA1-7-10');
    expect(cardAnchor('PROC-softwareTest')).toBe('k-PROC-softwareTest');
  });

  it('procedureTestType reads the test type of a „Geprüft nach …“ criterion only', () => {
    expect(procedureTestType({ ...criterion('PROC-pdfTest', 'B', null), kind: 'testProcedure' })).toBe('pdfTest');
    expect(procedureTestType(criterion('11.1.1', 'A', null))).toBeNull();
  });

  it('requirementTitle is the plain-language title, else the catalog title (O-S4-32)', () => {
    expect(requirementTitle(criterion('1', 'A', null, { title: 'Katalog', plainLanguage: 'Einfach' }))).toBe('Einfach');
    expect(requirementTitle(criterion('1', 'A', null, { title: 'Katalog', plainLanguage: null }))).toBe('Katalog');
  });

  describe('sourceTitle', () => {
    it('takes number and original title of the first procedure text', () => {
      const withText = criterion('11.2.1.1', 'A', null, {
        procedureTexts: [
          { ...procedureText('Ohne Maus nutzbar'), originalNumber: '11.2.1.1' },
          { ...procedureText('Zweiter'), originalNumber: '9.9' },
        ],
      });

      expect(sourceTitle(withText)).toEqual({
        key: 'criteria.item.sourceTitle',
        params: { number: '11.2.1.1', originalTitle: 'Ohne Maus nutzbar' },
      });
    });

    it('is left out without a procedure text', () => {
      expect(sourceTitle(criterion('1', 'A', null, { procedureTexts: [] }))).toBeNull();
    });
  });

  describe('metaLine (U100, O-S4-31)', () => {
    it('names product part and the EN 301 549 clause', () => {
      expect(metaLine(criterion('11.2.1.1', 'A', null), names)).toEqual({
        key: 'criteria.item.meta',
        params: { productPart: 'Base', number: '11.2.1.1' },
      });
    });

    it('takes the clause of the EN 301 549 reference, not the criterion number', () => {
      const c = criterion('11.1.3.1c', 'A', null, {
        standardReferences: [
          { standard: 'wcag', clause: '1.3.1', url: null },
          { standard: 'en301549', clause: '11.1.3.1', url: null },
        ],
      });

      expect(metaLine(c, names)?.params).toEqual({ productPart: 'Base', number: '11.1.3.1' });
    });

    it('shows only the product part without an EN 301 549 reference (PDF/UA-1 criteria)', () => {
      const pdf = criterion('UA1-7.10', 'A', null, { standardReferences: [] });

      expect(metaLine(pdf, names)).toEqual({ key: 'criteria.item.meta.part', params: { productPart: 'Base' } });
    });

    it('shows only the clause without a product part', () => {
      const c = criterion('11.2.1.1', 'A', null, { productPart: null });

      expect(metaLine(c, names)).toEqual({ key: 'criteria.item.meta.standard', params: { number: '11.2.1.1' } });
    });

    it('is left out when neither exists', () => {
      expect(metaLine(criterion('x', 'A', null, { productPart: null, standardReferences: [] }), names)).toBeNull();
    });
  });

  describe('ruleText (CAT-02)', () => {
    const rule = (kind: MatchedRule['kind'], appType: string | null, useCase: string | null): MatchedRule => ({
      kind,
      appType,
      useCase,
    });

    it.each([
      [rule('appTypeAndUseCase', 'web', 'hr'), 'criteria.priority.rule.full', { appType: 'Web', useCase: 'HR' }],
      [rule('appType', 'web', null), 'criteria.priority.rule.appType', { appType: 'Web' }],
      [rule('useCase', null, 'hr'), 'criteria.priority.rule.useCase', { useCase: 'HR' }],
      [rule('default', null, null), 'criteria.priority.rule.default', {}],
    ])('INV:CAT-02 names the matched rule %o with catalog names', (matched, key, params) => {
      expect(ruleText(matched, names)).toEqual({ key, params });
    });

    it('INV:CAT-02 keeps a key the options do not name, so the rule never goes missing', () => {
      expect(ruleText(rule('appType', 'unknown', null), names)).toEqual({
        key: 'criteria.priority.rule.appType',
        params: { appType: 'unknown' },
      });
    });
  });
});
