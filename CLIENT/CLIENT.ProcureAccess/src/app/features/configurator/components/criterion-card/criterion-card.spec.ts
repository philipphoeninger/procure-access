// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Criterion } from '../../models/criteria';
import { CriterionDecision, DEFAULT_DECISION } from '../../models/criteria-list';
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
    productParts: [{ id: 'base', name: 'Basisfunktionalität', explanation: '', alwaysSelected: true, excludedBy: [] }],
    testTypes: [{ id: 'softwareTest', name: 'Software', url: 'https://softwaretest.bit-inklusiv.de/', excludedBy: [] }],
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
    ...criterion('PROC-softwareTest', 'B', null, { title: 'Geprüft nach Softwaretest', plainLanguage: null }),
    kind: 'testProcedure',
    number: null,
    productPart: null,
    standardReferences: [],
    allowedRatings: [],
  };
}

async function show(c: Criterion, decision: CriterionDecision = DEFAULT_DECISION, catalog: CatalogNames = names()) {
  const rendered = await render(CriterionCard, {
    providers: [provideTestTranslations()],
    inputs: { criterion: c, decision, names: catalog },
  });
  const toggle = () => screen.getByRole('button', { name: /Details (anzeigen|ausblenden)/ });
  async function click(): Promise<void> {
    toggle().click();
    await rendered.fixture.whenStable();
  }
  return { ...rendered, toggle, click };
}

describe('CriterionCard (S11, R-LST-01, R-LST-02, Handoff §5.5)', () => {
  describe('requirement', () => {
    it('shows title as H2, source title, meta line and K5, compact', async () => {
      const { toggle } = await show(keyboardCriterion());

      expect(
        screen.getByRole('heading', { level: 2, name: 'Formulare lassen sich vollständig mit der Tastatur bedienen' }),
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
      expect(screen.getByText('Viele Mitarbeitende arbeiten ausschließlich mit der Tastatur.')).toBeTruthy();
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
      expect(screen.getByText('Von Ihnen geändert. Empfohlen war: A – Mindestanforderung.')).toBeTruthy();
    });

    it('shows the weight of a recommended B', async () => {
      await show(criterion('11.2.2', 'B', 3));

      expect(screen.getByText('Gewichtung 3')).toBeTruthy();
    });
  });

  describe('„Geprüft nach …“ (R-LST-07)', () => {
    it('shows title, text and the test type link that opens in a new tab', async () => {
      await show(procedureCriterion());

      expect(screen.getByRole('heading', { level: 2, name: 'Geprüft nach BIT inklusiv Software-Test' })).toBeTruthy();
      expect(
        screen.getByText(
          'Der Anbieter oder eine unabhängige Stelle hat das Produkt nach dem BIT inklusiv Software-Test geprüft und kann den Bericht vorlegen.',
        ),
      ).toBeTruthy();
      const link = screen.getByRole('link', { name: 'softwaretest.bit-inklusiv.de (öffnet in neuem Tab)' });
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

      expect(screen.getByRole('heading', { level: 3, name: 'Warum diese Priorität?' })).toBeTruthy();
      expect(screen.getByText('Rationale PROC-softwareTest')).toBeTruthy();
      expect(screen.getByText('Das ist die allgemeine Empfehlung für alle Anwendungstypen.')).toBeTruthy();
      expect(screen.getByText(SOURCE_LINE)).toBeTruthy();
      expect(screen.getByText(/Im Beschaffungsprojekt ergibt sich Ja oder Nein/)).toBeTruthy();
      await expectNoAxeViolations();
    });

    it('leaves out the meta line when the options name no URL for the test type', async () => {
      const { container } = await show(procedureCriterion(), DEFAULT_DECISION, { ...names(), testTypeUrls: {} });

      expect(container.textContent).not.toContain('Prüfverfahren ·');
      expect(screen.queryByRole('link')).toBeNull();
    });
  });
});
