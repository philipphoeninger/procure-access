// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { AbstractControl, FormControl, ValidationErrors, ValidatorFn } from '@angular/forms';

import { formatGermanDate, parseGermanDate, todayIsoDate } from '@app/shared/date/german-date';

import { EVIDENCE_LIMITS, TEST_PROCEDURES, TestEvidence, TestEvidenceRequest } from '../../../models/candidates';

/** „Art der Prüfung“: a catalog procedure, „Andere Prüfung“, „Eigene Prüfung ohne festes Verfahren“, or none chosen yet. */
export type KindChoice = '' | (typeof TEST_PROCEDURES)[number] | 'other' | 'own';

/** The fields of a card in the order of S34a (UX copy U56, Handoff §5.1). */
export const EVIDENCE_FIELDS = [
  'kind',
  'otherName',
  'testedBy',
  'testDate',
  'testedVersion',
  'evidenceUrl',
  'qualificationNote',
] as const;
export type EvidenceField = (typeof EVIDENCE_FIELDS)[number];

/** The label of each field (UX copy §5.8); all but the last three are required. */
export const EVIDENCE_LABELS: Readonly<Record<EvidenceField, string>> = {
  kind: 'candidate.evidence.testType',
  otherName: 'candidate.evidence.testType.otherName',
  testedBy: 'candidate.evidence.testedBy',
  testDate: 'candidate.evidence.testDate',
  testedVersion: 'candidate.evidence.version',
  evidenceUrl: 'candidate.evidence.url',
  qualificationNote: 'candidate.evidence.qualification',
};

/** The server's field path of each field (SPEC §8.6: `errors` keys as the client sends them). */
export const EVIDENCE_PATHS: Readonly<Record<string, EvidenceField>> = {
  testKind: 'kind',
  testTypeId: 'kind',
  otherTestTypeName: 'otherName',
  testedBy: 'testedBy',
  testDate: 'testDate',
  testedVersion: 'testedVersion',
  evidenceUrl: 'evidenceUrl',
  qualificationNote: 'qualificationNote',
};

function maxLength(max: number): ValidatorFn {
  return (control: AbstractControl<string>) => (control.value.trim().length > max ? { maxlength: true } : null);
}

function required(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

/** An address that starts with https:// (SPEC §7.2), as S33 checks it. */
function httpsAddress(control: AbstractControl<string>): ValidationErrors | null {
  const value = control.value.trim();
  if (value.length === 0) return null;
  try {
    return new URL(value).protocol === 'https:' && /^https:\/\//i.test(value) ? null : { url: true };
  } catch {
    return { url: true };
  }
}

/** „TT.MM.JJJJ“, a day that exists and not after today (the server checks Europe/Berlin). */
function testDate(control: AbstractControl<string>): ValidationErrors | null {
  if (control.value.trim().length === 0) return { required: true };
  const day = parseGermanDate(control.value);
  if (day === null) return { date: true };
  return day > todayIsoDate() ? { dateFuture: true } : null;
}

/**
 * One card of S34a: a saved evidence (`saved`) or a new one, with its form controls. `key` stays
 * the same for the card's life, so its ids and its place in the list do not change while it is
 * saved. The test status lives outside the cards; a card exists only while it is „Ja“.
 */
export class EvidenceCard {
  readonly kind = new FormControl<KindChoice>('', { nonNullable: true, validators: [required] });
  /** Required only with „Andere Prüfung“ (R-CAN-05); revalidated when the kind changes. */
  readonly otherName = new FormControl('', {
    nonNullable: true,
    validators: [
      (control: AbstractControl<string>) => (this.kind.value === 'other' ? required(control) : null),
      maxLength(EVIDENCE_LIMITS.otherTestTypeName),
    ],
  });
  readonly testedBy = new FormControl('', { nonNullable: true, validators: [required, maxLength(EVIDENCE_LIMITS.testedBy)] });
  readonly testDate = new FormControl('', { nonNullable: true, validators: [testDate] });
  readonly testedVersion = new FormControl('', { nonNullable: true, validators: [maxLength(EVIDENCE_LIMITS.testedVersion)] });
  readonly evidenceUrl = new FormControl('', { nonNullable: true, validators: [httpsAddress, maxLength(EVIDENCE_LIMITS.evidenceUrl)] });
  readonly qualificationNote = new FormControl('', { nonNullable: true, validators: [maxLength(EVIDENCE_LIMITS.qualificationNote)] });

  constructor(
    readonly key: string,
    /** The evidence as the server last answered it; null until a new card is saved. */
    public saved: TestEvidence | null = null,
  ) {
    if (saved !== null) this.fill(saved);
  }

  control(field: EvidenceField): FormControl<string> {
    return this[field];
  }

  /** A new card nobody has typed into: skipped on „Speichern“ (owner decision S6-17 D4 (b)). */
  isBlank(): boolean {
    return this.saved === null && EVIDENCE_FIELDS.every((field) => this.control(field).value.trim().length === 0);
  }

  /** Whether „Speichern“ has to send this card: a new card that is not blank, or a saved one whose fields changed. */
  needsSaving(): boolean {
    if (this.saved === null) return !this.isBlank();
    return JSON.stringify(this.request()) !== JSON.stringify(requestOf(this.saved));
  }

  /** The body of the card's request (S6-15): trimmed, the procedure or the given name only with their kind. */
  request(): TestEvidenceRequest {
    const kind = this.kind.value;
    const testKind = kind === 'other' || kind === 'own' ? kind : 'procedure';
    return {
      testKind,
      testTypeId: testKind === 'procedure' ? kind : null,
      otherTestTypeName: testKind === 'other' ? this.otherName.value.trim() : null,
      testedBy: this.testedBy.value.trim(),
      testDate: parseGermanDate(this.testDate.value) ?? '',
      testedVersion: this.testedVersion.value.trim(),
      evidenceUrl: this.evidenceUrl.value.trim(),
      qualificationNote: this.qualificationNote.value.trim(),
    };
  }

  private fill(evidence: TestEvidence): void {
    this.kind.setValue(evidence.testKind === 'procedure' ? ((evidence.testTypeId ?? '') as KindChoice) : evidence.testKind);
    this.otherName.setValue(evidence.otherTestTypeName ?? '');
    this.testedBy.setValue(evidence.testedBy);
    this.testDate.setValue(formatGermanDate(evidence.testDate));
    this.testedVersion.setValue(evidence.testedVersion ?? '');
    this.evidenceUrl.setValue(evidence.evidenceUrl ?? '');
    this.qualificationNote.setValue(evidence.qualificationNote ?? '');
  }
}

/** The body a saved evidence would send unchanged, to tell whether a card changed. */
function requestOf(evidence: TestEvidence): TestEvidenceRequest {
  return {
    testKind: evidence.testKind,
    testTypeId: evidence.testKind === 'procedure' ? evidence.testTypeId : null,
    otherTestTypeName: evidence.testKind === 'other' ? evidence.otherTestTypeName : null,
    testedBy: evidence.testedBy,
    testDate: evidence.testDate,
    testedVersion: evidence.testedVersion ?? '',
    evidenceUrl: evidence.evidenceUrl ?? '',
    qualificationNote: evidence.qualificationNote ?? '',
  };
}
