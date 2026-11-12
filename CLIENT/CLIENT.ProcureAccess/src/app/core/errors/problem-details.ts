// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpContextToken, HttpErrorResponse } from '@angular/common/http';
import { AbstractControl } from '@angular/forms';

import { ErrorSummaryItem } from './error-summary-item';

/** RFC 9457 Problem Details as the API sends them (SPEC §7.1, §8.6; ARCHITECTURE §7.1). */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  /** Field path as the client sent it (`product.link`, `items[0].note`) → messages; `$` = the body as a whole. */
  errors?: Record<string, string[]>;
  /** Of a `rate-limit` problem (429): whole minutes until the action works again (SPEC §7.3, `errors.rateLimit`). */
  retryAfterMinutes?: number;
}

/**
 * UX key per problem type for errors that are not shown at a form field (SPEC §8.6). Only types
 * whose key the documents name; the others keep the generic text until their step
 * (docs/sprints/sprint_02/S2-11_client_error_mapping.md, P1). `validation` is shown at the fields.
 */
export const PROBLEM_TYPE_KEYS: Readonly<Record<string, string>> = {
  'email-unconfirmed': 'auth.confirm.required',
};

/**
 * Set on a request whose form shows the field errors of a `validation` problem itself
 * (error summary and field errors, API-02); the error interceptor then shows no snackbar for it.
 */
export const FORM_SHOWS_FIELD_ERRORS = new HttpContextToken<boolean>(() => false);

/**
 * Problem types (SPEC §8.6) whose answer the caller shows itself, so the error interceptor shows
 * no snackbar for them, e.g. `not-found` of the filter options, which means „no active release“
 * (O-S4-3). 5xx answers and requests without an answer keep their system states.
 */
export const CALLER_HANDLES_PROBLEMS = new HttpContextToken<readonly string[]>(() => []);

/**
 * Set on a request the user did not ask for and never sees, such as the usage event of S11
 * (`CriteriaListShown`, SPEC §8.7): the error interceptor shows nothing for its failure — no
 * snackbar, no offline notice, no server page. An answer still counts as the API being reachable.
 */
export const BACKGROUND_REQUEST = new HttpContextToken<boolean>(() => false);

/**
 * Set on a request the user asked for whose failure the caller shows itself in place, whatever it
 * is — e.g. the export of S13, whose dialog shows every error with „Erneut versuchen“
 * (R-LST-11; owner decision O-S4-74 (a)): the error interceptor shows no snackbar and no server
 * page for it. A request without an answer still turns the offline notice on, because that
 * notice is the state of the connection, not of the request.
 */
export const CALLER_SHOWS_FAILURES = new HttpContextToken<boolean>(() => false);

/**
 * The name of the problem type: `type` is `<base>/problems/<name>` (SPEC §8.6). The base URI is
 * configurable on the server, so only the path counts.
 */
export function problemTypeName(error: unknown): string | undefined {
  const type = problemDetails(error)?.type;
  return typeof type === 'string' ? /\/problems\/([a-z-]+)$/.exec(type)?.[1] : undefined;
}

/** A form field that can show a server error: its control, the id of its input, its label key. */
export interface ServerErrorField {
  control: AbstractControl;
  id: string;
  labelKey: string;
}

/**
 * Binds the field errors of a `validation` problem to the form (API-02) and returns the lines of
 * the error summary. A path that `resolveField` knows sets the error `server` (the message) on its
 * control and marks it touched, so Material shows the `mat-error`, links it with `aria-describedby`
 * and sets `aria-invalid`; the error goes away when the user edits the field. Any other path
 * becomes a summary line without a link (Handoff §6.4). Not a validation problem → no lines.
 */
export function applyServerErrors(
  error: unknown,
  resolveField: (path: string) => ServerErrorField | undefined,
): ErrorSummaryItem[] {
  const errors = problemTypeName(error) === 'validation' ? problemDetails(error)?.errors : undefined;
  return Object.entries(errors ?? {})
    .filter(([, messages]) => messages.length > 0)
    .map(([path, messages]) => {
      const message = { key: 'errors.field.server', params: { message: messages.join(' ') } };
      const field = resolveField(path);
      if (!field) {
        return { message };
      }
      field.control.setErrors({ ...field.control.errors, server: message.params.message });
      field.control.markAsTouched();
      return { message, field: { id: field.id, labelKey: field.labelKey } };
    });
}

/** The field errors of a problem, keyed by field path as the client sent it (SPEC §8.6); empty without. */
export function problemFieldErrors(error: unknown): Readonly<Record<string, string[]>> {
  return problemDetails(error)?.errors ?? {};
}

/**
 * The Problem Details of a failed request; undefined for any other error body. A request for a
 * file answers its errors as a `Blob`: the error interceptor reads it first ({@link withProblemBody}).
 */
export function problemDetails(error: unknown): ProblemDetails | undefined {
  return error instanceof HttpErrorResponse &&
    typeof error.error === 'object' &&
    error.error !== null &&
    !(error.error instanceof Blob)
    ? (error.error as ProblemDetails)
    : undefined;
}

/**
 * The same error with its body read as JSON when a request for a file (`responseType: 'blob'`)
 * got a JSON answer such as Problem Details; any other error unchanged. A body that is not JSON
 * stays a `Blob`.
 */
export async function withProblemBody(error: HttpErrorResponse): Promise<HttpErrorResponse> {
  const body: unknown = error.error;
  if (!(body instanceof Blob) || !/json/i.test(body.type)) return error;
  try {
    const parsed: unknown = JSON.parse(await body.text());
    return new HttpErrorResponse({
      error: parsed,
      headers: error.headers,
      status: error.status,
      statusText: error.statusText,
      url: error.url ?? undefined,
    });
  } catch {
    return error;
  }
}
