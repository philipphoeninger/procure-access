// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import accessibility from '../../../../content/legal/barrierefreiheit.de.md';
import privacy from '../../../../content/legal/datenschutz.de.md';
import imprint from '../../../../content/legal/impressum.de.md';
import { parseLegalText } from './legal-markdown';

describe('parseLegalText', () => {
  it('splits the text at headings of level 2 to 4 and keeps the text between them as it is', () => {
    const source = [
      'Einleitung mit **Betonung**.',
      '',
      '## Verantwortlich',
      'Musterstraße 1',
      '- erster Punkt',
      '  - darunter',
      '### Kontakt',
      '#### Telefon',
      'Text',
    ].join('\n');

    expect(parseLegalText(source)).toEqual({
      placeholder: false,
      blocks: [
        { kind: 'text', text: 'Einleitung mit **Betonung**.\n' },
        { kind: 'heading', level: 2, text: 'Verantwortlich' },
        { kind: 'text', text: 'Musterstraße 1\n- erster Punkt\n  - darunter' },
        { kind: 'heading', level: 3, text: 'Kontakt' },
        { kind: 'heading', level: 4, text: 'Telefon' },
        { kind: 'text', text: 'Text' },
      ],
    });
  });

  it('shows a level-1 heading, a level-5 heading and a mark without a space as written', () => {
    const { blocks } = parseLegalText('# Titel\n##### Fünf\n##Ohne Leerzeichen');

    expect(blocks).toEqual([{ kind: 'text', text: '# Titel\n##### Fünf\n##Ohne Leerzeichen' }]);
  });

  it('drops comments, also over several lines, and reads Windows line ends', () => {
    const source = '<!--\nStand und Quelle\n-->\r\n## Titel <!-- intern -->\r\nText\r\n';

    expect(parseLegalText(source)).toEqual({
      placeholder: false,
      blocks: [
        { kind: 'heading', level: 2, text: 'Titel' },
        { kind: 'text', text: 'Text\n' },
      ],
    });
  });

  it('hides the rest of the file after a comment without its end, as HTML does', () => {
    expect(parseLegalText('## Titel\nText <!-- offen\n## Nicht gezeigt').blocks).toEqual([
      { kind: 'heading', level: 2, text: 'Titel' },
      { kind: 'text', text: 'Text ' },
    ]);
    expect(parseLegalText('<!-- offen\n## Nicht gezeigt').placeholder).toBe(true);
  });

  it('is a placeholder while the marker is in the file, in any case and next to content', () => {
    expect(parseLegalText('<!-- PLATZHALTER -->\n')).toEqual({ placeholder: true, blocks: [] });
    expect(parseLegalText('<!--Platzhalter-->\n## Titel\nText').placeholder).toBe(true);
  });

  it('is a placeholder without content: empty, blank or only comments', () => {
    for (const source of ['', ' \n\n', '<!-- Inhalt folgt -->']) {
      expect(parseLegalText(source).placeholder).toBe(true);
    }
  });

  it.each([
    ['impressum.de.md', imprint],
    ['datenschutz.de.md', privacy],
    ['barrierefreiheit.de.md', accessibility],
  ])('reads the repository file %s without a level-1 heading (the H1 is the page title)', (_, source) => {
    const texts = parseLegalText(source).blocks.flatMap((block) =>
      block.kind === 'text' ? block.text.split('\n') : [],
    );

    expect(texts.filter((line) => /^# /.test(line))).toEqual([]);
  });
});
