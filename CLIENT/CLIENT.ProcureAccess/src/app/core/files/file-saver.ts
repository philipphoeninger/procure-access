// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';

/**
 * Hands a file the client received to the browser's download (S13, R-LST-11): a temporary object
 * URL on a link with `download`, clicked once and removed. The link never takes the focus, so the
 * caller decides where it goes. A service, so tests replace the download.
 */
@Injectable({ providedIn: 'root' })
export class FileSaver {
  private readonly document = inject(DOCUMENT);

  save(content: Blob, fileName: string): void {
    const view = this.document.defaultView;
    if (!view) return;
    const url = view.URL.createObjectURL(content);
    const link = this.document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.hidden = true;
    this.document.body.appendChild(link);
    try {
      link.click();
    } finally {
      link.remove();
      // Some browsers start the download only after the click returns: free the URL a moment later.
      view.setTimeout(() => view.URL.revokeObjectURL(url), 1000);
    }
  }
}
