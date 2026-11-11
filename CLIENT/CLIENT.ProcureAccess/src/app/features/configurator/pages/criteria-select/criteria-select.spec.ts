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
import { FilterOptionsLayers, FilterOptionsResponse } from '../../models/filter-options';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { withConfigurator } from '../../state/with-configurator';
import { excluded, filterOptions, filterOptionsAnswer } from '../../testing/filter-options-fixture';
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

const BY_HR = { layer: 'useCase', id: 'hr', name: 'HR-Anwendung' } as const;
const BY_WEB = { layer: 'appType', id: 'web', name: 'Website oder Webanwendung' } as const;
const BY_AUDIO = { layer: 'productPart', id: 'audio', name: 'Audio' } as const;

/** Steps 2–4 with catalog-like names, the T.1 hint keys and one exclusion of each kind. */
function catalog(): FilterOptionsLayers {
  let layers: FilterOptionsLayers = {
    ...options(),
    appTypes: [
      { id: 'web', name: 'Website oder Webanwendung', hint: null, excludedBy: [] },
      { id: 'softwareClosed', name: 'Software (geschlossen)', hint: 'catalog hint', excludedBy: [] },
      { id: 'mobileOpen', name: 'Mobile Anwendung (offen)', hint: 'catalog hint', excludedBy: [] },
      { id: 'document', name: 'Dokument', hint: null, excludedBy: [] },
    ],
    productParts: [
      { id: 'base', name: 'Basisfunktionalität', explanation: 'Gilt in jeder Anwendung.', alwaysSelected: true, excludedBy: [] },
      { id: 'audio', name: 'Audio', explanation: 'Wiedergabe von Tönen.', alwaysSelected: false, excludedBy: [] },
      { id: 'video', name: 'Video', explanation: 'Wiedergabe von Bildern.', alwaysSelected: false, excludedBy: [] },
    ],
    testTypes: [
      { id: 'softwareTest', name: 'BIT inklusiv Software-Test', url: 'https://softwaretest.bit-inklusiv.de/', excludedBy: [] },
      { id: 'pdfTest', name: 'BIT inklusiv PDF-Test', url: 'https://pdftest.bit-inklusiv.de/', excludedBy: [] },
      { id: 'bikWeb', name: 'BIK BITV-Test (Web)', url: 'https://www.bitvtest.de/pruefverfahren/bitv-20-web', excludedBy: [] },
    ],
  };
  layers = excluded(layers, 'appTypes', 'document', BY_HR);
  layers = excluded(layers, 'productParts', 'video', BY_HR);
  layers = excluded(layers, 'testTypes', 'softwareTest', BY_AUDIO);
  layers = excluded(layers, 'testTypes', 'pdfTest', BY_HR, BY_WEB);
  return layers;
}

