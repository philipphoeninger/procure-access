// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ComponentFixture } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { DeleteAccountDialog } from './delete-account-dialog';

// Example component test; it shows the convention for component tests (CLAUDE §9, TESTING §5.3):
// - Find elements by role and accessible name (getByRole), as assistive technology does. If an
//   element cannot be found that way, fix its accessibility, not the query. getByText is for
//   plain text only.
// - Names are the real German texts from de.json (provideTestTranslations), not translation keys.
// - Trigger user actions without running change detection, then `await fixture.whenStable()`:
//   clicks with `element.click()` (like a browser, it does nothing on a disabled button; jsdom's
//   fireEvent.click would still run the handler), other events (input, keydown) with fireEvent from
//   '@testing-library/dom'. Do not use fireEvent from '@testing-library/angular': it runs change
//   detection itself and would hide a component that does not update when zoneless (TESTING §3.2).
// - Replace only what is outside the component (here: the dialog reference and its data).

const accountEmail = 'anna.beispiel@example.org';
const reason = 'Das Löschen ist erst möglich, wenn Sie Ihre E-Mail-Adresse genau so eingeben, wie sie in Ihrem Konto steht.';

async function renderDialog(email = accountEmail) {
  const dialogRef = { close: vi.fn() };
  const { fixture } = await render(DeleteAccountDialog, {
    providers: [
      provideTestTranslations(),
      { provide: MatDialogRef, useValue: dialogRef },
      { provide: MAT_DIALOG_DATA, useValue: { email } },
    ],
  });
  return { fixture, dialogRef };
}

const deleteButton = () => screen.getByRole('button', { name: 'Konto endgültig löschen' });

async function typeEmail(fixture: ComponentFixture<DeleteAccountDialog>, value: string) {
  fireEvent.input(screen.getByRole('textbox', { name: 'E-Mail-Adresse' }), { target: { value } });
  await fixture.whenStable();
}

describe('DeleteAccountDialog (S61, R-AUTH-10)', () => {
  it('shows the title, the text and a labelled address field', async () => {
    const { fixture } = await renderDialog();

    expect(screen.getByRole('heading', { name: 'Konto endgültig löschen?' })).toBeTruthy();
    expect(screen.getByText(/Das lässt sich nicht rückgängig machen/)).toBeTruthy();
    expect(screen.getByRole('textbox', { name: 'E-Mail-Adresse' }).getAttribute('autocomplete')).toBe('email');
    await expectNoAxeViolations(fixture.nativeElement as HTMLElement);
  });

  // CLAUDE §9: an unavailable action is `aria-disabled`, never `disabled`, with a visible reason.
  it('keeps the action unavailable with its visible reason until the account address is typed', async () => {
    const { fixture } = await renderDialog();

    expect(deleteButton().getAttribute('aria-disabled')).toBe('true');
    expect(deleteButton()).toHaveProperty('disabled', false);
    expect(deleteButton().getAttribute('aria-describedby')).toBe(screen.getByText(reason).id);

    await typeEmail(fixture, 'someone.else@example.org');
    expect(deleteButton().getAttribute('aria-disabled')).toBe('true');

    await typeEmail(fixture, accountEmail);
    expect(deleteButton().getAttribute('aria-disabled')).toBe('false');
    expect(deleteButton().getAttribute('aria-describedby')).toBeNull();
    expect(screen.queryByText(reason)).toBeNull();
  });

  // The match is exact: no case folding, no prefix match. (An e-mail field drops surrounding
  // spaces itself, so they never reach the comparison.)
  it.each([
    ['other upper/lower case', 'Anna.Beispiel@example.org'],
    ['a prefix of the address', 'anna.beispiel@example'],
  ])('does nothing for %s', async (_case, typed) => {
    const { fixture, dialogRef } = await renderDialog();

    await typeEmail(fixture, typed);
    deleteButton().click();
    await fixture.whenStable();

    expect(deleteButton().getAttribute('aria-disabled')).toBe('true');
    expect(dialogRef.close).not.toHaveBeenCalled();
  });

  it('never matches an empty input, also when the account address is empty (#95)', async () => {
    const { fixture, dialogRef } = await renderDialog('');

    await typeEmail(fixture, 'x');
    await typeEmail(fixture, '');
    deleteButton().click();

    expect(deleteButton().getAttribute('aria-disabled')).toBe('true');
    expect(dialogRef.close).not.toHaveBeenCalled();
  });

  it('closes with true once the address matches', async () => {
    const { fixture, dialogRef } = await renderDialog();

    await typeEmail(fixture, accountEmail);
    deleteButton().click();

    expect(dialogRef.close).toHaveBeenCalledExactlyOnceWith(true);
  });

  it('closes with false after „Konto behalten“', async () => {
    const { dialogRef } = await renderDialog();

    screen.getByRole('button', { name: 'Konto behalten' }).click();

    expect(dialogRef.close).toHaveBeenCalledExactlyOnceWith(false);
  });
});
