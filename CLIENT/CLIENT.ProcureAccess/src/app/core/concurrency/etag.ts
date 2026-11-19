// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpHeaders, HttpResponse } from '@angular/common/http';

/**
 * A shared resource as it was read, with the `ETag` of its answer (ARCHITECTURE §7.1; API-03;
 * S6-20). A change sends the tag back in `If-Match` ({@link ifMatch}); the server refuses it with
 * 409 `conflict` when someone else has changed the resource since.
 */
export interface Versioned {
  /** The value of the `ETag` header, with its quotes; opaque to the client. */
  etag: string;
}

/**
 * The body of an answer together with its `ETag` header. Request with `observe: 'response'`.
 * Throws when the answer has no body or no `ETag`: the resource could never be changed then.
 */
export function withEtag<T extends object>(response: HttpResponse<T>): T & Versioned {
  const etag = response.headers.get('ETag');
  if (response.body === null || etag === null) {
    throw new Error(`The answer of ${response.url ?? 'a request'} has no body or no ETag.`);
  }
  return { ...response.body, etag };
}

/** The header of a change of a resource read at the version `etag` names. */
export function ifMatch(etag: string): HttpHeaders {
  return new HttpHeaders({ 'If-Match': etag });
}
