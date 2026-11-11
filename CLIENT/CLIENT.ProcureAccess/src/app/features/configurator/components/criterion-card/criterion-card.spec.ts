// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { fireEvent } from '@testing-library/dom';
import { render, screen } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Criterion, ProcedureText } from '../../models/criteria';
import { CriterionDecision, DEFAULT_DECISION, NOTE_MAX_LENGTH } from '../../models/criteria-list';
import { CatalogNames, catalogNames } from '../../state/catalog-names';
import { criterion, procedureText } from '../../testing/criteria-fixture';
import { filterOptions } from '../../testing/filter-options-fixture';
import { CriterionCard } from './criterion-card';

const SOURCE_LINE =
  'Grundlage: Empfehlung des Projektteams WERTE.IT (fachliche Einschätzung, keine Vorgabe aus einer Norm).';

function names(): CatalogNames {
  const options = filterOptions();
  return catalogNames({
    ...options,
    useCases: [{ id: 'hr', name: 'HR-Anwendung', hint: null, excludedBy: [] }],
    appTypes: [{ id: 'web', name: 'Website oder Webanwendung', hint: null, excludedBy: [] }],
    productParts: [
      {
        id: 'base',
        name: 'Basisfunktionalität',
        explanation: '',
        alwaysSelected: true,
        excludedBy: [],
      },
    ],
    testTypes: [
      {
        id: 'softwareTest',
        name: 'Software',
        url: 'https://softwaretest.bit-inklusiv.de/',
        excludedBy: [],
      },
    ],
  });
}

function keyboardCriterion(overrides: Partial<Criterion> = {}): Criterion {
  const base = criterion('11.2.1.1', 'A', null, {
    title: 'Tastaturbedienung',
    plainLanguage: 'Formulare lassen sich vollständig mit der Tastatur bedienen',
    procedureTexts: [{ ...procedureText('Ohne Maus nutzbar'), originalNumber: '11.2.1.1' }],
    ...overrides,
  });
  return {
    ...base,
    resolvedPriority: {
      ...base.resolvedPriority,
      rationale: 'Viele Mitarbeitende arbeiten ausschließlich mit der Tastatur.',
      matchedRule: { kind: 'appTypeAndUseCase', appType: 'web', useCase: 'hr' },
    },
  };
}

function procedureCriterion(): Criterion {
  return {
    ...criterion('PROC-softwareTest', 'B', null, {
      title: 'Geprüft nach Softwaretest',
      plainLanguage: null,
    }),
    kind: 'testProcedure',
    number: null,
    productPart: null,
    appliesTo: [],
    standardReferences: [],
    allowedRatings: [],
  };
}

/** The full Software-Test text of a criterion, as a 2026.1 release folder holds it (joined lines). */
function softwareText(overrides: Partial<ProcedureText> = {}): ProcedureText {
  return {
    testType: 'softwareTest',
    originalNumber: '11.2.1.1',
    originalTitle: 'Ohne Maus nutzbar',
    applicability: 'Der Prüfschritt ist immer anwendbar.',
    requirement: 'Alle Funktionen sind mit der Tastatur bedienbar.\n\n- Menüs\n- Dialoge',
    rationaleWhy: 'Viele Menschen nutzen keine Maus.',
    allowedRatings: ['notApplicable', 'blockade', 'fulfilled', 'barrier'],
    positiveExamples: '- Gut 1\n- Gut 2',
    negativeExamples: '- Schlecht 1\n- Schlecht 2',
    sourceUrl: 'https://softwaretest.bit-inklusiv.de/#ohne-maus',
    ...overrides,
  };
}

function h3Names(): string[] {
  return screen.getAllByRole('heading', { level: 3 }).map((h) => h.textContent?.trim() ?? '');
}

function listItems(list: HTMLElement): string[] {
  return Array.from(list.children).map((li) => li.textContent?.replace(/\s+/g, ' ').trim() ?? '');
}

