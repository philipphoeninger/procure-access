import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '@features/identity/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('ProcureAccess');

  loggedIn = false;

  constructor(private authService: AuthService) {
    this.loggedIn = authService.isAuthenticated();
  }
}
