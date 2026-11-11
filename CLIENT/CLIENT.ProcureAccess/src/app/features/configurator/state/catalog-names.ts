// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { FilterOptionsLayers } from '../models/filter-options';

/**
 * Names of the catalog values by key, from the filter options of the draft. The criteria answer
 * names values only by key (SPEC §8.2); the card shows names.
 */
export interface CatalogNames {
  useCases: Readonly<Record<string, string>>;
  appTypes: Readonly<Record<string, string>>;
  productParts: Readonly<Record<string, string>>;
  /** URL of each test type, for the link of a „Geprüft nach …“ card. */
  testTypeUrls: Readonly<Record<string, string>>;
}

export const NO_CATALOG_NAMES: CatalogNames = { useCases: {}, appTypes: {}, productParts: {}, testTypeUrls: {} };

export function catalogNames(options: FilterOptionsLayers | null): CatalogNames {
  if (options === null) {
    return NO_CATALOG_NAMES;
  }
  const byId = (values: { id: string; name: string }[]) => Object.fromEntries(values.map((v) => [v.id, v.name]));
  return {
    useCases: byId(options.useCases),
    appTypes: byId(options.appTypes),
    productParts: byId(options.productParts),
    testTypeUrls: Object.fromEntries(options.testTypes.map((t) => [t.id, t.url])),
  };
}

/** A link to an external page with its domain as the visible text. */
export interface ExternalLink {
  url: string;
  domain: string;
}

/** The domain of a catalog URL, without `www.`; null for a URL that cannot be read. */
export function linkTo(url: string): ExternalLink | null {
  try {
    return { url, domain: new URL(url).hostname.replace(/^www\./, '') };
  } catch {
    return null;
  }
}
