// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * The small Markdown subset of the catalog texts (owner decisions O-S4-30 (a), O-S4-36 (a)): the
 * release folders store each text as Markdown lines joined with `\n` (S3-15 P6). The result is
 * plain data that `pa-catalog-text` renders as Angular nodes — never as HTML, so a `<H2>` in a
 * text stays text.
 *
 * - A blank line ends a paragraph or a list.
 * - `- ` starts a list item; two spaces of indentation per level nest it under the item above.
 * - A line that starts with a space (and is not an item) continues the list item above it.
 * - Every other line starts a paragraph, or continues the open one (joined with a space).
 * - Inline: `**bold**` and `[text](url)`, a link only for an http(s) URL; anything else, a
 *   heading mark `#` included, is shown as written.
 */
export type CatalogInline =
  | { kind: 'text'; text: string }
  | { kind: 'strong'; text: string }
  | { kind: 'link'; text: string; url: string };

export interface CatalogListItem {
  content: CatalogInline[];
  children: CatalogListItem[];
}

export type CatalogBlock =
  { kind: 'paragraph'; content: CatalogInline[] } | { kind: 'list'; items: CatalogListItem[] };

const LIST_ITEM = /^( *)- (.*)$/;
const CONTINUATION = /^ +(\S.*)$/;
const INLINE = /\*\*(.+?)\*\*|\[([^\]]+)\]\(([^)\s]+)\)/g;
const SPACES_PER_LEVEL = 2;

interface ItemDraft {
  raw: string;
  children: ItemDraft[];
}

type BlockDraft = { kind: 'paragraph'; raw: string } | { kind: 'list'; items: ItemDraft[] };

export function parseCatalogText(text: string): CatalogBlock[] {
  const blocks: BlockDraft[] = [];
  let paragraph: { kind: 'paragraph'; raw: string } | null = null;
  // Items by depth: levels[0] are the items of the open list, levels[n] the children of the last item of levels[n - 1].
  let levels: ItemDraft[][] | null = null;
  let lastItem: ItemDraft | null = null;

  for (const line of text.split('\n')) {
    const item = LIST_ITEM.exec(line);
    const continuation = CONTINUATION.exec(line);
    if (line.trim() === '') {
      paragraph = null;
      levels = null;
      lastItem = null;
    } else if (item) {
      paragraph = null;
      if (levels === null) {
        levels = [[]];
        blocks.push({ kind: 'list', items: levels[0] });
      }
      // At most one level deeper than the item above; an indented first item is a top-level item.
      let depth = Math.min(Math.floor(item[1].length / SPACES_PER_LEVEL), levels.length);
      if (depth === levels.length) {
        const parent = levels[depth - 1].at(-1);
        if (parent === undefined) {
          depth -= 1;
        } else {
          levels.push(parent.children);
        }
      }
      levels.length = depth + 1;
      lastItem = { raw: item[2], children: [] };
      levels[depth].push(lastItem);
    } else if (continuation && lastItem !== null) {
      lastItem.raw = `${lastItem.raw} ${continuation[1]}`;
    } else {
      levels = null;
      lastItem = null;
      if (paragraph === null) {
        paragraph = { kind: 'paragraph', raw: line.trim() };
        blocks.push(paragraph);
      } else {
        paragraph.raw = `${paragraph.raw} ${line.trim()}`;
      }
    }
  }

  return blocks.map((block) =>
    block.kind === 'paragraph'
      ? { kind: 'paragraph', content: parseInline(block.raw) }
      : { kind: 'list', items: block.items.map(toItem) },
  );
}

function toItem(draft: ItemDraft): CatalogListItem {
  return { content: parseInline(draft.raw), children: draft.children.map(toItem) };
}

/** `**bold**` and `[text](http(s) URL)`; the rest is text, a link to another scheme too. */
export function parseInline(raw: string): CatalogInline[] {
  const result: CatalogInline[] = [];
  const pushText = (text: string) => {
    if (text === '') {
      return;
    }
    const last = result.at(-1);
    if (last?.kind === 'text') {
      last.text += text;
    } else {
      result.push({ kind: 'text', text });
    }
  };

  let from = 0;
  for (const match of raw.matchAll(INLINE)) {
    pushText(raw.slice(from, match.index));
    const [whole, strong, linkText, url] = match;
    if (strong !== undefined) {
      result.push({ kind: 'strong', text: strong });
    } else if (isWebUrl(url)) {
      result.push({ kind: 'link', text: linkText, url });
    } else {
      pushText(whole);
    }
    from = match.index + whole.length;
  }
  pushText(raw.slice(from));
  return result;
}

/** True for an absolute http or https URL; a catalog text never links anywhere else. */
export function isWebUrl(url: string): boolean {
  try {
    const protocol = new URL(url).protocol;
    return protocol === 'https:' || protocol === 'http:';
  } catch {
    return false;
  }
}

/** Number of top-level list items of a text (the examples of one group, O-S4-37). */
export function countTopLevelItems(blocks: readonly CatalogBlock[]): number {
  return blocks.reduce((sum, block) => sum + (block.kind === 'list' ? block.items.length : 0), 0);
}

/**
 * The text with only its first `max` top-level list items (owner decision O-S4-37 (a)); nested
 * items stay with their parent, and paragraphs — the notes after a list — always stay. A list
 * left without items is dropped.
 */
export function limitTopLevelItems(blocks: readonly CatalogBlock[], max: number): CatalogBlock[] {
  let left = max;
  const result: CatalogBlock[] = [];
  for (const block of blocks) {
    if (block.kind === 'paragraph') {
      result.push(block);
    } else if (left > 0) {
      result.push({ kind: 'list', items: block.items.slice(0, left) });
      left -= Math.min(left, block.items.length);
    }
  }
  return result;
}