describe('CriteriaSelect (S10, R-CON-01 – R-CON-06)', () => {
  let fixture: ComponentFixture<CriteriaSelect>;
  let store: InstanceType<typeof ConfiguratorTestStore>;
  let announce: ReturnType<typeof vi.fn>;

  let api: { getFilterOptions: ReturnType<typeof vi.fn> };

  async function setUp({ wide = true, layers = options() } = {}) {
    announce = vi.fn().mockResolvedValue(undefined);
    api = { getFilterOptions: vi.fn().mockResolvedValue(filterOptionsAnswer(layers)) };
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

  describe('steps 2–4 (R-CON-03 – R-CON-06)', () => {
    const checkbox = (name: string) => screen.getByRole<HTMLInputElement>('checkbox', { name });
    const group = (name: string) => screen.getByRole('group', { name });

    /** Answers step 1 with HR-Anwendung and opens step 2. */
    async function toStep2(): Promise<void> {
      await setUp({ layers: catalog() });
      await choose('HR-Anwendung');
      button('Weiter').click();
      await fixture.whenStable();
    }

    async function toStep3(): Promise<void> {
      await toStep2();
      checkbox('Website oder Webanwendung').click();
      await fixture.whenStable();
      button('Weiter').click();
      await fixture.whenStable();
    }

    async function toStep4(): Promise<void> {
      await toStep3();
      checkbox('Audio').click();
      await fixture.whenStable();
      button('Weiter').click();
      await fixture.whenStable();
    }

    describe('step 2: app types', () => {
      it('is a checkbox group named by its question, with the offen/geschlossen hints (R-CON-03)', async () => {
        await toStep2();

        expect(group('Was möchten Sie beschaffen?')).toBeTruthy();
        expect(screen.getByRole('checkbox', {
          name: 'Software (geschlossen)',
          description: 'Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden, etwa bei Kiosk-Terminals.',
        })).toBeTruthy();
        expect(screen.getByRole('checkbox', {
          name: 'Mobile Anwendung (offen)',
          description: 'Eigene Hilfsmittel wie Screenreader lassen sich verwenden.',
        })).toBeTruthy();
        expect(screen.getByRole('checkbox', { name: 'Website oder Webanwendung', description: '' })).toBeTruthy();
        await expectNoAxeViolations();
      });

      it('adds the second sentence of the hint while an app type is excluded (UX copy U99)', async () => {
        await toStep2();

        expect(screen.getByText(/Einige passen nicht zum gewählten Verwendungszweck/)).toBeTruthy();
      });

      it('shows only the first sentence of the hint when nothing is excluded', async () => {
        await setUp();
        await choose('HR-Anwendung');
        button('Weiter').click();
        await fixture.whenStable();
        expect(screen.getByText(/Wählen Sie alle Anwendungstypen, die zutreffen\./)).toBeTruthy();
        expect(screen.queryByText(/Einige passen nicht/)).toBeNull();
      });

      it('INV:CFG-02 keeps an excluded app type focusable and aria-disabled with its reason; activating it changes nothing', async () => {
        await toStep2();
        const document_ = checkbox('Dokument');

        expect(document_.getAttribute('aria-disabled')).toBe('true');
        expect(document_.disabled).toBe(false);
        expect(screen.getByRole('checkbox', { name: 'Dokument', description: 'Passt nicht zum Verwendungszweck HR-Anwendung' })).toBe(document_);
        document_.focus();
        expect(document.activeElement).toBe(document_);

        document_.click();
        await fixture.whenStable();

        expect(document_.checked).toBe(false);
        expect(store.configurator.draft().appTypeIds).toEqual([]);
      });

      it('checks and unchecks an available app type', async () => {
        await toStep2();

        checkbox('Website oder Webanwendung').click();
        await fixture.whenStable();
        expect(checkbox('Website oder Webanwendung').checked).toBe(true);
        expect(store.configurator.draft().appTypeIds).toEqual(['web']);

        checkbox('Website oder Webanwendung').click();
        await fixture.whenStable();
        expect(checkbox('Website oder Webanwendung').checked).toBe(false);
        expect(store.configurator.draft().appTypeIds).toEqual([]);
      });

      it('„Weiter“ without an app type shows the error under the group and focuses its first available option', async () => {
        await toStep2();

        button('Weiter').click();
        await fixture.whenStable();

        const error = 'Bitte wählen Sie mindestens einen Anwendungstyp.';
        expect(screen.getByText(error)).toBeTruthy();
        expect(screen.getByRole('group', { name: 'Was möchten Sie beschaffen?', description: new RegExp(error) })).toBeTruthy();
        expect(document.activeElement).toBe(checkbox('Website oder Webanwendung'));
        expect(stepItems()[1].getAttribute('aria-current')).toBe('step');
        await expectNoAxeViolations();

        checkbox('Software (geschlossen)').click();
        await fixture.whenStable();
        expect(screen.queryByText(error)).toBeNull();
      });

      it('„Weiter“ with an app type opens step 3', async () => {
        await toStep3();

        expect(document.activeElement).toBe(screen.getByRole('heading', { level: 2, name: 'Welche Funktionen hat das Produkt?' }));
        expect(stepItems()[2].getAttribute('aria-current')).toBe('step');
      });
    });

    describe('step 3: product parts', () => {
      it('INV:CFG-03 Basisfunktionalität is checked, aria-disabled, „Immer ausgewählt.“ and cannot be unchecked', async () => {
        await toStep3();
        const base = screen.getByRole<HTMLInputElement>('checkbox', { name: 'Basisfunktionalität', description: 'Immer ausgewählt.' });

        expect(base.checked).toBe(true);
        expect(base.getAttribute('aria-disabled')).toBe('true');
        expect(base.disabled).toBe(false);

        base.click();
        await fixture.whenStable();

        expect(base.checked).toBe(true);
        expect(store.configurator.draft().productPartIds).toEqual(['base']);
      });

      it('„Erklärungen anzeigen“ is off by default and shows each catalog explanation, linked to its option (R-CON-04)', async () => {
        await toStep3();
        const toggle = screen.getByRole('switch', { name: 'Erklärungen anzeigen' });
        expect(toggle.getAttribute('aria-checked')).toBe('false');
        expect(screen.queryByText('Wiedergabe von Tönen.')).toBeNull();

        toggle.click();
        await fixture.whenStable();

        expect(screen.getByRole('checkbox', { name: 'Audio', description: 'Wiedergabe von Tönen.' })).toBeTruthy();
        expect(screen.getByRole('checkbox', {
          name: 'Basisfunktionalität',
          description: 'Immer ausgewählt. Gilt in jeder Anwendung.',
        })).toBeTruthy();
        // An excluded option shows its reason in place of the explanation.
        expect(screen.getByRole('checkbox', { name: 'Video', description: 'Passt nicht zum Verwendungszweck HR-Anwendung' })).toBeTruthy();
        await expectNoAxeViolations();
      });

      it('INV:CFG-02 keeps an excluded product part focusable and unchanged when activated', async () => {
        await toStep3();
        const video = checkbox('Video');

        expect(video.getAttribute('aria-disabled')).toBe('true');
        video.click();
        await fixture.whenStable();

        expect(video.checked).toBe(false);
        expect(store.configurator.draft().productPartIds).toEqual(['base']);
      });
    });

    describe('step 4: test types', () => {
      it('INV:CFG-04 can stay empty: nothing in the group is required and the selection is complete', async () => {
        await toStep4();

        expect(group('Soll das Produkt nach einem bestimmten Verfahren geprüft sein? (optional)')).toBeTruthy();
        expect(screen.getAllByRole('checkbox').every((box) => !(box as HTMLInputElement).required)).toBe(true);
        expect(store.configurator.draft().testTypeIds).toEqual([]);
        expect(store.configuratorComplete()).toBe(true);
      });

      it('shows the hint and a link to the procedure that opens in a new tab (R-CON-05)', async () => {
        await toStep4();

        expect(screen.getByRole('checkbox', { name: 'BIK BITV-Test (Web)', description: 'Für Websites und Webanwendungen.' })).toBeTruthy();
        const link = screen.getByRole('link', { name: 'Verfahren ansehen (bitvtest.de) (öffnet in neuem Tab)' });
        expect(link.getAttribute('href')).toBe('https://www.bitvtest.de/pruefverfahren/bitv-20-web');
        expect(link.getAttribute('target')).toBe('_blank');
        expect(link.getAttribute('rel')).toBe('noopener noreferrer');
      });

      it('INV:CFG-02 names every value that excludes a test type, the product part in a line of its own', async () => {
        await toStep4();

        const software = screen.getByRole<HTMLInputElement>('checkbox', {
          name: 'BIT inklusiv Software-Test',
          description: 'Passt nicht zum Funktionsbereich Audio',
        });
        expect(screen.getByRole('checkbox', {
          name: 'BIT inklusiv PDF-Test',
          description: 'Passt nicht zum Verwendungszweck HR-Anwendung und nicht zum Anwendungstyp Website oder Webanwendung',
        })).toBeTruthy();
        // Excluded: no hint and no link in place of the reason.
        expect(screen.getAllByRole('link')).toHaveLength(1);

        software.click();
        await fixture.whenStable();
        expect(store.configurator.draft().testTypeIds).toEqual([]);

        checkbox('BIK BITV-Test (Web)').click();
        await fixture.whenStable();
        expect(store.configurator.draft().testTypeIds).toEqual(['bikWeb']);
        await expectNoAxeViolations();
      });
    });

    describe('„Weiter“ and reloads', () => {
      it('waits for the options of the new selection before it opens the next step', async () => {
        await setUp({ layers: catalog() });
        let answer!: (value: FilterOptionsResponse) => void;
        api.getFilterOptions.mockReturnValueOnce(
          new Promise<FilterOptionsResponse>((resolve) => {
            answer = resolve;
          }),
        );
        await choose('HR-Anwendung');

        button('Weiter').click();
        await fixture.whenStable();
        expect(stepItems()[0].getAttribute('aria-current')).toBe('step');

        answer(filterOptionsAnswer(catalog()));
        // The page goes on after the store applied the answer: let that promise chain finish.
        await new Promise((resolve) => setTimeout(resolve));
        await fixture.whenStable();
        expect(stepItems()[1].getAttribute('aria-current')).toBe('step');
      });

      it('moves on by one step only when „Weiter“ is pressed twice during the wait', async () => {
        await toStep2();
        let answer!: (value: FilterOptionsResponse) => void;
        api.getFilterOptions.mockReturnValueOnce(
          new Promise<FilterOptionsResponse>((resolve) => {
            answer = resolve;
          }),
        );
        checkbox('Website oder Webanwendung').click();
        await fixture.whenStable();

        button('Weiter').click();
        button('Weiter').click();
        answer(filterOptionsAnswer(catalog()));
        await new Promise((resolve) => setTimeout(resolve));
        await fixture.whenStable();

        // Step 3 is always complete, so a second step change would land on step 4.
        expect(stepItems()[2].getAttribute('aria-current')).toBe('step');
      });

      it('stays on the step when the reload failed', async () => {
        await setUp({ layers: catalog() });
        api.getFilterOptions.mockRejectedValueOnce(new Error('offline'));
        await choose('HR-Anwendung');

        button('Weiter').click();
        await fixture.whenStable();

        expect(stepItems()[0].getAttribute('aria-current')).toBe('step');
      });

      it('keeps showing the chosen use case when a later answer no longer lists it', async () => {
        await setUp({ layers: catalog() });
        api.getFilterOptions.mockResolvedValue(
          filterOptionsAnswer({ ...catalog(), useCases: catalog().useCases.filter((u) => u.id !== 'hr') }),
        );

        await choose('HR-Anwendung');

        expect(store.configurator.options()?.useCases.map((u) => u.id)).not.toContain('hr');
        expect((combobox() as HTMLInputElement).value).toBe('HR-Anwendung');
      });
    });
  });
});
