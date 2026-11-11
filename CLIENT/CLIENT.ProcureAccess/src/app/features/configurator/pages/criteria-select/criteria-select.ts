// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { CdkStepperModule } from '@angular/cdk/stepper';
import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { injectWideLayout } from '@app/layout/shell/wide-layout';
import { Stepper } from '@app/shared/components/stepper/stepper';
import { FilterOptionCheckbox, OptionLink } from '../../components/filter-option/filter-option';
import { FilterOption, TestTypeOption, UseCaseOption } from '../../models/filter-options';
import { exclusionReasonLines, isExcluded, ReasonLine, stepError } from '../../state/selection-rules';

/** „Keiner davon passt“: its own answer of step 1, not a use case (R-CON-02). */
interface NoneEntry {
  kind: 'none';
}

/** What the combobox holds: typed text, or the chosen entry. */
type UseCaseEntry = UseCaseOption | NoneEntry;

const NONE: NoneEntry = { kind: 'none' };

/** Handoff §3.4: counters are announced at most once per second. */
const MATCHES_THROTTLE_MS = 1000;

/**
 * Hint lines of step 2 by app-type key (UX copy §5.4, `criteria.select.step2.*.hint`); the app
 * types without one have no hint. The catalog `hint` of the answer holds the same text and is not
 * shown, so the wording has one source.
 */
const APP_TYPE_HINT_KEYS: Readonly<Record<string, string>> = {
  softwareOpen: 'criteria.select.step2.open.hint',
  mobileOpen: 'criteria.select.step2.open.hint',
  softwareClosed: 'criteria.select.step2.closed.hint',
  mobileClosed: 'criteria.select.step2.closedMobile.hint',
};

/** Hint lines of step 4 by test-type key (UX copy §5.4, `testType.*.hint`). */
const TEST_TYPE_HINT_KEYS: Readonly<Record<string, string>> = {
  softwareTest: 'testType.softwareTest.hint',
  appTest: 'testType.appTest.hint',
  pdfTest: 'testType.pdfTest.hint',
  bikWeb: 'testType.bikWeb.hint',
  bikApp: 'testType.bikApp.hint',
};

/** The listbox keeps room for about three options, even when the keyboard leaves less (Handoff §2.2). */
const MIN_PANEL_HEIGHT_PX = 144;

function isNone(entry: UseCaseEntry): entry is NoneEntry {
  return 'kind' in entry && entry.kind === 'none';
}

/** The domain of the link text „Verfahren ansehen ({{domain}})“; null for a URL that cannot be read. */
function linkTo(url: string): OptionLink | null {
  try {
    return { url, domain: new URL(url).hostname.replace(/^www\./, '') };
  } catch {
    return null;
  }
}

function normalize(text: string): string {
  return text.trim().toLocaleLowerCase('de');
}

/**
 * S10 Kriterien zusammenstellen (R-CON-01 – R-CON-06): H1, intro and the linear four-step
 * `pa-stepper`. Step 1 is the single-choice use-case combobox: it filters while typing, keeps
 * „Keiner davon passt“ as the last option also while filtering, announces the number of matches
 * (throttled) and fits its listbox above the on-screen keyboard. „Weiter“ without an answer shows
 * the field error of S10-E and moves the focus into the field.
 *
 * Steps 2–4 are checkbox groups (`fieldset`, the question as `legend`) of `pa-filter-option`:
 * app types with the offen/geschlossen hints and at least one required, product parts with the
 * locked Basisfunktionalität and „Erklärungen anzeigen“, optional test types with their link.
 * Excluded options stay in place with their reason (R-CON-06). „Weiter“ waits until the options
 * answer the current selection, so the next step never offers outdated choices.
 *
 * The reset notice, „Bisherige Auswahl“, the submit, the page title and the error title prefix
 * come with S4-12. The selection lives in `withConfigurator()` (R-CON-09).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2134-3174
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2019-407
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2139-3332 (error)
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2086-2142 (step 2)
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2088-2200 (step 3)
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2089-2259 (step 4)
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2103-2940 (320 px)
 * @spec docs/SPEC_v2.md R-CON-01 – R-CON-06; docs/design_handoff_procureaccess_v1.md §2.2, §5.1, §5.4
 * @invariants CFG-02, CFG-03, CFG-04, CAT-12
 */
