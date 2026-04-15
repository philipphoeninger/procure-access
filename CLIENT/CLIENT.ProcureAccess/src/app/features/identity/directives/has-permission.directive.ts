import { Directive, Input, TemplateRef, ViewContainerRef } from "@angular/core";
import { AuthService } from "../services/auth.service";

@Directive({
  selector: '[appHasPermission]'
})
export class HasPermissionDirective {

  @Input() set appHasPermission(permission: string) {
    if (this.auth.hasPermission(permission)) {
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
