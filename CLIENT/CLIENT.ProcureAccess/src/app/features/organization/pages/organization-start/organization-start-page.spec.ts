// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { OrganizationApiService } from '../../services/api/organization-api.service';
import { OrganizationStartPage } from './organization-start-page';

const base = 'https://procureaccess.de/problems/';

describe('OrganizationStartPage (S20, R-ORG-01)', () => {
  let create: ReturnType<typeof vi.fn>;
  let setPageTitleState: ReturnType<typeof vi.fn>;
  let organizationJoined: ReturnType<typeof vi.fn>;

  async function openPage(options: { rejectWith?: unknown } = {}) {
    create =
      options.rejectWith === undefined ? vi.fn().mockResolvedValue({ id: 'org-1' }) : vi.fn().mockRejectedValue(options.rejectWith);
    setPageTitleState = vi.fn();
    organizationJoined = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(OrganizationStartPage, {
      routes: [{ path: 'organisation/:id/mitglieder', children: [] }],
      providers: [
        provideTestTranslations(),
        { provide: OrganizationApiService, useValue: { create } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: ProcureAccessStore, useValue: { session: () => ({ id: 'u1', email: 'ada@example.test' }), resendConfirmation: vi.fn(), organizationJoined } },
      ],
    });
    return rendered;
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  async function submit(fixture: { whenStable(): Promise<unknown> }) {
    screen.getByRole('button', { name: 'Organisation anlegen' }).click();
    await settle(fixture);
  }

  function fillName(value: string) {
    fireEvent.input(screen.getByLabelText('Name der Organisation'), { target: { value } });
  }

  it('shows the H1, the intro, the contribution note, the form and how to join (R-ORG-06)', async () => {
    await openPage();

    expect(screen.getByRole('heading', { level: 1, name: 'Organisation anlegen oder beitreten' })).toBeTruthy();
    expect(screen.getByText(/Andere Organisationen sehen Ihre Listen und Projekte nicht/)).toBeTruthy();
    expect(screen.getByText(/Dafür brauchen Sie keine Organisation/)).toBeTruthy();
    expect(screen.getByRole('group', { name: 'Art der Organisation' })).toBeTruthy();
    expect(screen.getAllByRole('radio').map((radio) => radio.getAttribute('value'))).toEqual(['publicBody', 'company', 'association']);
    expect(screen.getByRole('radio', { name: 'Öffentliche Stelle' })).toBeTruthy();
    expect(screen.getByText(/bitten Sie ein Mitglied mit der Rolle „Verwalten“ um eine Einladung/)).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('lists the missing name and type in the error summary and sends nothing (Handoff §6.4)', async () => {
    const { fixture } = await openPage();

    await submit(fixture);

    expect(screen.getByRole('heading', { level: 2, name: /2 Angaben müssen noch korrigiert werden/ })).toBeTruthy();
    expect(screen.getByRole('link', { name: /Name der Organisation: Bitte geben Sie Name der Organisation an/ }).getAttribute('href')).toBe(
      '#pa-org-name',
    );
    expect(screen.getByRole('link', { name: /Art der Organisation: Bitte wählen Sie eine Option\./ }).getAttribute('href')).toBe(
      '#pa-org-type-publicBody-input',
    );
    expect(screen.getByRole('radiogroup').getAttribute('aria-describedby')).toBe('pa-org-type-error');
    expect(setPageTitleState).toHaveBeenLastCalledWith({ error: true });
    expect(create).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('refuses a name longer than 120 characters after trimming, with the count (SPEC §7.2)', async () => {
    const { fixture } = await openPage();

    fillName(` ${'a'.repeat(121)} `);
    screen.getByRole('radio', { name: 'Unternehmen' }).click();
    await submit(fixture);

    expect(screen.getAllByText('Bitte kürzen Sie den Text auf höchstens 120 Zeichen. Derzeit sind es 121.').length).toBeGreaterThan(0);
    expect(create).not.toHaveBeenCalled();
  });

  it('sends the trimmed name and the type, makes the new organisation the header\'s and opens its S21 (R-SHL-02)', async () => {
    const { fixture } = await openPage();
    const router = fixture.debugElement.injector.get(Router);
    const navigate = vi.spyOn(router, 'navigate');

    fillName('  Stadt Musterhausen  ');
    screen.getByRole('radio', { name: 'Verband oder Verein' }).click();
    await submit(fixture);

    expect(create).toHaveBeenCalledWith({ name: 'Stadt Musterhausen', type: 'association' });
    expect(organizationJoined).toHaveBeenCalledExactlyOnceWith('org-1');
    expect(navigate).toHaveBeenCalledWith(['organisation', 'org-1', 'mitglieder']);
  });

  it('shows the confirmation notice with „erneut senden“ when the address is not confirmed, and keeps the input (SEC-10)', async () => {
    const { fixture } = await openPage({
      rejectWith: new HttpErrorResponse({ status: 403, error: { type: `${base}email-unconfirmed` } }),
    });

    fillName('Stadt Musterhausen');
    screen.getByRole('radio', { name: 'Öffentliche Stelle' }).click();
    await submit(fixture);

    expect(screen.getByText(/Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse/)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Bestätigungs-E-Mail erneut senden' })).toBeTruthy();
    expect(document.activeElement?.classList.contains('pa-org-start__confirm')).toBe(true);
    expect(screen.getByLabelText<HTMLInputElement>('Name der Organisation').value).toBe('Stadt Musterhausen');
    await expectNoAxeViolations();
  });

  it('shows the wait of a 429 in the error summary (SPEC §7.3, S6-26)', async () => {
    const { fixture } = await openPage({
      rejectWith: new HttpErrorResponse({ status: 429, error: { type: `${base}rate-limit`, retryAfterMinutes: 1440 } }),
    });

    fillName('Stadt Musterhausen');
    screen.getByRole('radio', { name: 'Unternehmen' }).click();
    await submit(fixture);

    expect(screen.getByText(/Bitte warten Sie 1440 Minuten und versuchen Sie es dann erneut\./)).toBeTruthy();
  });

  it('puts the server field errors at their fields (API-02)', async () => {
    const { fixture } = await openPage({
      rejectWith: new HttpErrorResponse({
        status: 400,
        error: { type: `${base}validation`, errors: { name: ['The name is required.'] } },
      }),
    });

    fillName('x');
    screen.getByRole('radio', { name: 'Unternehmen' }).click();
    await submit(fixture);

    expect(screen.getByRole('link', { name: /Name der Organisation: .*The name is required\./ })).toBeTruthy();
  });
});
