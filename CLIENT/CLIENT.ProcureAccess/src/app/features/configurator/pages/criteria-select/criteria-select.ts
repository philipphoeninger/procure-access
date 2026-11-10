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
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { injectWideLayout } from '@app/layout/shell/wide-layout';
import { Stepper } from '@app/shared/components/stepper/stepper';
import { UseCaseOption } from '../../models/filter-options';
import { stepError } from '../../state/selection-rules';

/** „Keiner davon passt“: its own answer of step 1, not a use case (R-CON-02). */
interface NoneEntry {
  kind: 'none';
}

/** What the combobox holds: typed text, or the chosen entry. */
type UseCaseEntry = UseCaseOption | NoneEntry;

const NONE: NoneEntry = { kind: 'none' };

/** Handoff §3.4: counters are announced at most once per second. */
const MATCHES_THROTTLE_MS = 1000;

/** The listbox keeps room for about three options, even when the keyboard leaves less (Handoff §2.2). */
const MIN_PANEL_HEIGHT_PX = 144;

function isNone(entry: UseCaseEntry): entry is NoneEntry {
  return 'kind' in entry && entry.kind === 'none';
}

function normalize(text: string): string {
  return text.trim().toLocaleLowerCase('de');
}

/**
 * S10 Kriterien zusammenstellen (R-CON-01, R-CON-02): H1, intro and the linear four-step
 * `pa-stepper`. Step 1 is the single-choice use-case combobox: it filters while typing, keeps
 * „Keiner davon passt“ as the last option also while filtering, announces the number of matches
 * (throttled) and fits its listbox above the on-screen keyboard. „Weiter“ without an answer shows
 * the field error of S10-E and moves the focus into the field.
 *
 * Steps 2–4 show their question and hint only; their checkbox groups come with S4-11, the reset
 * notice, „Bisherige Auswahl“, the submit, the page title and the error title prefix with S4-12.
 * The selection lives in `withConfigurator()` (R-CON-09).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2134-3174
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2019-407
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2139-3332 (error)
 * @spec docs/SPEC_v2.md R-CON-01, R-CON-02; docs/design_handoff_procureaccess_v1.md §2.2, §5.1, §5.4
 * @invariants CFG-03
 */
@Component({
  selector: 'pa-criteria-select',
  imports: [
    CdkStepperModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
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
  private readonly window = inject(DOCUMENT).defaultView;

  private readonly stepper = viewChild.required(Stepper);
  private readonly useCaseInput = viewChild<ElementRef<HTMLInputElement>>('useCaseInput');
  private readonly useCaseTrigger = viewChild(MatAutocompleteTrigger);

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
    if (choice.kind === 'useCase') return this.useCases().find((u) => u.id === choice.id) ?? null;
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

  protected nextFromUseCase(): void {
    if (!this.useCaseAnswered()) {
      this.useCaseChecked.set(true);
      this.useCaseField.markAsTouched();
      this.useCaseField.updateValueAndValidity();
      this.useCaseInput()?.nativeElement.focus();
      // The error is read with the field; the list would cover it (S10-E shows it closed).
      this.useCaseTrigger()?.closePanel();
      return;
    }
    this.stepper().next();
  }

  protected next(): void {
    this.stepper().next();
  }

  protected previous(): void {
    this.stepper().previous();
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