async function show(
  c: Criterion,
  decision: CriterionDecision = DEFAULT_DECISION,
  catalog: CatalogNames = names(),
) {
  const events = {
    includedChange: vi.fn(),
    levelChange: vi.fn(),
    weightChange: vi.fn(),
    priorityReset: vi.fn(),
    noteChange: vi.fn(),
  };
  const rendered = await render(CriterionCard, {
    providers: [
      provideTestTranslations(),
      // jsdom never ends a CSS transition, so the tab body would wait for it forever.
      { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
    ],
    inputs: { criterion: c, decision, names: catalog },
    on: events,
  });
  // mat-select shows its value once its options exist.
  await rendered.fixture.whenStable();
  const toggle = () => screen.getByRole('button', { name: /Details (anzeigen|ausblenden)/ });
  async function click(): Promise<void> {
    toggle().click();
    await rendered.fixture.whenStable();
  }
  async function act(element: HTMLElement): Promise<void> {
    element.click();
    await rendered.fixture.whenStable();
  }
  return { ...rendered, events, toggle, click, act };
}

describe('CriterionCard (S11, R-LST-01, R-LST-02, Handoff §5.5)', () => {
  describe('requirement', () => {
    it('shows title as H2, source title, meta line and K5, compact', async () => {
      const { toggle } = await show(keyboardCriterion());

      expect(
        screen.getByRole('heading', {
          level: 2,
          name: 'Formulare lassen sich vollständig mit der Tastatur bedienen',
        }),
      ).toBeTruthy();
      expect(screen.getByText('11.2.1.1 Ohne Maus nutzbar · BIT inklusiv')).toBeTruthy();
      expect(screen.getByText('Basisfunktionalität · EN 301 549 11.2.1.1')).toBeTruthy();
      expect(screen.getByText('Priorität: A – Mindestanforderung')).toBeTruthy();
      expect(toggle().getAttribute('aria-expanded')).toBe('false');
      expect(screen.queryByRole('heading', { level: 3 })).toBeNull();
      expect(screen.getByRole('heading', { level: 2 }).id).toBe('k-11-2-1-1');
      await expectNoAxeViolations();
    });

    it('shows the catalog title as H2 while the release has no plain-language title (O-S4-32)', async () => {
      await show(keyboardCriterion({ plainLanguage: null }));

      expect(screen.getByRole('heading', { level: 2, name: 'Tastaturbedienung' })).toBeTruthy();
    });

    it('leaves out source title and meta line when the criterion has neither', async () => {
      const { container } = await show(
        keyboardCriterion({ procedureTexts: [], productPart: null, standardReferences: [] }),
      );

      expect(container.textContent).not.toContain('BIT inklusiv');
      expect(container.textContent).not.toContain('EN 301 549');
    });

    it('names the toggle by the criterion title and points it at the panel', async () => {
      const { toggle, container } = await show(keyboardCriterion());

      const title = screen.getByRole('heading', { level: 2 });
      expect(toggle().getAttribute('aria-describedby')).toBe(title.id);
      const panel = container.querySelector(`#${toggle().getAttribute('aria-controls')}`);
      expect(panel).toBeTruthy();
      expect(panel?.hasAttribute('hidden')).toBe(true);
    });

    it('INV:CAT-02 opens the details with „Warum diese Priorität?“ first: rationale, rule and source', async () => {
      const { toggle, click } = await show(keyboardCriterion());
      toggle().focus();

      await click();

      expect(toggle().getAttribute('aria-expanded')).toBe('true');
      expect(toggle().textContent?.trim()).toBe('Details ausblenden');
      expect(document.activeElement).toBe(toggle());
      const sections = screen.getAllByRole('heading', { level: 3 });
      expect(sections[0]?.textContent?.trim()).toBe('Warum diese Priorität?');
      expect(
        screen.getByText('Viele Mitarbeitende arbeiten ausschließlich mit der Tastatur.'),
      ).toBeTruthy();
      expect(
        screen.getByText(
          'Diese Empfehlung gilt für Website oder Webanwendung mit dem Verwendungszweck HR-Anwendung.',
        ),
      ).toBeTruthy();
      expect(screen.getByText(SOURCE_LINE)).toBeTruthy();
      await expectNoAxeViolations();
    });

    it('closes the details again', async () => {
      const { toggle, click } = await show(keyboardCriterion());

      await click();
      await click();

      expect(toggle().getAttribute('aria-expanded')).toBe('false');
      expect(screen.queryByRole('heading', { level: 3 })).toBeNull();
    });

    it('shows the user override in K5 with the recommendation next to it', async () => {
      await show(keyboardCriterion(), { ...DEFAULT_DECISION, overrideLevel: 'B' });

      expect(screen.getByText('Priorität: B – Bewertungskriterium')).toBeTruthy();
      expect(
        screen.getByText('Von Ihnen geändert. Empfohlen war: A – Mindestanforderung.'),
      ).toBeTruthy();
    });

    it('shows the weight of a recommended B in the weight field', async () => {
      await show(criterion('11.2.2', 'B', 3));

      expect(screen.getByRole('combobox', { name: 'Gewichtung' }).textContent).toContain('3');
    });
  });

  describe('„Geprüft nach …“ (R-LST-07)', () => {
    it('shows title, text and the test type link that opens in a new tab', async () => {
      await show(procedureCriterion());

      expect(
        screen.getByRole('heading', { level: 2, name: 'Geprüft nach BIT inklusiv Software-Test' }),
      ).toBeTruthy();
      expect(
        screen.getByText(
          'Der Anbieter oder eine unabhängige Stelle hat das Produkt nach dem BIT inklusiv Software-Test geprüft und kann den Bericht vorlegen.',
        ),
      ).toBeTruthy();
      const link = screen.getByRole('link', {
        name: 'softwaretest.bit-inklusiv.de (öffnet in neuem Tab)',
      });
      expect(link.getAttribute('href')).toBe('https://softwaretest.bit-inklusiv.de/');
      expect(link.getAttribute('target')).toBe('_blank');
      expect(link.parentElement?.textContent?.replace(/\s+/g, ' ').trim()).toBe(
        'Prüfverfahren · softwaretest.bit-inklusiv.de (öffnet in neuem Tab)',
      );
      expect(screen.getByText('Priorität: B – Bewertungskriterium')).toBeTruthy();
      await expectNoAxeViolations();
    });

    it('INV:CAT-02 shows the priority section and the note in the details (O-S4-33)', async () => {
      const { click } = await show(procedureCriterion());

      await click();

      expect(
        screen.getByRole('heading', { level: 3, name: 'Warum diese Priorität?' }),
      ).toBeTruthy();
      expect(screen.getByText('Rationale PROC-softwareTest')).toBeTruthy();
      expect(
        screen.getByText('Das ist die allgemeine Empfehlung für alle Anwendungstypen.'),
      ).toBeTruthy();
      expect(screen.getByText(SOURCE_LINE)).toBeTruthy();
      expect(screen.getByText(/Im Beschaffungsprojekt ergibt sich Ja oder Nein/)).toBeTruthy();
      await expectNoAxeViolations();
    });

    it('has no catalog sections in the details', async () => {
      const { click } = await show(procedureCriterion());

      await click();

      expect(h3Names()).toEqual(['Warum diese Priorität?']);
    });

    it('leaves out the meta line when the options name no URL for the test type', async () => {
      const { container } = await show(procedureCriterion(), DEFAULT_DECISION, {
        ...names(),
        testTypeUrls: {},
      });

      expect(container.textContent).not.toContain('Prüfverfahren ·');
      expect(screen.queryByRole('link')).toBeNull();
    });
  });

  describe('catalog sections of the details (S4-15b, Handoff §5.5)', () => {
    it('shows the sections of one procedure in order, without tabs', async () => {
      const { click } = await show(
        keyboardCriterion({
          procedureTexts: [softwareText()],
          appliesTo: [
            { id: 'web', name: 'Website oder Webanwendung' },
            { id: 'softwareOpen', name: 'Software (offen)' },
          ],
        }),
      );

      await click();

      expect(h3Names()).toEqual([
        'Warum diese Priorität?',
        'Was wird verlangt?',
        'Wann gilt das?',
        'Wen betrifft es?',
        'Woran erkennt man es?',
        'Wie wird bewertet?',
        'So wird es geprüft',
        'Gilt für',
      ]);
      expect(screen.getByText('Alle Funktionen sind mit der Tastatur bedienbar.').tagName).toBe(
        'P',
      );
      expect(screen.getByText('Menüs').tagName).toBe('LI');
      expect(
        screen.getAllByRole('heading', { level: 4 }).map((h) => h.textContent?.trim()),
      ).toEqual(['So ist es gut', 'So ist es nicht gut']);
      const ratings = screen.getByText('Erfüllt').parentElement as HTMLElement;
      expect(listItems(ratings)).toEqual(['Erfüllt', 'Barriere', 'Blockade', 'Nicht anwendbar']);
      const step = screen.getByRole('link', {
        name: 'Prüfschritt 11.2.1.1 im BIT inklusiv Software-Test (öffnet bit-inklusiv.de) (öffnet in neuem Tab)',
      });
      expect(step.getAttribute('href')).toBe('https://softwaretest.bit-inklusiv.de/#ohne-maus');
      expect(step.getAttribute('target')).toBe('_blank');
      expect(screen.getByText('Website oder Webanwendung und Software (offen)')).toBeTruthy();
      expect(
        screen.getByText(
          'Texte: BIT inklusiv Software-Test, Prüfschritt 11.2.1.1 „Ohne Maus nutzbar“. Verwendet mit Erlaubnis.',
        ),
      ).toBeTruthy();
      expect(screen.queryByRole('tablist')).toBeNull();
      await expectNoAxeViolations();
    });

    it('leaves out sections without text, „Gilt für“ without app types and a step link that is not http(s)', async () => {
      const { click } = await show(
        keyboardCriterion({
          procedureTexts: [
            softwareText({
              applicability: null,
              positiveExamples: null,
              negativeExamples: null,
              allowedRatings: [],
              sourceUrl: 'javascript:alert(1)',
            }),
          ],
          appliesTo: [],
        }),
      );

      await click();

      expect(h3Names()).toEqual([
        'Warum diese Priorität?',
        'Was wird verlangt?',
        'Wen betrifft es?',
      ]);
      expect(screen.queryByRole('link')).toBeNull();
    });

    it('shows only priority and „Gilt für“ for a criterion without procedure texts', async () => {
      const { click } = await show(keyboardCriterion({ procedureTexts: [] }));

      await click();

      expect(h3Names()).toEqual(['Warum diese Priorität?', 'Gilt für']);
      expect(screen.queryByText(/^Texte:/)).toBeNull();
    });

    it('puts sections with the same text above one tab per procedure (U101)', async () => {
      const { click, fixture } = await show(
        keyboardCriterion({
          procedureTexts: [
            softwareText(),
            softwareText({
              testType: 'appTest',
              originalTitle: 'Ohne Maus bedienbar',
              requirement: 'Alle Funktionen der App sind mit externer Tastatur bedienbar.',
              sourceUrl: 'https://apptest.bit-inklusiv.de/#ohne-maus',
            }),
          ],
        }),
      );

      await click();

      const tabs = screen.getAllByRole('tab');
      expect(tabs.map((tab) => tab.textContent?.trim())).toEqual(['Software-Test', 'App-Test']);
      expect(h3Names()).toEqual([
        'Warum diese Priorität?',
        'Wann gilt das?',
        'Wen betrifft es?',
        'Woran erkennt man es?',
        'Wie wird bewertet?',
        'Was wird verlangt?',
        'So wird es geprüft',
        'Gilt für',
      ]);
      expect(screen.getByText('Alle Funktionen sind mit der Tastatur bedienbar.')).toBeTruthy();
      expect(screen.getByText(/Prüfschritt 11\.2\.1\.1 „Ohne Maus nutzbar“/)).toBeTruthy();
      await expectNoAxeViolations();

      tabs[1]?.click();
      await fixture.whenStable();

      expect(
        screen.getByText('Alle Funktionen der App sind mit externer Tastatur bedienbar.'),
      ).toBeTruthy();
      expect(
        screen.getByRole('link', {
          name: 'Prüfschritt 11.2.1.1 im BIT inklusiv App-Test (öffnet bit-inklusiv.de) (öffnet in neuem Tab)',
        }),
      ).toBeTruthy();
      expect(
        screen.getByText(/BIT inklusiv App-Test, Prüfschritt 11\.2\.1\.1 „Ohne Maus bedienbar“/),
      ).toBeTruthy();
    });

    it('shows three examples per group, then all on request with the focus on the fourth (O-S4-37)', async () => {
      const { click, fixture } = await show(
        keyboardCriterion({
          procedureTexts: [
            softwareText({
              positiveExamples:
                '- Gut 1\n- Gut 2\n  - Gut 2a\n- Gut 3\n- Gut 4\n- Gut 5\n\nEine Notiz zu den Beispielen.',
              negativeExamples: '- Schlecht 1',
            }),
          ],
        }),
      );
      await click();

      expect(screen.queryByText('Gut 4')).toBeNull();
      expect(screen.getByText('Gut 2a')).toBeTruthy();
      expect(screen.getByText('Eine Notiz zu den Beispielen.')).toBeTruthy();
      const more = screen.getByRole('button', { name: 'Alle 5 Beispiele anzeigen' });
      const describedBy = (more.getAttribute('aria-describedby') ?? '').split(' ');
      expect(describedBy.map((id) => document.getElementById(id)?.textContent?.trim())).toEqual([
        'So ist es gut',
        'Formulare lassen sich vollständig mit der Tastatur bedienen',
      ]);
      expect(screen.getAllByRole('button', { name: /Beispiele anzeigen/ })).toHaveLength(1);
      await expectNoAxeViolations();

      more.click();
      await fixture.whenStable();

      expect(screen.getByText('Gut 5')).toBeTruthy();
      expect(screen.queryByRole('button', { name: /Beispiele anzeigen/ })).toBeNull();
      expect(document.activeElement?.textContent?.trim()).toBe('Gut 4');
      await expectNoAxeViolations();
    });
  });

  describe('include and exclude (R-LST-04)', () => {
    it('names the checkbox „In Liste aufnehmen“ with the criterion title and reports a change', async () => {
      const { events, act } = await show(keyboardCriterion());
      const checkbox = screen.getByRole('checkbox', {
        name: 'In Liste aufnehmen Formulare lassen sich vollständig mit der Tastatur bedienen',
      });
      expect((checkbox as HTMLInputElement).checked).toBe(true);
      expect(screen.queryByText('Nicht in Ihrer Liste')).toBeNull();

      await act(checkbox);

      expect(events.includedChange).toHaveBeenCalledWith(false);
    });

    it('keeps an excluded criterion as a marked card with every control', async () => {
      const { container, events, act } = await show(keyboardCriterion(), { ...DEFAULT_DECISION, included: false });

      const checkbox = screen.getByRole('checkbox', { name: /^In Liste aufnehmen/ });
      expect((checkbox as HTMLInputElement).checked).toBe(false);
      expect(screen.getByText('Nicht in Ihrer Liste')).toBeTruthy();
      expect(container.classList).toContain('pa-criterion-card--excluded');
      expect(screen.getByRole('button', { name: /^Priorität:/ })).toBeTruthy();
      expect(screen.getByRole('button', { name: 'Details anzeigen' })).toBeTruthy();
      expect(screen.getByRole('button', { name: 'Notiz hinzufügen' })).toBeTruthy();
      await expectNoAxeViolations();

      await act(checkbox);

      expect(events.includedChange).toHaveBeenCalledWith(true);
    });

    it('lets a „Geprüft nach …“ card be deselected too', async () => {
      await show(procedureCriterion());

      expect(
        screen.getByRole('checkbox', { name: 'In Liste aufnehmen Geprüft nach BIT inklusiv Software-Test' }),
      ).toBeTruthy();
    });
  });

  describe('priority override (R-LST-03)', () => {
    it('reports a level chosen in the K5 menu', async () => {
      const { events, act } = await show(keyboardCriterion());
      const levelButton = screen.getByRole('button', { name: /^Priorität:/ });
      expect(levelButton.getAttribute('aria-describedby')).toBe('k-11-2-1-1');

      await act(levelButton);
      await act(screen.getByRole('menuitemradio', { name: 'Hinweis' }));

      expect(events.levelChange).toHaveBeenCalledWith('Info');
    });

    it('reports a chosen weight and „Empfehlung wiederherstellen“', async () => {
      const { events, act } = await show(criterion('11.2.2', 'B', 3), { ...DEFAULT_DECISION, overrideWeight: 5 });

      expect(screen.getByText('Von Ihnen geändert. Empfohlen war: B – Bewertungskriterium, Gewichtung 3.')).toBeTruthy();
      await act(screen.getByRole('combobox', { name: 'Gewichtung' }));
      await act(screen.getByRole('option', { name: '8' }));
      expect(events.weightChange).toHaveBeenCalledWith(8);

      await act(screen.getByRole('button', { name: 'Empfehlung wiederherstellen' }));
      expect(events.priorityReset).toHaveBeenCalledTimes(1);
    });
  });

  describe('note (R-LST-05, O-S4-42, O-S4-43)', () => {
    function noteField(): HTMLTextAreaElement {
      return screen.getByRole<HTMLTextAreaElement>('textbox', { name: 'Ihre Notiz zu diesem Kriterium (optional)' });
    }

    async function type(text: string, fixture: { whenStable(): Promise<unknown> }): Promise<void> {
      const field = noteField();
      field.value = text;
      fireEvent.input(field);
      await fixture.whenStable();
    }

    it('opens the note field from „Notiz hinzufügen“ with the focus in it', async () => {
      const { container, act } = await show(keyboardCriterion());
      const button = screen.getByRole('button', { name: 'Notiz hinzufügen' });
      expect(button.getAttribute('aria-expanded')).toBe('false');
      expect(button.getAttribute('aria-describedby')).toBe('k-11-2-1-1');
      const region = container.querySelector(`#${button.getAttribute('aria-controls')}`);
      expect(region?.hasAttribute('hidden')).toBe(true);

      await act(button);

      expect(button.getAttribute('aria-expanded')).toBe('true');
      expect(region?.hasAttribute('hidden')).toBe(false);
      expect(document.activeElement).toBe(noteField());
      expect(noteField().getAttribute('aria-describedby')).toContain('k-11-2-1-1');
      await expectNoAxeViolations();
    });

    it('reports the note as typed', async () => {
      const { events, act, fixture } = await show(keyboardCriterion());
      await act(screen.getByRole('button', { name: 'Notiz hinzufügen' }));

      await type('Im Lastenheft unter 4.2 aufführen.', fixture);

      expect(events.noteChange).toHaveBeenLastCalledWith('Im Lastenheft unter 4.2 aufführen.');
    });

    it('shows an existing note open, behind „Notiz bearbeiten“', async () => {
      await show(keyboardCriterion(), { ...DEFAULT_DECISION, note: 'Schon notiert.' });

      const button = screen.getByRole('button', { name: 'Notiz bearbeiten' });
      expect(button.getAttribute('aria-expanded')).toBe('true');
      expect(noteField().value).toBe('Schon notiert.');
    });

    it('shows the length error only after the field was left, keeps the text, and clears it once it fits', async () => {
      const { events, act, fixture } = await show(keyboardCriterion());
      await act(screen.getByRole('button', { name: 'Notiz hinzufügen' }));
      const tooLong = 'x'.repeat(NOTE_MAX_LENGTH + 1);

      await type(tooLong, fixture);
      expect(screen.queryByText(/Bitte kürzen Sie den Text/)).toBeNull();
      expect(noteField().getAttribute('aria-invalid')).toBe('false');

      fireEvent.blur(noteField());
      await fixture.whenStable();

      const error = screen.getByText('Bitte kürzen Sie den Text auf höchstens 1000 Zeichen. Derzeit sind es 1001.');
      expect(noteField().getAttribute('aria-invalid')).toBe('true');
      expect(noteField().getAttribute('aria-describedby')).toContain(error.id);
      expect(noteField().value).toHaveLength(NOTE_MAX_LENGTH + 1);
      expect(noteField().hasAttribute('maxlength')).toBe(false);
      expect(events.noteChange).toHaveBeenLastCalledWith(tooLong);
      await expectNoAxeViolations();

      await type('x'.repeat(NOTE_MAX_LENGTH), fixture);

      expect(screen.queryByText(/Bitte kürzen Sie den Text/)).toBeNull();
      expect(noteField().getAttribute('aria-invalid')).toBe('false');
    });
  });
});
