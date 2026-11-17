// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * The file name of a `Content-Disposition` header: `filename*` (RFC 6266, UTF-8, as ASP.NET writes
 * it) before `filename`; null when the header names none.
 */
export function fileNameOf(contentDisposition: string | null): string | null {
  if (contentDisposition === null) return null;
  const extended = /filename\*\s*=\s*UTF-8''([^;]+)/i.exec(contentDisposition);
  if (extended) {
    try {
      return decodeURIComponent(extended[1].trim());
    } catch {
      // A malformed escape: fall back to the plain name.
    }
  }
  const plain = /filename\s*=\s*"?([^";]+)"?/i.exec(contentDisposition);
  return plain ? plain[1].trim() : null;
}
