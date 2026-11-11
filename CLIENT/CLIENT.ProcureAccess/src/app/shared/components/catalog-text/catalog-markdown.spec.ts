// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { describe, expect, it } from 'vitest';

import {
  CatalogBlock,
  CatalogListItem,
  countTopLevelItems,
  isWebUrl,
  limitTopLevelItems,
  parseCatalogText,
  parseInline,
} from './catalog-markdown';

const text = (t: string) => ({ kind: 'text', text: t }) as const;
const item = (t: string, children: CatalogListItem[] = []): CatalogListItem => ({
  content: [text(t)],
  children,
});

describe('parseCatalogText', () => {
  it('splits paragraphs at blank lines and joins the lines of one paragraph with a space', () => {
    expect(parseCatalogText('Erster Satz.\nNoch dazu.\n\nZweiter Absatz.')).toEqual([
      { kind: 'paragraph', content: [text('Erster Satz. Noch dazu.')] },
      { kind: 'paragraph', content: [text('Zweiter Absatz.')] },
    ]);
  });

  it('reads a list between paragraphs (12.2.4)', () => {
    expect(
      parseCatalogText('Formate:\n\n- Webformat.\n- Nicht-Web-Format.\n\nAls Produktsupport …'),
    ).toEqual([
      { kind: 'paragraph', content: [text('Formate:')] },
      { kind: 'list', items: [item('Webformat.'), item('Nicht-Web-Format.')] },
      { kind: 'paragraph', content: [text('Als Produktsupport …')] },
    ]);
  });

  it('nests items by two spaces per level (10.1.3.2 pdfTest)', () => {
    const blocks = parseCatalogText(
      [
        '- Einschränkung: Kontoverbindung.',
        '  - visuell:',
        '    - IBAN DE12',
        '    - BIC BANKDEXXX',
        '  - Screenreader-Programm: DE12 IBAN',
        '- Barriere: Vielzahl.',
      ].join('\n'),
    );

    expect(blocks).toEqual([
      {
        kind: 'list',
        items: [
          item('Einschränkung: Kontoverbindung.', [
            item('visuell:', [item('IBAN DE12'), item('BIC BANKDEXXX')]),
            item('Screenreader-Programm: DE12 IBAN'),
          ]),
          item('Barriere: Vielzahl.'),
        ],
      },
    ]);
  });

  it('nests at most one level below the item above, and an indented first item is top-level', () => {
    expect(parseCatalogText('    - a\n      - b\n- c')).toEqual([
      { kind: 'list', items: [item('a', [item('b')]), item('c')] },
    ]);
  });

  it('continues a list item with a line that starts with a space (11.2.5.7 appTest)', () => {
    expect(
      parseCatalogText(
        '- Definition einer Ziehbewegung:\n Start und Ziel sind fest.\n- Pfad:\n Die Route zählt.',
      ),
    ).toEqual([
      {
        kind: 'list',
        items: [
          item('Definition einer Ziehbewegung: Start und Ziel sind fest.'),
          item('Pfad: Die Route zählt.'),
        ],
      },
    ]);
  });

  it('starts a paragraph after a list without a blank line and keeps the note after the examples', () => {
    expect(
      parseCatalogText('- erfüllt: ja\n\nWenn obige Anforderungen nicht voll erfüllt sind …'),
    ).toEqual([
      { kind: 'list', items: [item('erfüllt: ja')] },
      { kind: 'paragraph', content: [text('Wenn obige Anforderungen nicht voll erfüllt sind …')] },
    ]);
    expect(parseCatalogText('- a\nText')).toEqual([
      { kind: 'list', items: [item('a')] },
      { kind: 'paragraph', content: [text('Text')] },
    ]);
  });

  it('shows heading marks and HTML as written (11.1.3.2, 10.1.3.1f)', () => {
    expect(parseCatalogText('#todo#Bei einem Seitenaufruf …')).toEqual([
      { kind: 'paragraph', content: [text('#todo#Bei einem Seitenaufruf …')] },
    ]);
    expect(parseCatalogText('- bei: <H2> Dies ist eine </H2>')).toEqual([
      { kind: 'list', items: [item('bei: <H2> Dies ist eine </H2>')] },
    ]);
  });

  it('gives nothing for an empty text', () => {
    expect(parseCatalogText('')).toEqual([]);
    expect(parseCatalogText('\n\n')).toEqual([]);
  });
});

describe('parseInline', () => {
  it('reads bold (10.2.2.2)', () => {
    expect(parseInline('**1. Bewegen, Blinken, Scrollen**')).toEqual([
      { kind: 'strong', text: '1. Bewegen, Blinken, Scrollen' },
    ]);
    expect(parseInline('vor **fett** nach')).toEqual([
      text('vor '),
      { kind: 'strong', text: 'fett' },
      text(' nach'),
    ]);
  });

  it('reads an http(s) link (owner decision O-S4-36 (a), 12.2.4)', () => {
    expect(
      parseInline(
        'durchgeführt werden ([https://pac.pdf-accessibility.org/de](https://pac.pdf-accessibility.org/de)).',
      ),
    ).toEqual([
      text('durchgeführt werden ('),
      {
        kind: 'link',
        text: 'https://pac.pdf-accessibility.org/de',
        url: 'https://pac.pdf-accessibility.org/de',
      },
      text(').'),
    ]);
  });

  it('keeps a link to any other scheme as written text', () => {
    expect(parseInline('a [klick](javascript:alert(1)) b')).toEqual([
      text('a [klick](javascript:alert(1)) b'),
    ]);
    expect(parseInline('[Mail](mailto:x@example.invalid)')).toEqual([
      text('[Mail](mailto:x@example.invalid)'),
    ]);
    expect(parseInline('[relativ](/pfad)')).toEqual([text('[relativ](/pfad)')]);
  });

  it('keeps single stars and unclosed marks as text', () => {
    expect(parseInline('a * b ** c')).toEqual([text('a * b ** c')]);
  });
});

describe('isWebUrl', () => {
  it('accepts only absolute http and https URLs', () => {
    expect(isWebUrl('https://pdftest.bit-inklusiv.de/#nicht-text-inhalt')).toBe(true);
    expect(isWebUrl('http://example.org')).toBe(true);
    expect(isWebUrl('javascript:alert(1)')).toBe(false);
    expect(isWebUrl('data:text/html,x')).toBe(false);
    expect(isWebUrl('/relativ')).toBe(false);
  });
});

describe('limitTopLevelItems (owner decision O-S4-37 (a))', () => {
  const blocks: CatalogBlock[] = parseCatalogText(
    '- 1\n  - 1.1\n- 2\n\nNotiz\n\n- 3\n- 4\n- 5\n\nSchluss',
  );

  it('counts only top-level items', () => {
    expect(countTopLevelItems(blocks)).toBe(5);
  });

  it('keeps the first items with their children and every paragraph', () => {
    expect(limitTopLevelItems(blocks, 3)).toEqual([
      { kind: 'list', items: [item('1', [item('1.1')]), item('2')] },
      { kind: 'paragraph', content: [text('Notiz')] },
      { kind: 'list', items: [item('3')] },
      { kind: 'paragraph', content: [text('Schluss')] },
    ]);
  });

  it('drops a list left without items and keeps everything when the limit is not reached', () => {
    expect(limitTopLevelItems(blocks, 2)).toEqual([
      { kind: 'list', items: [item('1', [item('1.1')]), item('2')] },
      { kind: 'paragraph', content: [text('Notiz')] },
      { kind: 'paragraph', content: [text('Schluss')] },
    ]);
    expect(limitTopLevelItems(blocks, 5)).toEqual(blocks);
  });
});
