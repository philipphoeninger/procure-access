import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../identity/shared/auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard {
  constructor(
    private authService: AuthService,
    private router: Router,
  ) {}

  canActivate(): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    } else {
      this.router.navigate(['', { outlets: { login: ['auth'] } }]);
      return false;
    }
  }
}

