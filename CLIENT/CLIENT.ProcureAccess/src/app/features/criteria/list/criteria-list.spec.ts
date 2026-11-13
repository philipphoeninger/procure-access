// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Criterion } from '../models/criterion.model';
import { CriteriaApiService } from '../services/api/criteria-api.service';
import { CriteriaList } from './criteria-list';

// Covers only that the legacy list renders Markdown without `provideMarkdown()` in app.config.ts:
// the component provides it itself, so ngx-markdown loads with /filters, not with the shell (#264).
// The list goes with the legacy filter.
describe('CriteriaList (legacy)', () => {
  it('renders the Markdown of a criterion description with its own MarkdownService', async () => {
    const criteria = [new Criterion(1, 'Kontrast', 'Text mit **Fettdruck**')];
    await render(CriteriaList, {
      providers: [
        provideTestTranslations(),
        { provide: ProcureAccessStore, useValue: { criteria: () => criteria, criteriaCount: () => criteria.length } },
        { provide: CriteriaApiService, useValue: {} },
      ],
    });

    const bold = await screen.findByText('Fettdruck');
    expect(bold.tagName).toBe('STRONG');
  });
});
