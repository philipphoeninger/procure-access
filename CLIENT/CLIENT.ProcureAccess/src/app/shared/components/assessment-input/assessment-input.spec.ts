// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { AssessmentValue } from '../rating-legend/rating-legend';
import { AssessmentInput, AssessmentLayout } from './assessment-input';

const TITLE = 'Formulare lassen sich vollständig mit der Tastatur bedienen';
/** 11.2.1.1 of the frame: every level but „Nicht anwendbar“. */
const ALLOWED: AssessmentValue[] = ['blockade', 'fulfilled', 'slightRestriction', 'restriction', 'barrier'];
const IN_ORDER = ['Erfüllt', 'Leichte Einschränkung', 'Einschränkung', 'Barriere', 'Blockade', 'Nicht eingeschätzt'];

describe('AssessmentInput (K6 pa-assessment-input, Handoff §4.6)', () => {
  async function setUp(inputs: { layout?: AssessmentLayout; value?: AssessmentValue; note?: string; noteMaxLength?: number } = {}) {
    const valueChange = vi.fn<(value: AssessmentValue) => void>();
    const noteChange = vi.fn<(note: string) => void>();
    const rendered = await render(AssessmentInput, {
      inputs: { criterionTitle: TITLE, allowedValues: ALLOWED, ...inputs },
      providers: [provideTestTranslations(), { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } }],
    });
    rendered.fixture.componentInstance.value.subscribe(valueChange);
    rendered.fixture.componentInstance.note.subscribe(noteChange);
    await rendered.fixture.whenStable();
    return { ...rendered, valueChange, noteChange };
  }

  const group = () => screen.getByRole('group', { name: TITLE });
  const noteField = () => screen.getByRole('textbox', { name: 'Worauf stützt sich Ihre Einschätzung? (optional)', hidden: true });

  describe('radios (from 600 px)', () => {
    it('names the group with the criterion title, asks the question and offers the allowed levels in scale order, „Nicht eingeschätzt“ last (INV:CAT-15)', async () => {
      await setUp({ value: 'fulfilled' });

      const radioGroup = within(group()).getByRole('radiogroup');
      const question = screen.getByText('Wie erfüllt das Produkt dieses Kriterium?');
      expect(radioGroup.getAttribute('aria-describedby')).toBe(question.id);
      expect(within(radioGroup).getAllByRole('radio').map((radio) => radio.closest('mat-radio-button')?.textContent?.trim())).toEqual(IN_ORDER);
      expect(screen.queryByRole('radio', { name: 'Nicht anwendbar' })).toBeNull();
      expect(screen.getByRole<HTMLInputElement>('radio', { name: 'Erfüllt' }).checked).toBe(true);
      await expectNoAxeViolations();
    });

    it('starts at „Nicht eingeschätzt“ and emits the chosen level', async () => {
      const { fixture, valueChange } = await setUp();
      expect(screen.getByRole<HTMLInputElement>('radio', { name: 'Nicht eingeschätzt' }).checked).toBe(true);

      screen.getByRole('radio', { name: 'Barriere' }).click();
      await fixture.whenStable();

      expect(valueChange).toHaveBeenCalledWith('barrier');
      expect(screen.getByRole<HTMLInputElement>('radio', { name: 'Barriere' }).checked).toBe(true);
    });

    it('always shows the note field, described by the criterion title, and emits what is typed', async () => {
      const { fixture, noteChange } = await setUp({ note: 'Eigener Test.' });

      const field = noteField() as HTMLTextAreaElement;
      expect(field.closest('[hidden]')).toBeNull();
      expect(field.value).toBe('Eigener Test.');
      // The long question is a visible label above the field, not a floating one (Handoff §2.2 „Forms“)
      const label = field.labels?.[0];
      expect(label?.closest('mat-form-field')).toBeNull();
      expect(label?.nextElementSibling?.tagName).toBe('MAT-FORM-FIELD');
      expect(field.getAttribute('aria-describedby')).toContain(screen.getByText(TITLE).id);
      expect(screen.queryByRole('button', { name: /Begründung/ })).toBeNull();

      fireEvent.input(field, { target: { value: 'Eigener Test mit NVDA.' } });
      await fixture.whenStable();
      expect(noteChange).toHaveBeenLastCalledWith('Eigener Test mit NVDA.');
    });

    it('shows the length error once a too-long note was left, counted after trimming as the server does (SPEC §7.2)', async () => {
      const { fixture } = await setUp({ noteMaxLength: 10 });
      const field = noteField();

      fireEvent.input(field, { target: { value: '   0123456789   ' } });
      fireEvent.blur(field);
      await fixture.whenStable();
      expect(screen.queryByText(/Zeichen/)).toBeNull();

      fireEvent.input(field, { target: { value: '01234567890' } });
      fireEvent.blur(field);
      await fixture.whenStable();
      expect(screen.getByText('Bitte kürzen Sie den Text auf höchstens 10 Zeichen. Derzeit sind es 11.')).toBeTruthy();
    });
  });

  describe('compact (below 600 px, S34b-M)', () => {
    it('offers the same levels in a select „Einschätzung“ inside the group and emits the choice', async () => {
      const { fixture, valueChange } = await setUp({ layout: 'compact', value: 'restriction' });

      expect(screen.queryByRole('radiogroup')).toBeNull();
      const select = within(group()).getByRole('combobox', { name: 'Einschätzung' });
      expect(select.textContent).toContain('Einschränkung');
      select.click();
      await fixture.whenStable();
      expect(screen.getAllByRole('option').map((option) => option.textContent?.trim())).toEqual(IN_ORDER);

      screen.getByRole('option', { name: 'Blockade' }).click();
      await fixture.whenStable();
      expect(valueChange).toHaveBeenCalledWith('blockade');
      await expectNoAxeViolations();
    });

    it('keeps the note behind „Begründung hinzufügen“, which opens the field and moves the focus into it', async () => {
      const { fixture } = await setUp({ layout: 'compact' });

      const button = screen.getByRole('button', { name: 'Begründung hinzufügen' });
      expect(button.getAttribute('aria-expanded')).toBe('false');
      expect(button.getAttribute('aria-describedby')).toBe(screen.getByText(TITLE).id);
      const panel = document.getElementById(button.getAttribute('aria-controls') ?? '');
      expect(panel?.hidden).toBe(true);

      button.click();
      await fixture.whenStable();

      expect(button.getAttribute('aria-expanded')).toBe('true');
      expect(panel?.hidden).toBe(false);
      expect(document.activeElement).toBe(noteField());
      await expectNoAxeViolations();
    });

    it('names the button „Begründung bearbeiten“ while a note exists', async () => {
      await setUp({ layout: 'compact', note: 'Laut Prüfbericht.' });

      expect(screen.getByRole('button', { name: 'Begründung bearbeiten' })).toBeTruthy();
    });

    it('does not close the field while its note is too long, and shows the error', async () => {
      const { fixture } = await setUp({ layout: 'compact', noteMaxLength: 5 });
      const button = screen.getByRole('button', { name: 'Begründung hinzufügen' });
      button.click();
      await fixture.whenStable();

      fireEvent.input(noteField(), { target: { value: 'zu lang' } });
      await fixture.whenStable();
      button.click();
      await fixture.whenStable();

      expect(button.getAttribute('aria-expanded')).toBe('true');
      expect(document.activeElement).toBe(noteField());
      expect(screen.getByText('Bitte kürzen Sie den Text auf höchstens 5 Zeichen. Derzeit sind es 7.')).toBeTruthy();
    });
  });
});
