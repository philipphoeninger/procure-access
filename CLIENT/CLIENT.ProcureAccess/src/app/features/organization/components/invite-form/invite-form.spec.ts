// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';
import type { Mock } from 'vitest';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { OrganizationApiService } from '../../services/api/organization-api.service';
import { InviteForm } from './invite-form';

const base = 'https://procureaccess.de/problems/';

describe('InviteForm („Mitglied einladen“ on S21, R-ORG-02)', () => {
  let invite: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let invited: Mock<() => void>;
  let errorsShown: Mock<(shown: boolean) => void>;

  async function openForm(answer: { rejectWith?: unknown } = {}) {
    invite = answer.rejectWith === undefined ? vi.fn().mockResolvedValue({ id: 'i-1', validDays: 14 }) : vi.fn().mockRejectedValue(answer.rejectWith);
    announce = vi.fn().mockResolvedValue(undefined);
    invited = vi.fn<() => void>();
    errorsShown = vi.fn<(shown: boolean) => void>();
    return render(InviteForm, {
      inputs: { organizationId: 'org-1', memberEmails: ['owner@example.test', null] },
      on: { invited, errorsShown },
      providers: [
        provideTestTranslations(),
        { provide: OrganizationApiService, useValue: { invite } },
        { provide: A11yAnnouncerService, useValue: { announce } },
      ],
    });
  }

  async function submit(fixture: { whenStable(): Promise<unknown> }, values: { email?: string; role?: RegExp } = {}) {
    if (values.email !== undefined) fireEvent.input(screen.getByLabelText(/^E-Mail-Adresse/), { target: { value: values.email } });
    if (values.role) screen.getByRole('radio', { name: values.role }).click();
    await fixture.whenStable();
    screen.getByRole('button', { name: 'Einladung senden' }).click();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  it('shows the address field and the three roles with their hints, none chosen (owner decision S5-17b 1 (a))', async () => {
    await openForm();

    expect(screen.getByRole('heading', { level: 2, name: 'Mitglied einladen' })).toBeTruthy();
    const group = screen.getByRole('group', { name: 'Rolle' });
    const radios = within(group).getAllByRole('radio');
    expect(radios).toHaveLength(3);
    expect(radios.every((radio) => !(radio as HTMLInputElement).checked)).toBe(true);
    expect(within(group).getByRole('radio', { name: /^Bearbeiten.*Kann Listen und Projekte anlegen und bearbeiten\./ })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('refuses a missing address and a missing role before sending anything', async () => {
    const { fixture } = await openForm();

    await submit(fixture);

    expect(screen.getByRole('link', { name: 'E-Mail-Adresse: Bitte geben Sie E-Mail-Adresse an.' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Rolle: Bitte wählen Sie eine Option.' })).toBeTruthy();
    expect(invite).not.toHaveBeenCalled();
    expect(errorsShown).toHaveBeenLastCalledWith(true);
    await expectNoAxeViolations();
  });

  it('refuses the address of a member in any spelling before sending anything', async () => {
    const { fixture } = await openForm();

    await submit(fixture, { email: ' OWNER@example.test ', role: /^Lesen/ });

    expect(screen.getByRole('link', { name: 'E-Mail-Adresse: Diese E-Mail-Adresse gehört schon zu einem Mitglied.' })).toBeTruthy();
    expect(invite).not.toHaveBeenCalled();
  });

  it('invites with the trimmed address and the chosen role, announces it with the days and clears the form', async () => {
    const { fixture } = await openForm();

    await submit(fixture, { email: '  neu@example.test ', role: /^Bearbeiten/ });

    expect(invite).toHaveBeenCalledExactlyOnceWith('org-1', { email: 'neu@example.test', role: 'editor' });
    expect(announce).toHaveBeenCalledWith('org.invite.sent', { email: 'neu@example.test', days: 14 });
    expect(invited).toHaveBeenCalledOnce();
    expect(screen.getByLabelText<HTMLInputElement>(/^E-Mail-Adresse/).value).toBe('');
    expect(screen.getAllByRole('radio').every((radio) => !(radio as HTMLInputElement).checked)).toBe(true);
    expect(screen.queryByRole('link')).toBeNull();
  });

  it('shows a failed send in the error notice and still reports the kept invitation (SPEC §7.6)', async () => {
    const { fixture } = await openForm({ rejectWith: new HttpErrorResponse({ status: 503, error: { type: `${base}email-delivery` } }) });

    await submit(fixture, { email: 'neu@example.test', role: /^Lesen/ });

    const alert = screen.getByRole('alert');
    expect(within(alert).getByText(/Einladung nicht gesendet/)).toBeTruthy();
    expect(within(alert).getByText(/Die E-Mail konnte gerade nicht versendet werden/)).toBeTruthy();
    expect(invited).toHaveBeenCalledOnce();
    expect(announce).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('shows the rate limit with its minutes in the error notice', async () => {
    const { fixture } = await openForm({
      rejectWith: new HttpErrorResponse({ status: 429, error: { type: `${base}rate-limit`, retryAfterMinutes: 60 } }),
    });

    await submit(fixture, { email: 'neu@example.test', role: /^Lesen/ });

    expect(within(screen.getByRole('alert')).getByText(/60 Minuten/)).toBeTruthy();
    expect(invited).not.toHaveBeenCalled();
  });

  it('shows a server field error at its field (API-02)', async () => {
    const refused = new HttpErrorResponse({
      status: 400,
      error: { type: `${base}validation`, errors: { email: ['This address belongs to a member already.'] } },
    });
    const { fixture } = await openForm({ rejectWith: refused });

    await submit(fixture, { email: 'neu@example.test', role: /^Lesen/ });

    expect(screen.getByRole('link', { name: 'E-Mail-Adresse: This address belongs to a member already.' })).toBeTruthy();
    expect(screen.getByLabelText(/^E-Mail-Adresse/).getAttribute('aria-invalid')).toBe('true');
  });
});
