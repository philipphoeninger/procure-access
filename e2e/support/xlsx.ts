// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inflateRawSync } from 'node:zlib';

/**
 * Reads one sheet of an XLSX file as rows of cell texts, for E2E checks of a downloaded export
 * (J1, owner decision O-S4-76 (a): Node's `zlib` only, no XLSX package). It reads what the API's
 * ClosedXML writes — shared, inline and formula strings, numbers, booleans — and nothing more: no
 * styles, no dates as dates, no ZIP64. A number comes back as written in the file (`"1"`), an empty
 * cell as `""`, and every row is as long as its last filled cell.
 */
export function readXlsxSheet(file: Buffer, sheetName: string): string[][] {
  const entries = unzip(file);
  const text = (path: string): string => {
    const entry = entries.get(path);
    if (!entry) {
      throw new Error(`The XLSX file has no ${path}`);
    }
    return entry.toString('utf8');
  };

  const sheetId = [...text('xl/workbook.xml').matchAll(element('sheet'))]
    .map((match) => attributes(match[1]))
    .find((sheet) => sheet['name'] === sheetName)?.['r:id'];
  if (!sheetId) {
    throw new Error(`The XLSX file has no sheet „${sheetName}“`);
  }
  const target = [...text('xl/_rels/workbook.xml.rels').matchAll(element('Relationship'))]
    .map((match) => attributes(match[1]))
    .find((relationship) => relationship['Id'] === sheetId)?.['Target'];
  if (!target) {
    throw new Error(`The XLSX file does not say where sheet „${sheetName}“ is`);
  }
  // A target is relative to xl/ or absolute from the package root.
  const sheetPath = target.startsWith('/') ? target.slice(1) : `xl/${target}`;

  const shared = entries.has('xl/sharedStrings.xml')
    ? [...text('xl/sharedStrings.xml').matchAll(element('si'))].map((match) => textRuns(match[2] ?? ''))
    : [];

  return [...text(sheetPath).matchAll(element('row'))].map((row) => {
    const cells: string[] = [];
    for (const cell of (row[2] ?? '').matchAll(element('c'))) {
      const cellAttributes = attributes(cell[1]);
      const reference = cellAttributes['r'];
      const column = reference ? columnIndex(reference) : cells.length;
      while (cells.length < column) {
        cells.push('');
      }
      cells[column] = cellText(cellAttributes['t'], cell[2] ?? '', shared);
    }
    return cells;
  });
}

function cellText(type: string | undefined, content: string, shared: readonly string[]): string {
  if (type === 'inlineStr') {
    return textRuns(content);
  }
  const value = element('v').exec(content)?.[2];
  if (value === undefined) {
    return '';
  }
  if (type === 's') {
    const text = shared[Number(value)];
    if (text === undefined) {
      throw new Error(`Shared string ${value} does not exist`);
    }
    return text;
  }
  return decode(value);
}

/** The text of an `<si>` or `<is>`: every `<t>`, also inside rich-text runs, joined. */
function textRuns(content: string): string {
  return [...content.matchAll(element('t'))].map((match) => decode(match[2] ?? '')).join('');
}

/**
 * Every `name` element: group 1 holds its attributes, group 2 its content (undefined when it is
 * empty, `<name/>`). Writers may prefix the elements with a namespace (ClosedXML writes `<x:sheet>`).
 */
function element(name: string): RegExp {
  return new RegExp(`<(?:[\\w.-]+:)?${name}\\b([^>]*?)(?:\\/>|>([\\s\\S]*?)<\\/(?:[\\w.-]+:)?${name}>)`, 'g');
}

/** `A` → 0, `Z` → 25, `AA` → 26, from a reference such as `AB12`. */
function columnIndex(reference: string): number {
  const letters = /^[A-Z]+/.exec(reference)?.[0];
  if (!letters) {
    throw new Error(`Bad cell reference ${reference}`);
  }
  return [...letters].reduce((index, letter) => index * 26 + letter.charCodeAt(0) - 64, 0) - 1;
}

function attributes(source: string): Record<string, string> {
  const result: Record<string, string> = {};
  for (const match of source.matchAll(/([\w:]+)="([^"]*)"/g)) {
    result[match[1]] = decode(match[2]);
  }
  return result;
}

function decode(text: string): string {
  return text.replace(/&(#x[0-9a-fA-F]+|#\d+|amp|lt|gt|quot|apos);/g, (_, entity: string) => {
    switch (entity) {
      case 'amp':
        return '&';
      case 'lt':
        return '<';
      case 'gt':
        return '>';
      case 'quot':
        return '"';
      case 'apos':
        return "'";
      default:
        return String.fromCodePoint(entity[1] === 'x' ? parseInt(entity.slice(2), 16) : parseInt(entity.slice(1), 10));
    }
  });
}

const END_OF_CENTRAL_DIRECTORY = 0x06054b50;
const CENTRAL_DIRECTORY_ENTRY = 0x02014b50;
const LOCAL_FILE_HEADER = 0x04034b50;

/** The files of a ZIP archive by path; only "stored" and "deflated" entries, as XLSX uses. */
export function unzip(archive: Buffer): Map<string, Buffer> {
  // The end record is the last 22 bytes plus a comment of at most 64 kB.
  let end = -1;
  for (let offset = archive.length - 22; offset >= Math.max(0, archive.length - 22 - 0xffff); offset--) {
    if (archive.readUInt32LE(offset) === END_OF_CENTRAL_DIRECTORY) {
      end = offset;
      break;
    }
  }
  if (end < 0) {
    throw new Error('Not a ZIP archive: no end of central directory');
  }

  const files = new Map<string, Buffer>();
  const count = archive.readUInt16LE(end + 10);
  let entry = archive.readUInt32LE(end + 16);
  for (let i = 0; i < count; i++) {
    if (archive.readUInt32LE(entry) !== CENTRAL_DIRECTORY_ENTRY) {
      throw new Error(`Broken central directory at entry ${i}`);
    }
    const method = archive.readUInt16LE(entry + 10);
    const compressedSize = archive.readUInt32LE(entry + 20);
    const nameLength = archive.readUInt16LE(entry + 28);
    const extraLength = archive.readUInt16LE(entry + 30);
    const commentLength = archive.readUInt16LE(entry + 32);
    const localHeader = archive.readUInt32LE(entry + 42);
    const name = archive.toString('utf8', entry + 46, entry + 46 + nameLength);

    if (archive.readUInt32LE(localHeader) !== LOCAL_FILE_HEADER) {
      throw new Error(`Broken local header of ${name}`);
    }
    // The local header has its own name and extra lengths; the sizes come from the central directory.
    const dataStart = localHeader + 30 + archive.readUInt16LE(localHeader + 26) + archive.readUInt16LE(localHeader + 28);
    const data = archive.subarray(dataStart, dataStart + compressedSize);
    if (method === 0) {
      files.set(name, Buffer.from(data));
    } else if (method === 8) {
      files.set(name, inflateRawSync(data));
    } else {
      throw new Error(`${name} uses ZIP compression method ${method}`);
    }
    entry += 46 + nameLength + extraLength + commentLength;
  }
  return files;
}
