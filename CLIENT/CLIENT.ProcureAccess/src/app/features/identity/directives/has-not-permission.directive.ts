// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Directive, Input, TemplateRef, ViewContainerRef } from "@angular/core";
import { AuthService } from "../services/auth.service";

@Directive({
  selector: '[appHasNotPermission]'
})
export class HasNotPermissionDirective {

  @Input() set appHasNotPermission(permission: string) {
    if (this.auth.hasNotPermission(permission)) {
      this.view.createEmbeddedView(this.template);
    } else {
      this.view.clear();
    }
  }

  constructor(
    private auth: AuthService,
    private template: TemplateRef<any>,
    private view: ViewContainerRef
  ) {}
}