@Component({
  selector: 'pa-criteria-select',
  imports: [
    CdkStepperModule,
    FilterOptionCheckbox,
    MatAutocompleteModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    ReactiveFormsModule,
    Stepper,
    TranslatePipe,
  ],
  templateUrl: './criteria-select.html',
  styleUrl: './criteria-select.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriteriaSelect {
  private readonly store = inject(ProcureAccessStore);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly document = inject(DOCUMENT);
  private readonly window = this.document.defaultView;
  /** Joins several names of one layer in a reason line („A oder B“). */
  private readonly names = new Intl.ListFormat(this.document.documentElement.lang || 'de', {
    type: 'disjunction',
  });

  private readonly stepper = viewChild.required(Stepper);
  private readonly useCaseInput = viewChild<ElementRef<HTMLInputElement>>('useCaseInput');
  private readonly useCaseTrigger = viewChild(MatAutocompleteTrigger);
  private readonly appTypeGroup = viewChild<ElementRef<HTMLFieldSetElement>>('appTypeGroup');

  protected readonly NONE = NONE;
  protected readonly wide = injectWideLayout();

  /** „Weiter“ was pressed on step 1 without an answer: the field shows its error (S10-E). */
  private readonly useCaseChecked = signal(false);

  protected readonly useCaseField = new FormControl<string | UseCaseEntry>('', {
    nonNullable: true,
    validators: (): ValidationErrors | null => (this.useCaseErrorShown() ? { noUseCase: true } : null),
  });

  /** The text typed into the combobox; empty while it shows a chosen entry. */
  private readonly typed = signal('');

  /** The use case chosen last, shown also when a later answer no longer lists it (S4-10 R1). */
  private readonly chosenUseCase = signal<UseCaseOption | null>(null);

  /** „Weiter“ was pressed on step 2 without an app type (`criteria.select.error.noAppType`). */
  private readonly appTypeChecked = signal(false);

  /** „Erklärungen anzeigen“ in step 3 (R-CON-04), off by default. */
  protected readonly showExplanations = signal(false);

  private readonly draft = this.store.configurator.draft;
  protected readonly appTypes = computed(() => this.store.configurator.options()?.appTypes ?? []);
  protected readonly productParts = computed(() => this.store.configurator.options()?.productParts ?? []);
  protected readonly testTypes = computed(() => this.store.configurator.options()?.testTypes ?? []);
  protected readonly selectedAppTypes = computed(() => new Set(this.draft().appTypeIds));
  protected readonly selectedProductParts = computed(() => new Set(this.draft().productPartIds));
  protected readonly selectedTestTypes = computed(() => new Set(this.draft().testTypeIds));
  protected readonly appTypeAnswered = computed(() => stepError('appType', this.draft()) === null);
  protected readonly appTypeErrorShown = computed(() => this.appTypeChecked() && !this.appTypeAnswered());
  /** The second sentence of the step-2 hint, only while an app type is excluded (UX copy U99). */
  protected readonly someAppTypeExcluded = computed(() => this.appTypes().some(isExcluded));

  /** Reason lines by option id, one map for every option of steps 2–4 (ids are unique per layer). */
  protected readonly reasons = computed(() => ({
    appType: this.reasonsOf(this.appTypes()),
    productPart: this.reasonsOf(this.productParts()),
    testType: this.reasonsOf(this.testTypes()),
  }));

  protected readonly useCases = computed(() => this.store.configurator.options()?.useCases ?? []);
  protected readonly optionsLoaded = computed(() => this.store.configurator.options() !== null);
  protected readonly matches = computed(() => this.filter(this.typed()));
  protected readonly query = computed(() => this.typed().trim());
  protected readonly useCaseAnswered = computed(
    () => stepError('useCase', this.store.configurator.draft()) === null,
  );

  /** The entry of the current answer, as the combobox shows it; null while unanswered. */
  private readonly currentEntry = computed<UseCaseEntry | null>(() => {
    const choice = this.store.configurator.draft().useCase;
    if (choice.kind === 'none') return NONE;
    if (choice.kind === 'useCase') {
      const chosen = this.chosenUseCase();
      return this.useCases().find((u) => u.id === choice.id) ?? (chosen?.id === choice.id ? chosen : null);
    }
    return null;
  });

  protected readonly displayEntry = (entry: string | UseCaseEntry | null): string => {
    if (entry === null || typeof entry === 'string') return entry ?? '';
    return isNone(entry) ? this.translate.instant('criteria.select.step1.none') as string : entry.name;
  };

  constructor() {
    const status = this.store.configurator.optionsStatus();
    if (status === 'notLoaded' || status === 'failed') void this.store.loadFilterOptions();

    // The field follows the answer in the store, e.g. when the page opens with a kept draft.
    effect(() => {
      const entry = this.currentEntry();
      untracked(() => {
        if (!this.sameEntry(this.useCaseField.value, entry)) this.showEntry(entry);
      });
    });

    const viewport = this.window?.visualViewport;
    if (viewport) {
      const fit = () => this.fitPanelToVisualViewport();
      viewport.addEventListener('resize', fit);
      inject(DestroyRef).onDestroy(() => viewport.removeEventListener('resize', fit));
    }
  }

  /** User input only: filter the list and announce how many use cases match. */
  protected onUseCaseInput(text: string): void {
    this.typed.set(text);
    void this.announcer.announce(
      'criteria.select.step1.matches',
      { count: this.filter(text).length },
      { throttleMs: MATCHES_THROTTLE_MS },
    );
  }

  protected chooseUseCase(entry: UseCaseEntry): void {
    if (!isNone(entry)) this.chosenUseCase.set(entry);
    this.store.chooseUseCase(isNone(entry) ? { kind: 'none' } : { kind: 'useCase', id: entry.id });
    this.typed.set('');
    this.useCaseField.updateValueAndValidity();
  }

  /** Text that chose nothing does not stay: the field shows the current answer again. */
  protected onPanelClosed(): void {
    const entry = this.currentEntry();
    if (!this.sameEntry(this.useCaseField.value, entry)) this.showEntry(entry);
  }

  /**
   * On a narrow screen the on-screen keyboard covers the lower half: the field moves to the top
   * of the visible area (Handoff §2.2, S10-M1). Wide screens keep the browser's own scrolling.
   */
  protected onUseCaseFocus(): void {
    if (this.wide()) return;
    this.useCaseInput()?.nativeElement.closest('mat-form-field')?.scrollIntoView({ block: 'start' });
  }

  /** The listbox ends at the bottom of the visual viewport and scrolls inside (Handoff §2.2). */
  protected fitPanelToVisualViewport(): void {
    const trigger = this.useCaseTrigger();
    const input = this.useCaseInput()?.nativeElement;
    const panel = trigger?.autocomplete.panel?.nativeElement as HTMLElement | undefined;
    const viewport = this.window?.visualViewport;
    if (!trigger?.panelOpen || !input || !panel || !viewport) return;
    const available = viewport.offsetTop + viewport.height - input.getBoundingClientRect().bottom;
    panel.style.maxHeight = `${Math.max(Math.floor(available), MIN_PANEL_HEIGHT_PX)}px`;
  }

  protected async nextFromUseCase(): Promise<void> {
    if (!this.useCaseAnswered()) {
      this.useCaseChecked.set(true);
      this.useCaseField.markAsTouched();
      this.useCaseField.updateValueAndValidity();
      this.useCaseInput()?.nativeElement.focus();
      // The error is read with the field; the list would cover it (S10-E shows it closed).
      this.useCaseTrigger()?.closePanel();
      return;
    }
    await this.nextWhenOptionsCurrent();
  }

  /** Step 2 needs at least one app type (R-CON-03): the error goes under the group, the focus to its first option. */
  protected async nextFromAppTypes(): Promise<void> {
    if (!this.appTypeAnswered()) {
      this.appTypeChecked.set(true);
      this.appTypeGroup()?.nativeElement.querySelector<HTMLInputElement>('input[type="checkbox"]:not([aria-disabled="true"])')?.focus();
      return;
    }
    await this.nextWhenOptionsCurrent();
  }

  protected async next(): Promise<void> {
    await this.nextWhenOptionsCurrent();
  }

  protected toggleAppType(id: string): void {
    this.store.toggleAppType(id);
  }

  protected toggleProductPart(id: string): void {
    this.store.toggleProductPart(id);
  }

  protected toggleTestType(id: string): void {
    this.store.toggleTestType(id);
  }

  protected appTypeHintKey(id: string): string | null {
    return APP_TYPE_HINT_KEYS[id] ?? null;
  }

  protected testTypeHintKey(id: string): string | null {
    return TEST_TYPE_HINT_KEYS[id] ?? null;
  }

  protected testTypeLink(testType: TestTypeOption): OptionLink | null {
    return linkTo(testType.url);
  }

  protected previous(): void {
    this.stepper().previous();
  }

  /**
   * A change of steps 1–3 reloads the options; „Weiter“ waits for that answer, so the next step
   * shows current exclusions and takes its choices. A failed reload keeps the page on its step.
   * Only the step that started the wait moves on: a second „Weiter“ or a „Zurück“ during the
   * wait must not skip a step.
   */
  private async nextWhenOptionsCurrent(): Promise<void> {
    const stepper = this.stepper();
    const from = stepper.selectedIndex;
    if ((await this.store.optionsCurrent()) && stepper.selectedIndex === from) stepper.next();
  }

  private reasonsOf(options: FilterOption[]): ReadonlyMap<string, ReasonLine[]> {
    const join = (names: string[]) => this.names.format(names);
    return new Map(options.map((option) => [option.id, exclusionReasonLines(option, join)]));
  }

  private useCaseErrorShown(): boolean {
    return this.useCaseChecked() && !this.useCaseAnswered();
  }

  private filter(text: string): UseCaseOption[] {
    const query = normalize(text);
    return query ? this.useCases().filter((u) => normalize(u.name).includes(query)) : this.useCases();
  }

  private showEntry(entry: UseCaseEntry | null): void {
    this.useCaseField.setValue(entry ?? '');
    this.typed.set('');
  }

  private sameEntry(value: string | UseCaseEntry, entry: UseCaseEntry | null): boolean {
    if (entry === null) return value === '';
    if (typeof value === 'string') return false;
    if (isNone(entry)) return isNone(value);
    return !isNone(value) && value.id === entry.id;
  }
}
