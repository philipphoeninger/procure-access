import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '@features/identity/services/auth.service';
import { Header } from '@layout/header/header';
import { ProcureAccessStore } from './core/state/app.store';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, MatProgressSpinnerModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('ProcureAccess');
  protected store = inject(ProcureAccessStore);
  protected authService = inject(AuthService);

  showHeader = true;

  constructor() {}
}
