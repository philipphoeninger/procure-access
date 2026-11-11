// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TextFieldModule } from '@angular/cdk/text-field';
import { DOCUMENT, NgTemplateOutlet } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  Injector,
  input,
  OnInit,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslatePipe } from '@ngx-translate/core';

import { PriorityIndicator } from '@app/shared/components/priority-indicator/priority-indicator';
import { PriorityRationale } from '@app/shared/components/priority-indicator/priority-rationale';

import { Criterion, PriorityLevel } from '../../models/criteria';
import { CriterionDecision, NoteLengthError } from '../../models/criteria-list';
import { CatalogNames, ExternalLink, linkTo } from '../../state/catalog-names';
import { effectiveLevel, effectiveWeight, noteLengthError } from '../../state/criteria-list-rules';
import {
  cardAnchor,
  metaLine,
  procedureTestType,
  requirementTitle,
  ruleText,
  sourceTitle,
} from '../../state/criterion-card-rules';
import { detailLayout, tabLabelKey } from '../../state/criterion-detail-rules';
import { PRIORITY_WEIGHT_RANGE } from '../../state/priority-weight-range';
import { ProcedureSections } from '../procedure-sections/procedure-sections';

/** Stands for the link inside `criteria.item.testedBy.meta`; a private-use character no text holds. */
const LINK_MARK = '';

/** The note's length check as a form validator, so Material shows its error after the field was left. */
function noteLength(control: AbstractControl<string>): ValidationErrors | null {
  const error = noteLengthError(control.value);
  return error === null ? null : { noteLength: error };
}

/**
 * One criterion of the criteria list S11 (Handoff §5.5): compact by default, with a detail panel
 * that opens in place. The title is the H2, the sections of the panel are H3 (R-LST-01).
 *
 * - Requirement: title (plain language, else the catalog title, O-S4-32), source title of the
 *   first procedure text, meta line (product part · EN 301 549 clause, U100), K5.
 * - „Geprüft nach …“ (R-LST-07): `criteria.item.testedBy.*` with the test type's link, K5; the
 *   panel adds `criteria.item.testedBy.note` (owner decision O-S4-33 (a)).
 * - The panel starts with „Warum diese Priorität?“: rationale, matched rule and source (R-LST-02,
 *   CAT-02); K5 itself has no toggle here (`showRationaleToggle=false`, U28). The catalog sections
 *   of the procedure texts follow (`pa-procedure-sections`, S4-15b), then „Gilt für“ (`appliesTo`,
 *   O-S4-29) and the source line of the texts. With several procedures the sections whose text is
 *   the same in all stand above a `mat-tab-group` with one tab per procedure (U101); each tab holds
 *   the other sections, „So wird es geprüft“ and its source line.
 * - Title row: a checkbox `criteria.item.include` (named „In Liste aufnehmen“ + the title, no visible
 *   text of its own, Figma S11) and, for an excluded criterion, `criteria.item.excluded`. An excluded
 *   card stays in the list on `surface-container-low` and keeps every control (R-LST-04).
 * - K5 with the level menu, the weight field of B (range 🔧 `PRIORITY_WEIGHT_RANGE`) and
 *   „Empfehlung wiederherstellen“ (R-LST-03).
 * - „Notiz hinzufügen“ / „Notiz bearbeiten“ opens the note field `criteria.item.note` and moves the
 *   focus into it (O-S4-42 (a)); the field starts open when a note exists. Its length error
 *   (`errors.field.maxLength`, SPEC §7.1, §7.2) shows once the field was left and then follows the
 *   text until it fits (O-S4-43 (a)); the text is kept as typed, without `maxlength` (R-LST-05).
 *
 * The card holds no list state: every change is an output, which the page hands to the in-memory
 * draft of `withConfigurator()`.
 *
 * The toggle keeps the focus and names the criterion through `aria-describedby`; the panel stays in
 * the DOM while hidden, so `aria-controls` always points at it. The title carries the anchor
 * `k-<key>` (the host keeps its own id, which the test library uses to remove a rendered card).
 *
 * @spec docs/SPEC_v2.md R-LST-01 – R-LST-05, R-LST-07; docs/design_handoff_procureaccess_v1.md §5.5
 * @invariants CAT-02, CAT-08
 */
@Component({
  selector: 'pa-criterion-card',
  imports: [
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatTabsModule,
    NgTemplateOutlet,
    PriorityIndicator,
    PriorityRationale,
    ProcedureSections,
    ReactiveFormsModule,
    TextFieldModule,
    TranslatePipe,
  ],
  templateUrl: './criterion-card.html',
  styleUrl: './criterion-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'pa-criterion-card',
    '[class.pa-criterion-card--excluded]': '!decision().included',
  },
})
export class CriterionCard implements OnInit {
  private readonly injector = inject(Injector);

