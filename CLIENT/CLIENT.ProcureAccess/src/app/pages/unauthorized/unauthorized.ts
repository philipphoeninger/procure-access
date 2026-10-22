// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { AuthService } from '@app/features/identity/services/auth.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-unauthorized',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    RouterModule,
    TranslatePipe
],
  templateUrl: './unauthorized.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './unauthorized.scss'
})
export class Unauthorized {
    protected authService = inject(AuthService);
}
