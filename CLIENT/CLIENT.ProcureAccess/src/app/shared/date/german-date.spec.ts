// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { formatGermanDate, parseGermanDate, todayIsoDate } from './german-date';

describe('German dates (TT.MM.JJJJ)', () => {
  it.each([
    ['14.03.2026', '2026-03-14'],
    ['4.3.2026', '2026-03-04'],
    [' 29.02.2024 ', '2024-02-29'],
  ])('reads %j as %s', (text, iso) => {
    expect(parseGermanDate(text)).toBe(iso);
  });

  it.each(['', '2026-03-14', '14.03.26', '31.02.2026', '29.02.2025', '00.01.2026', '14.13.2026', '14/03/2026', '14.03.2026 x', '01.01.0999'])(
    'refuses %j',
    (text) => {
      expect(parseGermanDate(text)).toBeNull();
    },
  );

  it('writes a day of the API as TT.MM.JJJJ and names today in the browser time zone', () => {
    expect(formatGermanDate('2026-03-14')).toBe('14.03.2026');
    expect(todayIsoDate(new Date(2026, 10, 9, 23, 30))).toBe('2026-11-09');
  });
});
