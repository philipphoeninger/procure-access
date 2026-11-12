// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';
import { deflateRawSync } from 'node:zlib';
import { readXlsxSheet, unzip } from './xlsx';

/**
 * Self-test of the XLSX reader on small archives built here (no stack, no browser). J1 reads the
 * real ClosedXML file with it; these cases show that a cell the reader got wrong would fail.
 */

/** A ZIP archive with one entry per file, deflated or stored (the reader does not check the CRC). */
function zip(files: Record<string, string>, stored: readonly string[] = []): Buffer {
  const locals: Buffer[] = [];
  const centrals: Buffer[] = [];
  let offset = 0;
  for (const [name, content] of Object.entries(files)) {
    const raw = Buffer.from(content, 'utf8');
    const method = stored.includes(name) ? 0 : 8;
    const data = method === 0 ? raw : deflateRawSync(raw);
    const nameBytes = Buffer.from(name, 'utf8');
    // A local extra field the central directory does not have, as some writers do.
    const localExtra = Buffer.from([0xfe, 0xca, 0, 0]);

    const local = Buffer.alloc(30);
    local.writeUInt32LE(0x04034b50, 0);
    local.writeUInt16LE(method, 8);
    local.writeUInt32LE(data.length, 18);
    local.writeUInt32LE(raw.length, 22);
    local.writeUInt16LE(nameBytes.length, 26);
    local.writeUInt16LE(localExtra.length, 28);

    const central = Buffer.alloc(46);
    central.writeUInt32LE(0x02014b50, 0);
    central.writeUInt16LE(method, 10);
    central.writeUInt32LE(data.length, 20);
    central.writeUInt32LE(raw.length, 24);
    central.writeUInt16LE(nameBytes.length, 28);
    central.writeUInt32LE(offset, 42);

    locals.push(local, nameBytes, localExtra, data);
    centrals.push(central, nameBytes);
    offset += local.length + nameBytes.length + localExtra.length + data.length;
  }
  const directory = Buffer.concat(centrals);
  const end = Buffer.alloc(22);
  end.writeUInt32LE(0x06054b50, 0);
  end.writeUInt16LE(Object.keys(files).length, 8);
  end.writeUInt16LE(Object.keys(files).length, 10);
  end.writeUInt32LE(directory.length, 12);
  end.writeUInt32LE(offset, 16);
  return Buffer.concat([...locals, directory, end]);
}

const workbook = `<?xml version="1.0" encoding="UTF-8"?>
<x:workbook xmlns:x="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <x:sheets><x:sheet name="Kriterien" sheetId="1" r:id="R1" /><x:sheet name="Info" sheetId="2" r:id="R2" /></x:sheets>
</x:workbook>`;

const relationships = `<?xml version="1.0" encoding="UTF-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="R1" Type="worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="R2" Type="worksheet" Target="/xl/worksheets/sheet2.xml"/>
</Relationships>`;

const sharedStrings = `<?xml version="1.0" encoding="UTF-8"?>
<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="3" uniqueCount="3">
  <si><t>Nr.</t></si>
  <si><t xml:space="preserve">Priorität &amp; „Wahl“ &lt;B&gt; </t></si>
  <si><r><rPr><b/></rPr><t>Fett</t></r><r><t xml:space="preserve"> und normal</t></r></si>
</sst>`;

const sheet1 = `<?xml version="1.0" encoding="UTF-8"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>
  <row r="1"><c r="A1" t="s"><v>0</v></c><c r="B1" t="s"><v>1</v></c></row>
  <row r="2"><c r="A2"><v>1</v></c><c r="C2" t="inlineStr"><is><t>inline &#x2013; &#8364;</t></is></c><c r="D2" t="str"><f>A2</f><v>Formel</v></c><c r="E2" s="1"/><c r="AB2" t="s"><v>2</v></c></row>
  <row r="3"/>
</sheetData></worksheet>`;

// With a namespace prefix on every element, as ClosedXML writes.
const sheet2 = `<x:worksheet xmlns:x="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><x:sheetData>
  <x:row r="1" spans="1:2"><x:c r="A1" t="b"><x:v>1</x:v></x:c><x:c r="B1" s="2" /><x:c r="C1" t="inlineStr"><x:is><x:t>präfix</x:t></x:is></x:c></x:row>
</x:sheetData></x:worksheet>`;

const xlsx = zip(
  {
    'xl/workbook.xml': workbook,
    'xl/_rels/workbook.xml.rels': relationships,
    'xl/sharedStrings.xml': sharedStrings,
    'xl/worksheets/sheet1.xml': sheet1,
    'xl/worksheets/sheet2.xml': sheet2,
  },
  ['xl/worksheets/sheet2.xml'],
);

test.describe('XLSX reader helper', () => {
  test('reads shared, rich-text, inline and formula strings, numbers and gaps in column order', () => {
    const rows = readXlsxSheet(xlsx, 'Kriterien');

    expect(rows).toHaveLength(3);
    expect(rows[0]).toEqual(['Nr.', 'Priorität & „Wahl“ <B> ']);
    expect(rows[1].slice(0, 5)).toEqual(['1', '', 'inline – €', 'Formel', '']);
    // AB is column 28: the gap up to it is filled with empty cells.
    expect(rows[1]).toHaveLength(28);
    expect(rows[1][27]).toBe('Fett und normal');
    expect(rows[2]).toEqual([]);
  });

  test('reads elements with a namespace prefix, an absolute relationship target and a stored entry', () => {
    expect(readXlsxSheet(xlsx, 'Info')).toEqual([['1', '', 'präfix']]);
  });

  test('fails on a sheet that does not exist and on a file that is not a ZIP archive', () => {
    expect(() => readXlsxSheet(xlsx, 'Fehlt')).toThrow('The XLSX file has no sheet „Fehlt“');
    expect(() => unzip(Buffer.from('Nr.;Kriterium\r\n'.repeat(4)))).toThrow('Not a ZIP archive');
  });
});
