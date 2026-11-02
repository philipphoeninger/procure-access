// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Component, Directive, Input, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { provideRouter } from '@angular/router';
import { render, screen, within } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';
import { TranslatePipe } from '@ngx-translate/core';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { ProposalApiService } from '@app/features/proposal/services/api/proposal-api.service';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { ProductProposal } from './product-proposal';

// API-02 on the example form of S2-11 (CLAUDE §9 convention, see delete-account-dialog.spec.ts).
// Replaced, because they are outside the form: the app store (it starts loading everything), the
// criteria filter selection and the permission directive (both need the API).

@Component({ selector: 'pa-filters-selection', template: '' })
class FiltersSelectionStub {}

@Directive({ selector: '[appHasPermission]' })
class NoPermissionStub {
  @Input() appHasPermission = '';
}

const linkError = 'The Link field is not a valid fully-qualified http, https, or ftp URL.';
const bodyError = 'The request body is invalid.';

function validationProblem(errors: Record<string, string[]>) {
  return new HttpErrorResponse({
    status: 400,
    error: { type: 'https://procureaccess.de/problems/validation', title: 'Validation failed', status: 400, errors },
  });
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
  it('Submit_ServerFieldErrors_BoundToFieldsAndSummaryFocused INV:API-02', async () => {
    const { fixture, store } = await renderForm();
    store.upsertProposal.mockRejectedValue(
      validationProblem({ 'product.link': [linkError], $: [bodyError] }),
    );

    await fillAndSubmit(fixture);

    // The summary names the number of errors and takes the focus, so it is announced.
    const heading = screen.getByRole('heading', { level: 2, name: 'Fehler: 2 Angaben müssen noch korrigiert werden' });
    expect(document.activeElement).toBe(heading.closest('pa-notice'));

    // The field error sits at the field and is linked to it.
    const link = field('Link');
    const fieldError = screen.getByText(linkError);
    expect(link.getAttribute('aria-invalid')).toBe('true');
    expect(link.getAttribute('aria-describedby')?.split(' ')).toContain(fieldError.id);

    // A known field is a link in the summary that moves the focus to the field;
    // a path without a field ($ = the whole body) is a line without a link.
    const summary = within(heading.closest('pa-notice') as HTMLElement);
    summary.getByRole('link', { name: `Link: ${linkError}` }).click();
    expect(document.activeElement).toBe(link);
    expect(summary.getByText(bodyError).closest('a')).toBeNull();
    expect(summary.getAllByRole('link')).toHaveLength(1);
  });

  it('removes the field error when the user edits the field and focuses the summary again on the next failed submit', async () => {
    const { fixture, store } = await renderForm();
    store.upsertProposal.mockRejectedValue(validationProblem({ 'product.link': [linkError] }));
    await fillAndSubmit(fixture);

    fireEvent.input(field('Link'), { target: { value: 'https://beispiel.de' } });
    await fixture.whenStable();
    expect(field('Link').getAttribute('aria-invalid')).toBe('false');
    expect(screen.queryByText(linkError, { selector: 'mat-error' })).toBeNull();

    field('Link').focus();
    screen.getByRole('button', { name: 'Senden' }).click();
    await fixture.whenStable();

    const heading = screen.getByRole('heading', { level: 2, name: 'Fehler: 1 Angabe muss noch korrigiert werden' });
    expect(document.activeElement).toBe(heading.closest('pa-notice'));
  });

  it('shows no summary when the proposal is saved', async () => {
    const { fixture, store } = await renderForm();
    store.upsertProposal.mockResolvedValue(undefined);

    await fillAndSubmit(fixture);

    expect(store.upsertProposal).toHaveBeenCalledOnce();
    expect(screen.queryByRole('heading', { level: 2 })).toBeNull();
  });
});
