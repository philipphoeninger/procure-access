import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { AuthService } from '@app/features/identity/services/auth.service';

@Component({
  selector: 'app-unauthorized',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    RouterModule
],
  templateUrl: './unauthorized.html',
  styleUrl: './unauthorized.scss'
})
export class Unauthorized {
    protected authService = inject(AuthService);
}
