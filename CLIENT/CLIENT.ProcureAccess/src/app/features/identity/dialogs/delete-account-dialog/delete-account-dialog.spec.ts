// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ComponentFixture } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

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

const deleteButton = () => screen.getByRole('button', { name: 'Löschen' });

async function typeEmail(fixture: ComponentFixture<DeleteAccountDialog>, value: string) {
  fireEvent.input(screen.getByRole('textbox', { name: 'E-Mail' }), { target: { value } });
  await fixture.whenStable();
}

describe('DeleteAccountDialog', () => {
  it('shows the title, the account e-mail and a labelled e-mail field', async () => {
    await renderDialog();

    expect(screen.getByRole('heading', { name: 'Konto löschen' })).toBeTruthy();
    expect(screen.getByText(`Ihre E-Mail: ${accountEmail}`)).toBeTruthy();
    expect(screen.getByRole('textbox', { name: 'E-Mail' })).toBeTruthy();
  });

  // The dialog uses native `disabled`. CLAUDE §9 asks for `aria-disabled` with a visible reason;
  // this test describes today's behaviour and does not set the pattern (screen rebuilt in M1/M2).
  it('keeps "Löschen" unavailable until the account e-mail is typed', async () => {
    const { fixture } = await renderDialog();

    expect(deleteButton()).toHaveProperty('disabled', true);

    await typeEmail(fixture, 'someone.else@example.org');
    expect(deleteButton()).toHaveProperty('disabled', true);

    await typeEmail(fixture, accountEmail);
    expect(deleteButton()).toHaveProperty('disabled', false);
  });

  // The match is exact: no case folding, no trimming, no prefix match.
  it.each([
    ['other upper/lower case', 'Anna.Beispiel@example.org'],
    ['a trailing space', `${accountEmail} `],
    ['a prefix of the address', 'anna.beispiel@example'],
  ])('keeps "Löschen" unavailable for %s', async (_case, typed) => {
    const { fixture } = await renderDialog();

    await typeEmail(fixture, typed);

    expect(deleteButton()).toHaveProperty('disabled', true);
  });

  it('keeps "Löschen" unavailable when the account e-mail is empty (#95)', async () => {
    const { fixture } = await renderDialog('');

    expect(deleteButton()).toHaveProperty('disabled', true);

    await typeEmail(fixture, 'x');
    await typeEmail(fixture, '');
    expect(deleteButton()).toHaveProperty('disabled', true);
  });

  it('closes with true after "Löschen"', async () => {
    const { fixture, dialogRef } = await renderDialog();

    await typeEmail(fixture, accountEmail);
    deleteButton().click();

    expect(dialogRef.close).toHaveBeenCalledExactlyOnceWith(true);
  });

  it('closes with false after "Abbrechen" with nothing typed', async () => {
    const { dialogRef } = await renderDialog();

    screen.getByRole('button', { name: 'Abbrechen' }).click();

    expect(dialogRef.close).toHaveBeenCalledExactlyOnceWith(false);
  });
});
