// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Figma link check (TESTING §8.2 stage 1; figma_code_mapping_procureaccess_v1.md §3–§4).
// Node, no dependencies: node scripts/check-figma-links.mjs (self-test: node --test scripts/check-figma-links.test.mjs)
// Verifies that every Figma-mapped component and page declares the agreed @figma nodes.
import { readdirSync, readFileSync, realpathSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
export const FILE_KEY = 'rPeSyK8NNe3d8AnUEXW9mt';
const BASE = 'CLIENT/CLIENT.ProcureAccess/'; // Angular app, relative to the repository root
const TAG_PREFIX = `https://www.figma.com/design/${FILE_KEY}/ProcureAccess?node-id=`;

// Components of mapping §2: every node is one @figma line of the file, and no other file declares the selector.
// pa-app-header and pa-app-header-compact are one Angular class, so header.ts carries two nodes.
export const COMPONENTS = {
  'src/app/shared/components/signals/fulfillment-breakdown/fulfillment-breakdown.ts': { nodes: ['2014-341'], selector: 'pa-fulfillment-breakdown' },
  'src/app/shared/components/signals/documentation-signal/documentation-signal.ts': { nodes: ['2013-233'], selector: 'pa-documentation-signal' },
  'src/app/shared/components/signals/source-label/source-label.ts': { nodes: ['2010-348'], selector: 'pa-source-label' },
  'src/app/shared/components/signals/tested-status/tested-status.ts': { nodes: ['2009-103'], selector: 'pa-tested-status' },
  'src/app/shared/components/priority-indicator/priority-indicator.ts': { nodes: ['2011-203'], selector: 'pa-priority-indicator' },
  'src/app/shared/components/assessment-input/assessment-input.ts': { nodes: ['2012-2430'], selector: 'pa-assessment-input' },
  'src/app/features/directory/components/entry-row/entry-row.ts': { nodes: ['2033-616'], selector: 'pa-entry-row' },
  'src/app/features/directory/components/entry-card/entry-card.ts': { nodes: ['2015-378'], selector: 'pa-entry-card' },
  'src/app/shared/components/notice/notice.ts': { nodes: ['2032-486'], selector: 'pa-notice' },
  'src/app/features/directory/components/product-result/product-result.ts': { nodes: ['2053-593'], selector: 'pa-product-result' },
  'src/app/features/directory/components/contribution-item/contribution-item.ts': { nodes: ['2056-593'], selector: 'pa-contribution-item' },
  'src/app/shared/components/rating-legend/rating-legend.ts': { nodes: ['2099-760'], selector: 'pa-rating-legend' },
  'src/app/layout/header/header.ts': { nodes: ['2017-650', '2039-2131'], selector: 'pa-header' },
};

// Screens of mapping §2.2: the first @figma line of the file is the base frame (360 px where one exists,
// else the first frame listed). Further lines are free. Pages are found by path only: their selectors
// are not part of the mapping, and legacy selectors such as pa-criteria-list would collide.
export const PAGES = {
  'src/app/features/configurator/pages/criteria-select/criteria-select.ts': '2134-3174', // S10
  'src/app/features/configurator/pages/criteria-list/criteria-list.ts': '2020-70', // S11
  'src/app/features/configurator/dialogs/export-dialog/export-dialog.ts': '2134-3825', // S13
  'src/app/features/workspace/pages/project-detail/project-detail.ts': '2148-4697', // S32
  'src/app/features/workspace/pages/candidate-detail/candidate-detail.ts': '2134-3184', // S34 Überblick
  'src/app/features/workspace/pages/publish/publish.ts': '2023-633', // S35
  'src/app/shared/components/conflict-dialog/conflict-dialog.ts': '2056-2519', // S37
  'src/app/features/directory/pages/product-search/product-search.ts': '2134-2905', // S40
  'src/app/features/directory/pages/product-detail/product-detail.ts': '2025-676', // S41
  'src/app/features/directory/pages/entry-detail/entry-detail.ts': '2132-3382', // S42
  'src/app/features/directory/pages/experience-report/experience-report.ts': '2136-4005', // S51
  'src/app/features/directory/pages/my-contributions/my-contributions.ts': '2056-2334', // S53
  'src/app/features/directory/pages/vendor-publication/vendor-publication.ts': '2145-3653', // S55
};

// Mapped paths where a file exists that is not the mapped component or page yet (TESTING V60), with the reason.
// They get a notice instead of a failure while they carry no tag of this Figma file; a wrong node still fails.
// Remove an entry when the mapped file replaces it (the check says so), or when the file is gone (the check fails).
// Empty since S2-15 (the shell header carries its tag).
export const PENDING = {};

/** Node ids of the @figma lines of a source in their order; null for a link to another file or a foreign URL. */
export function figmaTags(source) {
  return [...source.matchAll(/@figma\s+(\S+)/g)].map(([, url]) =>
    url.startsWith(TAG_PREFIX) ? (url.slice(TAG_PREFIX.length).match(/^[\w-]+/)?.[0] ?? null) : null,
  );
}

const declaresSelector = (source, selector) => new RegExp(`selector:\\s*['"\`]${selector}['"\`]`).test(source);

/**
 * files: Map of client path (relative to the Angular app) → source, for every .ts file the check may read;
 * base is put in front of every path in a message. Returns errors, notices and the counts of checked,
 * not yet built and pending files.
 */
export function check({ components, pages, pending, files, base = '' }) {
  const errors = [];
  const notices = [];
  let checked = 0;
  let missing = 0;

  const entries = [
    ...Object.entries(components).map(([rel, { nodes }]) => ({ rel, nodes, page: false })),
    ...Object.entries(pages).map(([rel, node]) => ({ rel, nodes: [node], page: true })),
  ];
  const mapped = new Set(entries.map((e) => e.rel));
  for (const rel of Object.keys(pending)) {
    if (!mapped.has(rel)) errors.push(`PENDING entry ${base}${rel} is neither in COMPONENTS nor in PAGES`);
  }

  for (const { rel, nodes, page } of entries) {
    const source = files.get(rel);
    if (source === undefined) {
      if (rel in pending) errors.push(`${base}${rel}: on the PENDING list, but the file does not exist; remove the entry`);
      else missing++; // not built yet — allowed during the migration
      continue;
    }
    const tags = figmaTags(source);
    const tagged = page ? tags[0] === nodes[0] : nodes.every((n) => tags.includes(n));
    const expected = page ? `first @figma line node ${nodes[0]}` : `node ${nodes.join(' and node ')}`;
    if (rel in pending) {
      if (tagged) notices.push(`${base}${rel} now carries its @figma tag; remove it from PENDING`);
      else if (tags.some((t) => t !== null)) errors.push(`${base}${rel}: on the PENDING list, but its @figma tag is wrong (expected ${expected})`);
      else notices.push(`${base}${rel} skipped: ${pending[rel]}`);
      continue;
    }
    checked++;
    if (!tagged) errors.push(`${base}${rel}: missing or wrong @figma tag (expected ${expected})`);
  }

  // A mapped component built at another path would otherwise count as "not built yet" for ever.
  for (const [rel, { selector }] of Object.entries(components)) {
    for (const [other, source] of files) {
      if (other !== rel && declaresSelector(source, selector)) {
        errors.push(`${base}${other}: declares ${selector}, which the mapping places at ${base}${rel}; move it there or update the mapping §2 and COMPONENTS`);
      }
    }
  }

  return { errors, notices, checked, missing, pending: Object.keys(pending).length };
}

/** Production .ts files under src/app; tests and test doubles may reuse a selector. */
function clientFiles(dir = 'src/app') {
  return readdirSync(join(ROOT, BASE, dir), { withFileTypes: true }).flatMap((e) => {
    const rel = `${dir}/${e.name}`;
    if (e.isDirectory()) return rel === 'src/app/testing' ? [] : clientFiles(rel);
    return e.name.endsWith('.ts') && !e.name.endsWith('.spec.ts') ? [rel] : [];
  });
}

function main() {
  const inActions = process.env.GITHUB_ACTIONS === 'true';
  const files = new Map(clientFiles().map((rel) => [rel, readFileSync(join(ROOT, BASE, rel), 'utf8')]));
  const result = check({ components: COMPONENTS, pages: PAGES, pending: PENDING, files, base: BASE });
  for (const n of result.notices) console.log(inActions ? `::notice title=Figma link check::${n}` : `- ${n}`);
  for (const e of result.errors) console.error(`✗ ${e}`);
  console.log(`Figma link check: ${result.checked} checked, ${result.missing} not built yet, ${result.pending} pending.`);
  process.exit(result.errors.length ? 1 : 0);
}

// Real paths on both sides: through a symlinked path the check would otherwise skip itself and pass.
if (process.argv[1] && realpathSync(process.argv[1]) === fileURLToPath(import.meta.url)) main();
