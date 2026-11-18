// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Component, Directive, Input, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';
import { TranslatePipe } from '@ngx-translate/core';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { ProposalApiService } from '@app/features/proposal/services/api/proposal-api.service';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { ProductProposal } from './product-proposal';

// The example form of API-02 until S6-23 moved the tagged test to S02 (register-page.spec.ts).
// Replaced, because they are outside the form: the app store (it starts loading everything), the
// criteria filter selection and the permission directive (both need the API).

@Component({ selector: 'pa-filters-selection', template: '' })
class FiltersSelectionStub {}

@Directive({ selector: '[appHasPermission]' })
class NoPermissionStub {
  @Input() appHasPermission = '';
}

async function renderForm() {
  const store = {
    user: signal(undefined),
    selectedCriteriaFiltersIds: signal<number[]>([]),
    loadProposals: vi.fn().mockResolvedValue(undefined),
    getProposalById: vi.fn().mockReturnValue(null),
    upsertProposal: vi.fn(),
    reviewProposal: vi.fn(),
    approveProposal: vi.fn(),
  };
  const { fixture } = await render(ProductProposal, {
    componentImports: [
      FormsModule,
      ReactiveFormsModule,
      MatFormFieldModule,
      MatInputModule,
      MatButtonModule,
      MatIconModule,
      TranslatePipe,
      ErrorSummary,
      FiltersSelectionStub,
      NoPermissionStub,
    ],
    providers: [
      provideTestTranslations(),
      provideRouter([]),
      { provide: ProcureAccessStore, useValue: store },
      { provide: ProposalApiService, useValue: {} },
    ],
  });
  return { fixture, store };
}

const field = (name: string) => screen.getByRole('textbox', { name });

async function fillAndSubmit(fixture: Awaited<ReturnType<typeof renderForm>>['fixture']) {
  fireEvent.input(field('Name'), { target: { value: 'Beispiel-Software' } });
  fireEvent.input(field('Link'), { target: { value: 'beispiel' } });
  fireEvent.input(field('Beschreibung'), { target: { value: 'Ein Programm zum Testen.' } });
  await fixture.whenStable();
  screen.getByRole('button', { name: 'Senden' }).click();
  await fixture.whenStable();
}

describe('ProductProposal server errors', () => {
  it('shows no summary when the proposal is saved', async () => {
    const { fixture, store } = await renderForm();
    store.upsertProposal.mockResolvedValue(undefined);

    await fillAndSubmit(fixture);

    expect(store.upsertProposal).toHaveBeenCalledOnce();
    expect(screen.queryByRole('heading', { level: 2 })).toBeNull();
  });
});
