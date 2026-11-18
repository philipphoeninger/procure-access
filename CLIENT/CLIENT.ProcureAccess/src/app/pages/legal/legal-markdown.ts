// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * The Markdown of a legal page (`content/legal/*.de.md`, S6-28, R-SHL-07, owner decision O-S6-6):
 * the catalog-text subset of `catalog-markdown.ts` (paragraphs, lists, `**bold**`, http(s)
 * links), plus headings and comments. The result is plain data, rendered as Angular nodes, never
 * as HTML.
 *
 * - `## `, `### ` and `#### ` lines are headings of level 2 to 4; level 1 is the page's H1, a UX
 *   copy text, so a `# ` line is shown as written.
 * - `<!-- … -->` is a comment, also over several lines, and is never shown; a comment without
 *   its end hides the rest of the file, as in HTML.
 * - The text between two headings goes to `pa-catalog-text` as it is.
 *
 * The placeholder marker is the comment `<!-- PLATZHALTER -->`: while a file carries it, or has no
 * content, the page shows `legal.placeholder` and the release check fails
 * (`scripts/check-release-config.mjs` uses the same marker and comment rule; keep them in step).
 */
export type LegalBlock =
  | { kind: 'heading'; level: 2 | 3 | 4; text: string }
  | { kind: 'text'; text: string };

export interface LegalText {
  /** True while the file carries the placeholder marker or has no content. */
  placeholder: boolean;
  blocks: LegalBlock[];
}

export const LEGAL_PLACEHOLDER = /<!--\s*PLATZHALTER\s*-->/i;
const HEADING = /^(#{2,4}) +(\S.*)$/;

export function parseLegalText(source: string): LegalText {
  const blocks: LegalBlock[] = [];
  let lines: string[] = [];
  const endText = () => {
    const text = lines.join('\n');
    if (text.trim() !== '') {
      blocks.push({ kind: 'text', text });
    }
    lines = [];
  };

  for (const line of withoutComments(source).replace(/\r\n?/g, '\n').split('\n')) {
    const heading = HEADING.exec(line.trimEnd());
    if (heading) {
      endText();
      blocks.push({
        kind: 'heading',
        level: heading[1].length as 2 | 3 | 4,
        text: heading[2],
      });
    } else {
      lines.push(line);
    }
  }
  endText();

  return { placeholder: LEGAL_PLACEHOLDER.test(source) || blocks.length === 0, blocks };
}

/**
 * The source without its comments. A scan instead of a regular-expression replacement, so no
 * `<!--` can survive by nesting (the result is never HTML, but nothing is left to look like a
 * comment either).
 */
export function withoutComments(source: string): string {
  let result = '';
  let from = 0;
  for (;;) {
    const start = source.indexOf('<!--', from);
    if (start === -1) {
      return result + source.slice(from);
    }
    result += source.slice(from, start);
    const end = source.indexOf('-->', start + 4);
    if (end === -1) {
      return result;
    }
    from = end + 3;
  }
}
