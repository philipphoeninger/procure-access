// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';

import { ProcureAccessStore } from '@app/core/state/app.store';

/**
 * Renders its template while the session carries the permission (legacy proposal pages until
 * step 5). The permissions come from the probe (`withIdentity().permissions`), so the view follows
 * every sign-in and sign-out; the server still checks every request itself.
 */
@Directive({ selector: '[appHasPermission]' })
export class HasPermissionDirective {
  readonly appHasPermission = input.required<string>();

  private readonly store = inject(ProcureAccessStore);
  private readonly template = inject<TemplateRef<unknown>>(TemplateRef);
  private readonly view = inject(ViewContainerRef);

  constructor() {
    effect(() => {
      this.view.clear();
      if (this.store.hasPermission(this.appHasPermission())) {
        this.view.createEmbeddedView(this.template);
      }
    });
  }
}
