// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TextFieldModule } from '@angular/cdk/text-field';
import { HttpErrorResponse } from '@angular/common/http';
import { NgTemplateOutlet } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  ElementRef,
  inject,
  Injector,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors, ValidatorFn } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemTypeName, ServerErrorField } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ConfirmRequiredNotice } from '@app/features/identity/components/confirm-required-notice/confirm-required-notice';
import { ErrorPage, ErrorPageKind } from '@app/pages/error/error-page';
import { ConflictDialog } from '@app/shared/components/conflict-dialog/conflict-dialog';
import { changesText, EditConflict } from '@app/shared/components/conflict-dialog/edit-conflict';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { Notice } from '@app/shared/components/notice/notice';

import { CandidateTabs } from '../../components/candidate-tabs/candidate-tabs';
import { Candidate, CANDIDATE_LIMITS, CandidateRequest, candidatePath } from '../../models/candidates';
import { projectPath } from '../../models/projects';
import { CandidatesApiService } from '../../services/api/candidates-api.service';
import { CandidateLoad, isProjectEditor } from '../candidate-detail/candidate-load';

type FieldName = keyof CandidateRequest;

/** The fields of a stored candidate as the form shows them; an empty optional field is ''. */
function valuesOf(candidate: Candidate): CandidateRequest {
  return {
    name: candidate.name,
    vendor: candidate.vendor ?? '',
    website: candidate.website ?? '',
    functionalSummary: candidate.functionalSummary ?? '',
    contactNotes: candidate.privateSection.contactNotes ?? '',
    internalNotes: candidate.privateSection.internalNotes ?? '',
  };
}

/** One field of S33 in the order of the form; the private ones in the section „Nur für Ihre Organisation“. */
interface FieldSpec {
  name: FieldName;
  labelKey: string;
  required: boolean;
  multiline: boolean;
  url: boolean;
  private: boolean;
  autocomplete: string;
}

const FIELDS: readonly FieldSpec[] = [
  { name: 'name', labelKey: 'candidate.form.name', required: true, multiline: false, url: false, private: false, autocomplete: 'off' },
  { name: 'vendor', labelKey: 'candidate.form.vendor', required: false, multiline: false, url: false, private: false, autocomplete: 'organization' },
  { name: 'website', labelKey: 'candidate.form.website', required: false, multiline: false, url: true, private: false, autocomplete: 'url' },
  { name: 'functionalSummary', labelKey: 'candidate.form.summary', required: false, multiline: true, url: false, private: false, autocomplete: 'off' },
  { name: 'contactNotes', labelKey: 'candidate.form.contacts', required: false, multiline: true, url: false, private: true, autocomplete: 'off' },
  { name: 'internalNotes', labelKey: 'candidate.form.internalNotes', required: false, multiline: true, url: false, private: true, autocomplete: 'off' },
];

