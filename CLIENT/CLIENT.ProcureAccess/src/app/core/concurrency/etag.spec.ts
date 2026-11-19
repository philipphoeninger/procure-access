// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpHeaders, HttpResponse } from '@angular/common/http';

import { ifMatch, withEtag } from './etag';

describe('etag (API-03)', () => {
  it('joins the body of an answer with its ETag header', () => {
    const response = new HttpResponse({
      body: { id: 'p-1' },
      headers: new HttpHeaders({ ETag: '"1-4"' }),
    });

    expect(withEtag(response)).toEqual({ id: 'p-1', etag: '"1-4"' });
  });

  it('refuses an answer without an ETag or without a body', () => {
    expect(() =>
      withEtag(new HttpResponse({ body: { id: 'p-1' }, url: '/api/v1/projects/p-1' })),
    ).toThrowError(/\/api\/v1\/projects\/p-1 has no body or no ETag/);
    expect(() =>
      withEtag(new HttpResponse<object>({ headers: new HttpHeaders({ ETag: '"1-4"' }) })),
    ).toThrowError(/no body/);
  });

  it('sends the tag back unchanged in If-Match', () => {
    expect(ifMatch('"1-4"').get('If-Match')).toBe('"1-4"');
  });
});
