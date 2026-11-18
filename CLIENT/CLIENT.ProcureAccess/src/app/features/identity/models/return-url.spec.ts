// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { safeReturnUrl } from './return-url';

const origin = 'https://procureaccess.de';

describe('safeReturnUrl (S6-03, open redirect)', () => {
  it.each(['/konto', '/einstellungen', '/kriterienlisten/0b6c1f9e-2d1a-4b8e-9b55-8a1d2c3e4f50?ansicht=liste#k-12', '/'])(
    'accepts the same-origin relative path %s',
    (path) => {
      expect(safeReturnUrl(path, origin)).toBe(path);
    },
  );

  it.each([
    ['an absolute URL to another site', 'https://evil.example/konto'],
    ['an absolute URL to this site', 'https://procureaccess.de/konto'],
    ['an absolute http URL', 'http://evil.example'],
    ['a protocol-relative URL', '//evil.example/konto'],
    ['a protocol-relative URL with three slashes', '///evil.example'],
    ['a backslash after the slash', '/\\evil.example'],
    ['two backslashes', '\\\\evil.example'],
    ['a backslash inside the path', '/konto\\..\\evil'],
    ['javascript:', 'javascript:alert(1)'],
    ['javascript: in mixed case', 'JaVaScRiPt:alert(1)'],
    ['javascript: behind spaces', '  javascript:alert(1)'],
    ['data:', 'data:text/html,<script>alert(1)</script>'],
    ['a tab between the slashes', '/\t/evil.example'],
    ['a newline between the slashes', '/\n/evil.example'],
    ['a space', '/ /evil.example'],
    ['a double slash later in the path', '/konto//evil.example'],
    ['the login outlet (sign-in loop)', '/(login:anmelden)'],
    ['the login outlet beside a page', '/konto(login:anmelden)'],
    ['a relative path without a slash', 'konto'],
    ['an empty string', ''],
  ])('refuses %s', (_name, value) => {
    expect(safeReturnUrl(value, origin)).toBeNull();
  });

  it.each([
    ['undefined', undefined],
    ['null', null],
    ['a number', 42],
    ['an object', { toString: () => '/konto' }],
  ])('refuses %s, which is not a string', (_name, value) => {
    expect(safeReturnUrl(value, origin)).toBeNull();
  });
});
