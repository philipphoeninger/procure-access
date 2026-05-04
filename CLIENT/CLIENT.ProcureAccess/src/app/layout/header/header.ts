import { Component, inject, OnInit } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '@features/identity/services/auth.service';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { HasPermissionDirective } from '@app/features/identity/directives/has-permission.directive';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'pa-header',
  imports: [
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatMenuModule,
    RouterModule,
    MatTooltipModule,
    HasPermissionDirective,
    TranslatePipe
  ],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header {
  protected logoPath = '/werte_it_logo.jpg';
  protected store = inject(ProcureAccessStore);

  constructor(
    protected authService: AuthService,
    private router: Router,
  ) {}

  logout() {
    this.authService.logout();
    this.store.setUser(null);
    this.store.reloadUICustomization();
  }
}
