// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// A Markdown file imported by the client is its text: `angular.json` maps `.md` to esbuild's
// `text` loader (the legal pages, S6-28, R-SHL-07).
declare module '*.md' {
  const text: string;
  export default text;
}