  readonly criterion = input.required<Criterion>();
  readonly decision = input.required<CriterionDecision>();
  readonly names = input.required<CatalogNames>();

  /** „In Liste aufnehmen“ checked or not (R-LST-04). */
  readonly includedChange = output<boolean>();
  /** A level chosen in the K5 menu (R-LST-03). */
  readonly levelChange = output<PriorityLevel>();
  /** A weight chosen for B (R-LST-03). */
  readonly weightChange = output<number>();
  /** „Empfehlung wiederherstellen“ (R-LST-03). */
  readonly priorityReset = output<void>();
  /** The note as typed (R-LST-05). */
  readonly noteChange = output<string>();

  protected readonly linkMark = LINK_MARK;
  protected readonly expanded = signal(false);
  protected readonly weightRange = inject(PRIORITY_WEIGHT_RANGE);

  protected readonly noteOpen = signal(false);
  protected readonly noteControl = new FormControl('', { nonNullable: true, validators: [noteLength] });
  private readonly noteField = viewChild.required<string, ElementRef<HTMLTextAreaElement>>('noteField', {
    read: ElementRef,
  });

  /** The anchor `k-<key>` is the id of the title, so a link scrolls to the heading. */
  protected readonly titleId = computed(() => cardAnchor(this.criterion().key));
  protected readonly panelId = computed(() => `${this.titleId()}-details`);
  protected readonly includeLabelId = computed(() => `${this.titleId()}-include`);
  protected readonly noteId = computed(() => `${this.titleId()}-note`);

  /** The test type of a „Geprüft nach …“ card; null for a requirement. */
  protected readonly testType = computed(() => procedureTestType(this.criterion()));
  protected readonly testTypeKey = computed(() => `testType.${this.testType()}`);
  protected readonly testTypeLink = computed<ExternalLink | null>(() => {
    const testType = this.testType();
    const url = testType === null ? undefined : this.names().testTypeUrls[testType];
    return url === undefined ? null : linkTo(url);
  });

  protected readonly title = computed(() => requirementTitle(this.criterion()));
  protected readonly sourceTitle = computed(() => sourceTitle(this.criterion()));
  protected readonly meta = computed(() => metaLine(this.criterion(), this.names()));

  protected readonly level = computed(() => effectiveLevel(this.criterion(), this.decision()));
  protected readonly recommendedLevel = computed(() => this.criterion().resolvedPriority.level);
  protected readonly weight = computed(() => effectiveWeight(this.criterion(), this.decision()));
  protected readonly recommendedWeight = computed(() => {
    const recommended = this.criterion().resolvedPriority;
    return recommended.level === 'B' ? recommended.weight : null;
  });
  protected readonly rule = computed(() =>
    ruleText(this.criterion().resolvedPriority.matchedRule, this.names()),
  );

  protected readonly layout = computed(() => detailLayout(this.criterion()));
  protected readonly tabLabelKey = tabLabelKey;
  private readonly appTypeList = new Intl.ListFormat(
    inject(DOCUMENT).documentElement.lang || 'de',
    {
      type: 'conjunction',
    },
  );
  /** „Gilt für“: the selected app types the criterion applies to, joined in the document language. */
  protected readonly appliesTo = computed(() => {
    const names = this.criterion().appliesTo.map((appType) => appType.name);
    return names.length === 0 ? null : this.appTypeList.format(names);
  });

  constructor() {
    this.noteControl.valueChanges
      .pipe(takeUntilDestroyed(inject(DestroyRef)))
      .subscribe((note) => this.noteChange.emit(note));
  }

  ngOnInit(): void {
    const note = this.decision().note;
    this.noteControl.setValue(note, { emitEvent: false });
    this.noteOpen.set(note !== '');
  }

  protected toggle(): void {
    this.expanded.update((expanded) => !expanded);
  }

  /** K5 sets a weight only from its field, which lists numbers. */
  protected emitWeight(weight: number | null): void {
    if (weight !== null) {
      this.weightChange.emit(weight);
    }
  }

  /** Opens or closes the note field; opening moves the focus into it. */
  protected toggleNote(): void {
    this.noteOpen.update((open) => !open);
    if (this.noteOpen()) {
      afterNextRender(() => this.noteField().nativeElement.focus(), { injector: this.injector });
    }
  }

  /** The values of `errors.field.maxLength`, while the note is too long. */
  protected noteError(): NoteLengthError | null {
    return (this.noteControl.errors?.['noteLength'] as NoteLengthError | undefined) ?? null;
  }

  /** The text of `criteria.item.testedBy.meta` before the link. */
  protected beforeLink(text: string): string {
    return text.split(LINK_MARK)[0] ?? text;
  }

  /** The text of `criteria.item.testedBy.meta` after the link. */
  protected afterLink(text: string): string {
    return text.split(LINK_MARK)[1] ?? '';
  }
}
