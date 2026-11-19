// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TextFieldModule } from '@angular/cdk/text-field';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  ElementRef,
  inject,
  Injector,
  input,
  model,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { TranslatePipe } from '@ngx-translate/core';

import { ASSESSMENT_VALUES, AssessmentValue } from '../rating-legend/rating-legend';

/** Radios from 600 px, the compact form with a select and the note behind a button below (Handoff §2.2, §4.6). */
export type AssessmentLayout = 'compact' | 'radios';

/** The values of `errors.field.maxLength` while the note is too long. */
interface NoteLengthError {
  max: number;
  current: number;
}

let nextId = 0;

/**
 * K6 pa-assessment-input · Einschätzung je Kriterium (Handoff §4.6, Figma `2012:2430`): how well a
 * product meets one criterion, and on what the user bases it.
 *
 * - A `<fieldset>` whose `<legend>` is the criterion title (title/medium).
 * - `radios` (from 600 px): the question `candidate.assessment.legend` under the title, linked to
 *   the radio group with `aria-describedby`; one radio per allowed level in scale order, then
 *   „Nicht eingeschätzt“; in a row that wraps. The note field is always shown.
 * - `compact` (below 600 px, S34b-M): a select „Einschätzung“ with the same options in the same
 *   order; the note field sits behind „Begründung hinzufügen“ / „Begründung bearbeiten“ (a
 *   disclosure with the title as description; opening moves the focus into the field). A note that
 *   is too long keeps the field open.
 * - Only `allowedValues` are offered (the union of the list's app types, served by the API, C31);
 *   „Nicht eingeschätzt“ always. No sliders, no stars, no colour per level.
 * - The note: an outlined text area with the long question `candidate.assessment.note` as a
 *   visible label above it (Handoff §2.2 „Forms“, as S34a), described by the title; its length
 *   error `errors.field.maxLength` (counted after trimming, as the server does) shows once the
 *   field was left.
 *
 * `value` and `note` are models: the page saves what they emit (autosave, R-CAN-06). The same
 * component in both layouts; the page sets `layout` with a `BreakpointObserver` (Figma mapping §2.1).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2012-2430
 * @spec docs/SPEC_v2.md R-CAN-06; docs/design_handoff_procureaccess_v1.md §4.6
 * @invariants CAT-15, A11Y-01
 */
@Component({
  selector: 'pa-assessment-input',
  imports: [MatButtonModule, MatFormFieldModule, MatInputModule, MatRadioModule, MatSelectModule, ReactiveFormsModule, TextFieldModule, TranslatePipe],
  templateUrl: './assessment-input.html',
  styleUrl: './assessment-input.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'pa-assessment-input' },
})
export class AssessmentInput {
  private readonly injector = inject(Injector);

  /** The criterion title, the name of the group. */
  readonly criterionTitle = input.required<string>();
  /** The levels the criterion allows, without „Nicht eingeschätzt“, which is always offered. */
  readonly allowedValues = input.required<readonly AssessmentValue[]>();
  readonly layout = input<AssessmentLayout>('radios');
  /** The longest note the server accepts, after trimming (SPEC §7.2). */
  readonly noteMaxLength = input(1000);
  readonly value = model<AssessmentValue>('unknown');
  readonly note = model('');

  protected readonly id = `pa-assessment-${nextId++}`;
  protected readonly titleId = `${this.id}-title`;
  protected readonly questionId = `${this.id}-question`;
  protected readonly noteId = `${this.id}-note`;
  protected readonly noteFieldId = `${this.id}-note-field`;

  /** The allowed levels in scale order, „Nicht eingeschätzt“ last. */
  protected readonly options = computed<AssessmentValue[]>(() => {
    const allowed = this.allowedValues();
    return [...ASSESSMENT_VALUES.filter((value) => value !== 'unknown' && allowed.includes(value)), 'unknown'];
  });

  protected readonly noteOpen = signal(false);
  protected readonly noteControl = new FormControl('', {
    nonNullable: true,
    validators: [(control: AbstractControl<string>) => this.noteLength(control.value)],
  });
  private readonly noteField = viewChild.required<string, ElementRef<HTMLTextAreaElement>>('noteField', { read: ElementRef });

  constructor() {
    effect(() => {
      const note = this.note();
      if (note !== this.noteControl.value) this.noteControl.setValue(note, { emitEvent: false });
    });
    this.noteControl.valueChanges.pipe(takeUntilDestroyed()).subscribe((note) => this.note.set(note));
  }

  protected optionKey(value: AssessmentValue): string {
    return `assessment.value.${value}`;
  }

  protected choose(value: AssessmentValue): void {
    this.value.set(value);
  }

  /** The values of `errors.field.maxLength` while the note is too long. */
  protected noteError(): NoteLengthError | null {
    return (this.noteControl.errors?.['noteLength'] as NoteLengthError | undefined) ?? null;
  }

  /** Opens or closes the note field (compact); opening moves the focus into it. A too-long note keeps it open. */
  protected toggleNote(): void {
    if (this.noteOpen() && this.noteError() !== null) {
      this.noteControl.markAsTouched();
      this.focusNote();
      return;
    }
    this.noteOpen.update((open) => !open);
    if (this.noteOpen()) this.focusNote();
  }

  private focusNote(): void {
    afterNextRender(() => this.noteField().nativeElement.focus(), { injector: this.injector });
  }

  private noteLength(note: string): ValidationErrors | null {
    const max = this.noteMaxLength();
    const current = note.trim().length;
    return current > max ? { noteLength: { max, current } satisfies NoteLengthError } : null;
  }
}
