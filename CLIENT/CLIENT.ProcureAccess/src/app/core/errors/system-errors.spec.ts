// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';

import { ConnectionStatus } from './system-errors';

describe('ConnectionStatus', () => {
  it('follows the browser going offline and online again', () => {
    const status = TestBed.inject(ConnectionStatus);
    expect(status.offline()).toBe(false);

    window.dispatchEvent(new Event('offline'));
    expect(status.offline()).toBe(true);

    window.dispatchEvent(new Event('online'));
    expect(status.offline()).toBe(false);
  });

  it('is offline after a request without answer until the server answers', () => {
    const status = TestBed.inject(ConnectionStatus);

    status.noAnswer();
    expect(status.offline()).toBe(true);

    status.answered();
    expect(status.offline()).toBe(false);
  });
});
