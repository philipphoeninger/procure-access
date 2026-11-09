# SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

"""Cut the Material Symbols Outlined font down to the icons the client uses (#135, S3-25).

Run it through scripts/subset-material-symbols.sh (Docker, pinned fontTools); it needs the
client's node_modules (npm install in CLIENT/CLIENT.ProcureAccess first).

Input:  the icon list material-symbols-icons.json and the full font of the material-symbols package.
Output: material-symbols-outlined.woff2 (the subset) and material-symbols-outlined.manifest.json
        (package version, icons, SHA-256 of the subset), both next to the icon list.
scripts/check-icons.mjs compares the list, the manifest and the font in CI stage 1.

Each icon is a ligature: the font replaces the letters of its name (`add_2`) with its glyph. A
plain subset keeps every ligature whose letters are kept, i.e. every icon, so the ligatures of the
icons not on the list are removed first. The variable axes (FILL, GRAD, opsz, wght) stay, so a
kept icon looks exactly as in the full font.
"""

import hashlib
import json
import sys
from pathlib import Path

from fontTools import subset
from fontTools.ttLib import TTFont

ROOT = Path(__file__).resolve().parent.parent
CLIENT = ROOT / "CLIENT" / "CLIENT.ProcureAccess"
PACKAGE = CLIENT / "node_modules" / "material-symbols"
FONTS = CLIENT / "src" / "assets" / "fonts"
ICON_LIST = FONTS / "material-symbols-icons.json"
SUBSET = FONTS / "material-symbols-outlined.woff2"
MANIFEST = FONTS / "material-symbols-outlined.manifest.json"


def fail(message):
    print(f"error: {message}", file=sys.stderr)
    sys.exit(1)


def ligature_subtables(font):
    """Every ligature substitution subtable of GSUB, unwrapped from extension lookups."""
    for lookup in font["GSUB"].table.LookupList.Lookup:
        for table in lookup.SubTable:
            if lookup.LookupType == 7:
                table = table.ExtSubTable
            if table.LookupType == 4:
                yield table


def main():
    icons = sorted(set(json.loads(ICON_LIST.read_text(encoding="utf-8"))["icons"]))

    version = json.loads((PACKAGE / "package.json").read_text(encoding="utf-8"))["version"]
    pinned = json.loads((CLIENT / "package.json").read_text(encoding="utf-8"))["dependencies"]["material-symbols"]
    if version != pinned:
        fail(f"node_modules holds material-symbols {version}, package.json pins {pinned}: run npm install")

    # Keep the font's own date, so the same input always gives the same bytes (the manifest hash).
    font = TTFont(PACKAGE / "material-symbols-outlined.woff2", recalcTimestamp=False)
    char_of = {glyph: chr(code) for code, glyph in font.getBestCmap().items()}
    tables = list(ligature_subtables(font))

    def spelling(first, ligature):
        return "".join(char_of.get(glyph, "\0") for glyph in [first, *ligature.Component])

    # Match the rules by the name they spell, not by glyph name: some names are aliases whose
    # ligature produces a glyph with another name (`create` → `edit`).
    wanted = set(icons)
    keep = {}
    for table in tables:
        for first in list(table.ligatures):
            kept = [lig for lig in table.ligatures[first] if spelling(first, lig) in wanted]
            for lig in kept:
                keep[spelling(first, lig)] = lig.LigGlyph
            if kept:
                table.ligatures[first] = kept
            else:
                del table.ligatures[first]

    unknown = [name for name in icons if name not in keep]
    if unknown:
        fail(f"not an icon of material-symbols {version}: {', '.join(unknown)}")

    options = subset.Options()
    options.layout_features = ["*"]
    options.flavor = "woff2"
    subsetter = subset.Subsetter(options)
    # The letters, digits and underscores that spell the names, and the icon glyphs.
    subsetter.populate(unicodes={ord(c) for c in "".join(icons)}, glyphs=set(keep.values()))
    subsetter.subset(font)
    subset.save_font(font, str(SUBSET), options)

    digest = hashlib.sha256(SUBSET.read_bytes()).hexdigest()
    manifest = {"package": "material-symbols", "version": version, "icons": icons, "sha256": digest}
    MANIFEST.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"{SUBSET.relative_to(ROOT)}: {len(icons)} icons, {SUBSET.stat().st_size} bytes")


if __name__ == "__main__":
    main()
