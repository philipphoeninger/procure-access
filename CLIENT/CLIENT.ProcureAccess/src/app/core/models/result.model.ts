// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class Result {
  @jsonMember
  succeeded: boolean;

  @jsonMember
  error?: string;

  constructor(
    pSucceeded: boolean,
    pError: string | undefined) {
    this.succeeded = pSucceeded;
    this.error = pError;
  }
}


@jsonObject
export class TResult<T> extends Result {
  @jsonMember
  value?: T;

  constructor(
    pSucceeded: boolean,
    pError: string | undefined,
    pValue: T) {
      super(pSucceeded, pError);
      this.value = pValue;
  }
}
