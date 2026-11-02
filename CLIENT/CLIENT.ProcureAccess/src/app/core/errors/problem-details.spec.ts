// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { FormControl } from '@angular/forms';

import { applyServerErrors, problemTypeName } from './problem-details';

function problem(type: string | undefined, errors?: Record<string, string[]>) {
  return new HttpErrorResponse({ status: 400, error: { type, status: 400, errors } });
}

describe('problemTypeName', () => {
  it('reads the name after /problems/ under any configured base URI', () => {
    expect(problemTypeName(problem('https://procureaccess.de/problems/validation'))).toBe('validation');
    expect(problemTypeName(problem('https://staging.example.org/base/problems/email-unconfirmed'))).toBe(
      'email-unconfirmed',
    );
  });

  it('returns nothing for responses that are not Problem Details of the API', () => {
    expect(problemTypeName(problem('https://tools.ietf.org/html/rfc9110#section-15.5.1'))).toBeUndefined();
    expect(problemTypeName(problem(undefined))).toBeUndefined();
    expect(problemTypeName(new HttpErrorResponse({ status: 500, error: 'plain text' }))).toBeUndefined();
    expect(problemTypeName(new Error('not HTTP'))).toBeUndefined();
  });
});

describe('applyServerErrors', () => {
  const validation = 'https://procureaccess.de/problems/validation';

  it('sets the server error on a known field, marks it touched and links the summary line', () => {
    const control = new FormControl('beispiel');
    const items = applyServerErrors(problem(validation, { 'product.link': ['Not a URL.', 'Too short.'] }), (path) =>
      path === 'product.link' ? { control, id: 'link-input', labelKey: 'COMMON.LINK' } : undefined,
    );

    expect(control.getError('server')).toBe('Not a URL. Too short.');
    expect(control.touched).toBe(true);
    expect(items).toEqual([
      {
        message: { key: 'errors.field.server', params: { message: 'Not a URL. Too short.' } },
        field: { id: 'link-input', labelKey: 'COMMON.LINK' },
      },
    ]);
  });

  it('keeps the client errors of the control', () => {
    const control = new FormControl('');
    control.setErrors({ required: true });

    applyServerErrors(problem(validation, { name: ['Required.'] }), () => ({ control, id: 'n', labelKey: 'COMMON.NAME' }));

    expect(control.errors).toEqual({ required: true, server: 'Required.' });
  });

  it('gives paths without a field, e.g. $ for the whole body, a line without a link', () => {
    const items = applyServerErrors(problem(validation, { $: ['The body is invalid.'], empty: [] }), () => undefined);

    expect(items).toEqual([{ message: { key: 'errors.field.server', params: { message: 'The body is invalid.' } } }]);
  });

  it('returns no lines for other problem types', () => {
    expect(
      applyServerErrors(problem('https://procureaccess.de/problems/not-found', { x: ['y'] }), () => undefined),
    ).toEqual([]);
  });
});
