// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ComponentFixture } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { provideLayout } from '@app/testing/shell-test-doubles';
import { FilterOptionsLayers } from '../../models/filter-options';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { withConfigurator } from '../../state/with-configurator';
import { filterOptions, filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { CriteriaSelect } from './criteria-select';

// The real configurator slice stands in for the root store, so the page runs the S4-09 rules.
const ConfiguratorTestStore = signalStore(withConfigurator());

const NONE_OPTION = 'Keiner davon passt Es gelten die allgemeinen Empfehlungen.';

function options(): FilterOptionsLayers {
  return {
    ...filterOptions(),
    useCases: [
      { id: 'hr', name: 'HR-Anwendung', hint: null, excludedBy: [] },
      { id: 'reporting', name: 'Reporting-Anwendung', hint: null, excludedBy: [] },
      { id: 'payroll', name: 'Gehaltsabrechnungen', hint: null, excludedBy: [] },
    ],
  };
}

describe('CriteriaSelect (S10, R-CON-01, R-CON-02)', () => {
  let fixture: ComponentFixture<CriteriaSelect>;
  let store: InstanceType<typeof ConfiguratorTestStore>;
  let announce: ReturnType<typeof vi.fn>;

  async function setUp({ wide = true } = {}) {
    announce = vi.fn().mockResolvedValue(undefined);
    const api = { getFilterOptions: vi.fn().mockResolvedValue(filterOptionsAnswer(options())) };
    const rendered = await render(CriteriaSelect, {
      providers: [
        provideTestTranslations(),
        provideLayout(wide),
        { provide: ProcureAccessStore, useClass: ConfiguratorTestStore },
        { provide: CatalogApiService, useValue: api },
        { provide: A11yAnnouncerService, useValue: { announce } },
      ],
    });
    fixture = rendered.fixture;
    // The page asks for the root store; the test provides the configurator slice under its token.
    store = rendered.debugElement.injector.get<InstanceType<typeof ConfiguratorTestStore>>(ProcureAccessStore);
    await fixture.whenStable();
  }

  const combobox = () => screen.getByRole('combobox', { name: 'Verwendungszweck' });
  const optionNames = () => screen.getAllByRole('option').map((o) => text(o));
  const button = (name: string) => screen.getByRole('button', { name });
  const stepItems = () => within(screen.getByRole('list')).getAllByRole('listitem');
  const text = (element: Element) => element.textContent?.replace(/\s+/g, ' ').trim();

  /** Focus opens the list; a field that already has the focus opens it on click. */
  async function open(): Promise<void> {
    combobox().focus();
    await fixture.whenStable();
    if (combobox().getAttribute('aria-expanded') !== 'true') {
      combobox().click();
      await fixture.whenStable();
    }
  }

  async function type(text: string): Promise<void> {
    fireEvent.input(combobox(), { target: { value: text } });
    await fixture.whenStable();
  }

  async function choose(name: string | RegExp): Promise<void> {
    await open();
    screen.getByRole('option', { name }).click();
    await fixture.whenStable();
  }

  describe('frame', () => {
    it('shows the H1, the intro and the question of step 1 as H2', async () => {
      await setUp();

      expect(screen.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeTruthy();
      expect(screen.getByText(/Beantworten Sie bis zu vier Fragen zu Ihrer Beschaffung/)).toBeTruthy();
      expect(screen.getByRole('heading', { level: 2, name: 'Wofür setzen Sie das Produkt ein?' })).toBeTruthy();
    });

    it('names the four steps in fixed order, marks the current one and offers no tab to click (R-CON-01)', async () => {
      await setUp();

      expect(stepItems().map((item) => text(item))).toEqual([
        '1 Verwendungszweck',
        '2 Anwendungstyp',
        '3 Funktionsbereiche',
        '4 Prüfverfahren (optional)',
      ]);
      expect(stepItems().map((item) => item.getAttribute('aria-current'))).toEqual(['step', null, null, null]);
      expect(screen.queryAllByRole('tab')).toHaveLength(0);
      expect(within(screen.getByRole('list')).queryAllByRole('button')).toHaveLength(0);
    });

    it('shows the text line instead of the step list below 600 px (S10-M)', async () => {
      await setUp({ wide: false });

      expect(screen.queryByRole('list')).toBeNull();
      expect(screen.getByText('Schritt 1 von 4: Verwendungszweck')).toBeTruthy();
    });

    it('has no axe violations on step 1, closed and open', async () => {
      await setUp();
      await expectNoAxeViolations();

      await open();
      expect(screen.getByRole('listbox')).toBeTruthy();
      await expectNoAxeViolations();
    });
  });

  describe('step 1: use-case combobox (CFG-03)', () => {
    it('INV:CFG-03 is a combobox whose list ends with „Keiner davon passt“', async () => {
      await setUp();
      await open();

      expect(combobox().getAttribute('aria-expanded')).toBe('true');
      expect(optionNames()).toEqual(['HR-Anwendung', 'Reporting-Anwendung', 'Gehaltsabrechnungen', NONE_OPTION]);
    });

    it('INV:CFG-03 keeps „Keiner davon passt“ last while filtering', async () => {
      await setUp();
      await open();

      await type('anwendung');

      expect(optionNames()).toEqual(['HR-Anwendung', 'Reporting-Anwendung', NONE_OPTION]);
    });

    it('INV:CFG-03 without a match shows the empty text and still offers „Keiner davon passt“ last', async () => {
      await setUp();
      await open();

      await type('Kiosk');

      const [empty, none] = screen.getAllByRole('option');
      expect(text(empty)).toBe('Kein Verwendungszweck passt zu „Kiosk“. Wählen Sie „Keiner davon passt“.');
      expect(empty.getAttribute('aria-disabled')).toBe('true');
      expect(text(none)).toBe(NONE_OPTION);
      expect(screen.getAllByRole('option')).toHaveLength(2);
    });

    it('INV:CFG-03 holds one answer: a second choice replaces the first', async () => {
      await setUp();

      await choose('HR-Anwendung');
      expect(store.configurator.draft().useCase).toEqual({ kind: 'useCase', id: 'hr' });
      expect((combobox() as HTMLInputElement).value).toBe('HR-Anwendung');

      await choose(/Keiner davon passt/);
      expect(store.configurator.draft().useCase).toEqual({ kind: 'none' });
      expect((combobox() as HTMLInputElement).value).toBe('Keiner davon passt');
    });

    it('announces the number of matching use cases, throttled to once per second (R-CON-02)', async () => {
      await setUp();
      await open();

      await type('anwendung');

      expect(announce).toHaveBeenLastCalledWith('criteria.select.step1.matches', { count: 2 }, { throttleMs: 1000 });
    });

    it('shows the current answer again when typed text chose nothing', async () => {
      await setUp();
      await choose('HR-Anwendung');
      await open();
      await type('Repo');

      fireEvent.keyDown(combobox(), { key: 'Escape', keyCode: 27 });
      await fixture.whenStable();

      expect((combobox() as HTMLInputElement).value).toBe('HR-Anwendung');
      expect(store.configurator.draft().useCase).toEqual({ kind: 'useCase', id: 'hr' });
    });
  });

  describe('„Weiter“', () => {
    it('without an answer shows the field error, marks the field invalid and focuses it (S10-E)', async () => {
      await setUp();

      button('Weiter').click();
      await fixture.whenStable();

      const field = combobox();
      expect(screen.getByText('Bitte wählen Sie einen Verwendungszweck oder „Keiner davon passt“.')).toBeTruthy();
      expect(field.getAttribute('aria-invalid')).toBe('true');
      expect(document.activeElement).toBe(field);
      expect(field.getAttribute('aria-expanded')).toBe('false');
      expect(stepItems()[0].getAttribute('aria-current')).toBe('step');
      await expectNoAxeViolations();
    });

    it('drops the field error once an answer is chosen', async () => {
      await setUp();
      button('Weiter').click();
      await fixture.whenStable();

      await choose(/Keiner davon passt/);

      expect(screen.queryByText('Bitte wählen Sie einen Verwendungszweck oder „Keiner davon passt“.')).toBeNull();
      expect(combobox().getAttribute('aria-invalid')).toBe('false');
    });

    it('with an answer opens step 2, focuses its question and announces the step (R-CON-01)', async () => {
      await setUp();
      await choose('HR-Anwendung');

      button('Weiter').click();
      await fixture.whenStable();

      const question = screen.getByRole('heading', { level: 2, name: 'Was möchten Sie beschaffen?' });
      expect(document.activeElement).toBe(question);
      expect(announce).toHaveBeenCalledWith('criteria.select.stepLabel', {
        current: 2,
        total: 4,
        name: 'Anwendungstyp',
      });
      expect(stepItems().map((item) => item.getAttribute('aria-current'))).toEqual([null, 'step', null, null]);
      expect(stepItems()[0].textContent).toContain('erledigt');
      expect(stepItems()[1].textContent).not.toContain('erledigt');
      expect(screen.queryByRole('combobox')).toBeNull();
    });

    it('„Zurück“ returns to step 1 with the answer kept', async () => {
      await setUp();
      await choose('HR-Anwendung');
      button('Weiter').click();
      await fixture.whenStable();

      button('Zurück').click();
      await fixture.whenStable();

      expect(document.activeElement).toBe(screen.getByRole('heading', { level: 2, name: 'Wofür setzen Sie das Produkt ein?' }));
      expect((combobox() as HTMLInputElement).value).toBe('HR-Anwendung');
    });
  });
});
