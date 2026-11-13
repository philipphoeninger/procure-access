// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';

import { ProcureAccessStore } from '@app/core/state/app.store';

/** The opposite of `appHasPermission`: renders its template while the session lacks the permission. */
@Directive({ selector: '[appHasNotPermission]' })
export class HasNotPermissionDirective {
  readonly appHasNotPermission = input.required<string>();

  private readonly store = inject(ProcureAccessStore);
  private readonly template = inject<TemplateRef<unknown>>(TemplateRef);
  private readonly view = inject(ViewContainerRef);

  constructor() {
    effect(() => {
      this.view.clear();
      if (!this.store.hasPermission(this.appHasNotPermission())) {
        this.view.createEmbeddedView(this.template);
      }
    });
  }
}
