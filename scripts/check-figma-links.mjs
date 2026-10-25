// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Figma link check (TESTING §8.2 stage 1; figma_code_mapping_procureaccess_v1.md §4).
// Node, no dependencies: node scripts/check-figma-links.mjs
// Verifies that every Figma-mapped component declares the agreed @figma node.
import { existsSync, readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const FILE_KEY = 'rPeSyK8NNe3d8AnUEXW9mt';
const BASE = 'CLIENT/CLIENT.ProcureAccess/'; // Angular app, relative to the repository root
const MAP = {
  'src/app/shared/components/signals/fulfillment-breakdown/fulfillment-breakdown.ts': '2014-341',
  'src/app/shared/components/signals/documentation-signal/documentation-signal.ts': '2013-233',
  'src/app/shared/components/signals/source-label/source-label.ts': '2010-348',
  'src/app/shared/components/signals/tested-status/tested-status.ts': '2009-103',
  'src/app/shared/components/priority-indicator/priority-indicator.ts': '2011-203',
  'src/app/shared/components/assessment-input/assessment-input.ts': '2012-2430',
  'src/app/features/directory/components/entry-row/entry-row.ts': '2033-616',
  'src/app/features/directory/components/entry-card/entry-card.ts': '2015-378',
  'src/app/shared/components/notice/notice.ts': '2032-486',
  'src/app/features/directory/components/product-result/product-result.ts': '2053-593',
  'src/app/features/directory/components/contribution-item/contribution-item.ts': '2056-593',
  'src/app/shared/components/rating-legend/rating-legend.ts': '2099-760',
  'src/app/layout/header/header.ts': '2017-650',
};

// Mapped paths where a file exists that is not the mapped component yet (TESTING_v11 V60).
// They get a notice instead of a failure. Remove an entry when the mapped component replaces it.
const PENDING = {
  'src/app/layout/header/header.ts':
    'legacy header, not pa-app-header yet; the step-1 shell adds the @figma tag and removes this entry',
};

const inActions = process.env.GITHUB_ACTIONS === 'true';
let failed = false;
let checked = 0;
let missing = 0;

for (const rel of Object.keys(PENDING)) {
  if (!(rel in MAP)) {
    console.error(`✗ PENDING entry ${rel} is not in MAP`);
    failed = true;
  }
}

for (const [rel, nodeId] of Object.entries(MAP)) {
  const file = BASE + rel;
  if (!existsSync(join(ROOT, file))) {
    missing++;
    continue; // not built yet — allowed during migration
  }
  const src = readFileSync(join(ROOT, file), 'utf8');
  const expected = `@figma https://www.figma.com/design/${FILE_KEY}/ProcureAccess?node-id=${nodeId}`;
  // The node ID must end there: node 2014-341 must not match a tag for node 2014-3410.
  const tagged = src.split(expected).slice(1).some((rest) => !/^[\w-]/.test(rest));
  if (PENDING[rel]) {
    const message = tagged
      ? `${file} now carries its @figma tag; remove it from PENDING`
      : `${file} skipped: ${PENDING[rel]}`;
    console.log(inActions ? `::notice title=Figma link check::${message}` : `- ${message}`);
    continue;
  }
  checked++;
  if (!tagged) {
    console.error(`✗ ${file}: missing or wrong @figma tag (expected node ${nodeId})`);
    failed = true;
  }
}

const pending = Object.keys(PENDING).length;
console.log(`Figma link check: ${checked} checked, ${missing} not built yet, ${pending} pending.`);
process.exit(failed ? 1 : 0);
