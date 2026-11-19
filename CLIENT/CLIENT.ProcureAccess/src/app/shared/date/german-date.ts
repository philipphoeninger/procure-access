// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Calendar days as the UX copy writes them, „TT.MM.JJJJ“ (`common.date.hint`, `errors.field.date`),
// and as the API sends them, `yyyy-MM-dd`. A plain text field with these helpers stands for
// Material's datepicker until the UX copy has the texts of its calendar (owner decision S6-17 D6 (a)).

const GERMAN_DATE = /^(\d{1,2})\.(\d{1,2})\.(\d{4})$/;

const pad = (value: number, length = 2) => String(value).padStart(length, '0');

/** The day `TT.MM.JJJJ` (one-digit day and month allowed) as `yyyy-MM-dd`; null for anything else or a day that does not exist. */
export function parseGermanDate(text: string): string | null {
  const match = GERMAN_DATE.exec(text.trim());
  if (match === null) return null;
  const [day, month, year] = [Number(match[1]), Number(match[2]), Number(match[3])];
  // Date.UTC rolls 31.02. over into March: a day that does not exist comes back changed
  const date = new Date(Date.UTC(year, month - 1, day));
  if (year < 1000 || date.getUTCFullYear() !== year || date.getUTCMonth() !== month - 1 || date.getUTCDate() !== day) return null;
  return `${pad(year, 4)}-${pad(month)}-${pad(day)}`;
}

/** The day `yyyy-MM-dd` as `TT.MM.JJJJ`. */
export function formatGermanDate(isoDate: string): string {
  const [year, month, day] = isoDate.split('-');
  return `${day}.${month}.${year}`;
}

/** Today in the browser's time zone as `yyyy-MM-dd`; the server checks against Europe/Berlin. */
export function todayIsoDate(now: Date = new Date()): string {
  return `${pad(now.getFullYear(), 4)}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
}
