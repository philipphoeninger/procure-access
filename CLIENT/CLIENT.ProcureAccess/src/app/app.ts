import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '@features/identity/services/auth.service';
import { Header } from '@layout/header/header';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('ProcureAccess');
  protected authService = inject(AuthService);

  showHeader = true;

  constructor() {}
}