function required(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

function maxLength(max: number): ValidatorFn {
  return (control: AbstractControl<string>) => (control.value.trim().length > max ? { maxlength: true } : null);
}

/** An address that starts with https:// (SPEC §7.2), checked like the server's absolute `https` URI. */
function httpsAddress(control: AbstractControl<string>): ValidationErrors | null {
  const value = control.value.trim();
  if (value.length === 0) return null;
  try {
    return new URL(value).protocol === 'https:' && /^https:\/\//i.test(value) ? null : { url: true };
  } catch {
    return { url: true };
  }
}

/**
 * S33 „Produkt hinzufügen“ at `/projekte/:id/produkte/neu` and „Produkt bearbeiten“ at
 * `/projekte/:id/produkte/:pid/bearbeiten` (R-CAN-02; Handoff §5.3: no Figma frame — the title row
 * and tabs of S34a, the private section visually apart; UX copy §5.8).
 *
 * - H1 `candidate.form.title.new|edit`; when editing, the tabs of the candidate below it (none is
 *   current: this page is the form). `projects.archived.note` instead of the form for an archived
 *   project (R-PRJ-05); „Kein Zugriff“ in place for a Reader (the resolver's title).
 * - `common.requiredNote`, then Produktname (required, ≤ 200), Anbieter, Webseite des Anbieters
 *   (an https address), „Was kann das Produkt? (Stichworte)“ — all but the name with
 *   `common.optional` (UX copy U176). Then the section „Nur für Ihre Organisation“
 *   (`candidate.form.privateSection`, H2, on surface-container-low with a border) with
 *   `candidate.form.privateSection.text`, the contacts and internal notes (PRV-01, PRV-02).
 *   Field limits as the server's (SPEC §7.2), counted after trimming.
 * - Fields are checked on submit (Handoff §6.4): the error summary takes the focus and the title
 *   gets „Fehler:“. Server field errors show at their fields (API-02); antiforgery in the summary;
 *   an unconfirmed address shows `pa-confirm-required-notice` with the focus (SEC-10) and keeps the
 *   input. Anything else keeps the interceptor's system states (`project-archived` → its note).
 * - 409 `conflict` when editing (someone else changed the product since it was read; API-03,
 *   R-CAN-11) opens S37 with the changed fields as text (S6-21). „Neue Fassung laden“ reads the
 *   product again, fills the form with it and moves the focus to the H1; after Escape „Speichern“
 *   opens S37 again without a request until then.
 * - „Speichern“ with the button spinner, `aria-disabled` while sending; „Abbrechen“ leads back
 *   (to S32 for a new product, to S34 when editing). Success opens S34 Überblick of the product,
 *   whose route change titles, focuses and announces it. Sending every field: an empty optional
 *   field clears it (S6-15).
 * - The name-match hint `candidate.form.name.match` needs the directory's products (M6, SPEC
 *   v2.53-4) and is not shown yet.
 *
 * @spec docs/SPEC_v2.md R-CAN-02; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants API-02, SEC-10, PRV-02, A11Y-09
 */
@Component({
  selector: 'pa-candidate-form',
  imports: [
    CandidateTabs,
    ConfirmRequiredNotice,
    ErrorPage,
    ErrorSummary,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    NgTemplateOutlet,
    Notice,
    ReactiveFormsModule,
    RouterLink,
    TextFieldModule,
    TranslatePipe,
  ],
  templateUrl: './candidate-form.html',
  styleUrl: './candidate-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateFormPage {
  private readonly load = inject(CandidateLoad);
  private readonly api = inject(CandidatesApiService);
  private readonly store = inject(ProcureAccessStore);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly injector = inject(Injector);
  private readonly dialog = inject(MatDialog);
  private readonly conflict = new EditConflict();

  protected readonly publicFields = FIELDS.filter((field) => !field.private);
  protected readonly privateFields = FIELDS.filter((field) => field.private);

  protected readonly controls: Readonly<Record<FieldName, FormControl<string>>> = {
    name: new FormControl('', { nonNullable: true, validators: [required, maxLength(CANDIDATE_LIMITS.name)] }),
    vendor: new FormControl('', { nonNullable: true, validators: [maxLength(CANDIDATE_LIMITS.vendor)] }),
    website: new FormControl('', { nonNullable: true, validators: [httpsAddress, maxLength(CANDIDATE_LIMITS.website)] }),
    functionalSummary: new FormControl('', { nonNullable: true, validators: [maxLength(CANDIDATE_LIMITS.functionalSummary)] }),
    contactNotes: new FormControl('', { nonNullable: true, validators: [maxLength(CANDIDATE_LIMITS.contactNotes)] }),
    internalNotes: new FormControl('', { nonNullable: true, validators: [maxLength(CANDIDATE_LIMITS.internalNotes)] }),
  };

  protected readonly busy = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  /** True after the server refused the unconfirmed address (SEC-10). */
  protected readonly confirmationRequired = signal(false);
  private readonly submitted = signal(false);
  private readonly confirmNotice = viewChild<ElementRef<HTMLElement>>('confirmNotice');

  /** The project and (when editing) the candidate; null for „Seite nicht gefunden“ or the S80 server text. */
  protected readonly shown = computed(() => {
    const view = this.load.view();
    return view.kind === 'candidate' ? { project: view.project, candidate: view.candidate } : null;
  });

  /** What shows in place of the form: S80 for a failed read or a Reader, the archived note, or nothing. */
  protected readonly state = computed<'form' | 'archived' | 'notFound' | 'forbidden' | 'server'>(() => {
    const view = this.load.view();
    if (view.kind !== 'candidate') return view.kind;
    if (!isProjectEditor(this.store.organizations(), view.project)) return 'forbidden';
    return view.project.status === 'archived' ? 'archived' : 'form';
  });

  /** The S80 page in place of the form. */
  protected readonly errorKind = computed<ErrorPageKind>(() => {
    const state = this.state();
    return state === 'form' || state === 'archived' ? 'notFound' : state;
  });

  /** Field errors show after the first submit (Handoff §6.4), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  constructor() {
    // Fill the form with the candidate being edited, and name the page title's parameters.
    effect(() => {
      const shown = this.shown();
      if (shown === null) return;
      untracked(() => {
        if (shown.candidate !== null) this.fill(shown.candidate);
        this.pageTitle.setPageTitleState(this.titleParams());
      });
    });
  }

  protected fieldId(field: FieldSpec): string {
    return `pa-candidate-${field.name}`;
  }

  /** The message at a field, also its line of the error summary (E12). */
  protected message(field: FieldSpec): TextKey | null {
    const control = this.controls[field.name];
    if (control.hasError('server')) return { key: 'errors.field.server', params: { message: control.getError('server') } };
    if (control.hasError('required')) {
      return { key: 'errors.field.required', params: { field: this.translate.instant(field.labelKey) as string } };
    }
    if (control.hasError('url')) return { key: 'errors.field.url' };
    if (control.hasError('maxlength')) {
      return { key: 'errors.field.maxLength', params: { max: CANDIDATE_LIMITS[field.name], current: control.value.trim().length } };
    }
    return null;
  }

  /** Back to where the form was opened from: S34 when editing, S32 for a new product. */
  protected cancelPath(): string {
    const shown = this.shown();
    if (shown === null) return '/';
    return shown.candidate === null ? projectPath(shown.project.id) : candidatePath(shown.project.id, shown.candidate.id);
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const shown = this.shown();
    if (this.busy() || shown === null) return;
    this.submitted.set(true);
    const invalid = this.fieldLines();
    if (invalid.length > 0) {
      this.showErrors(invalid);
      return;
    }
    // After a conflict nothing is sent until the product is read again (S37)
    if (this.conflict.blocked && shown.candidate !== null) {
      await this.showConflict(shown.candidate);
      return;
    }
    this.busy.set(true);
    let saved: Candidate;
    try {
      const body = this.body();
      saved =
        shown.candidate === null
          ? await this.api.create(shown.project.id, body)
          : await this.api.change(shown.candidate.id, body, shown.candidate.etag);
    } catch (error: unknown) {
      this.busy.set(false);
      if (shown.candidate !== null && this.conflict.caught(error)) {
        this.showErrors([]);
        await this.showConflict(shown.candidate);
      } else {
        this.refused(error);
      }
      return;
    } finally {
      this.busy.set(false);
    }
    this.showErrors([]);
    await this.router.navigateByUrl(candidatePath(shown.project.id, saved.id));
  }

  private fill(candidate: Candidate): void {
    const values = valuesOf(candidate);
    for (const field of FIELDS) this.controls[field.name].setValue(values[field.name]);
  }

  /**
   * S37 with the fields that differ from the product as it was read (UX copy U189). „Neue Fassung
   * laden“ reads it again: the form is filled from the stored version and the focus goes to the H1.
   */
  private async showConflict(candidate: Candidate): Promise<void> {
    const read = valuesOf(candidate);
    const body = this.body();
    const fields = FIELDS.filter((field) => body[field.name] !== read[field.name]).map((field) => ({
      label: this.translate.instant(field.labelKey) as string,
      value: body[field.name],
    }));
    const reload = await ConflictDialog.show(this.dialog, {
      editorDisplayName: this.conflict.editorDisplayName,
      changes: changesText([{ fields }]),
    });
    if (reload) await this.reload(candidate.id);
  }

  private async reload(candidateId: string): Promise<void> {
    this.store.incrementLoadingCount();
    let candidate: Candidate;
    try {
      candidate = await this.api.get(candidateId);
    } catch (error: unknown) {
      const status = error instanceof HttpErrorResponse ? error.status : 0;
      this.load.view.set(status === 404 || status === 400 ? { kind: 'notFound' } : { kind: 'server' });
      this.pageTitle.focusPageHeading();
      return;
    } finally {
      this.store.decrementLoadingCount();
    }
    this.conflict.clear();
    this.submitted.set(false);
    // The effect fills the form with the stored version
    this.load.view.update((view) => (view.kind === 'candidate' ? { ...view, candidate } : view));
    this.showErrors([]);
    this.pageTitle.focusPageHeading();
  }

  /** Every field, trimmed; an empty optional one clears it on a change. */
  private body(): CandidateRequest {
    const value = (name: FieldName) => this.controls[name].value.trim();
    return {
      name: value('name'),
      vendor: value('vendor'),
      website: value('website'),
      functionalSummary: value('functionalSummary'),
      contactNotes: value('contactNotes'),
      internalNotes: value('internalNotes'),
    };
  }

  private refused(error: unknown): void {
    switch (problemTypeName(error)) {
      case 'email-unconfirmed':
        this.showErrors([]);
        this.confirmationRequired.set(true);
        afterNextRender(() => this.confirmNotice()?.nativeElement.focus(), { injector: this.injector });
        return;
      case 'antiforgery':
        this.showErrors([{ message: { key: 'errors.antiforgery' } }]);
        return;
      case 'validation':
        this.showErrors(applyServerErrors(error, (path) => this.serverField(path)));
        return;
      default:
        // Anything else keeps the system states of the error interceptor.
        this.showErrors([]);
    }
  }

  private serverField(path: string): ServerErrorField | undefined {
    const field = FIELDS.find((candidate) => candidate.name === path);
    return field && { control: this.controls[field.name], id: this.fieldId(field), labelKey: field.labelKey };
  }

  private showErrors(lines: ErrorSummaryItem[]): void {
    this.summary.set(lines);
    this.pageTitle.setPageTitleState({ ...this.titleParams(), error: lines.length > 0 });
  }

  private titleParams(): { params?: Record<string, unknown> } {
    const shown = this.shown();
    return shown === null ? {} : { params: { project: shown.project.name, product: shown.candidate?.name ?? '' } };
  }

  private fieldLines(): ErrorSummaryItem[] {
    return FIELDS.flatMap((field) => {
      const message = this.message(field);
      return message ? [{ message, field: { id: this.fieldId(field), labelKey: field.labelKey } }] : [];
    });
  }
}
