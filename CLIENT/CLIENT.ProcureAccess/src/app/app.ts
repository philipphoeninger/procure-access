// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '@features/identity/services/auth.service';
import { Header } from '@layout/header/header';
import { ProcureAccessStore } from './core/state/app.store';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, MatProgressSpinnerModule],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('ProcureAccess');
  protected store = inject(ProcureAccessStore);
  protected authService = inject(AuthService);

  showHeader = true;

  constructor() {}
}
