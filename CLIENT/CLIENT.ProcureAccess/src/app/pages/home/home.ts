import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatCardModule } from '@angular/material/card';
import { MatDivider } from "@angular/material/divider";
import { AuthService } from '@app/features/identity/services/auth.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-home',
  imports: [
    MatButtonModule,
    MatIconModule,
    RouterModule,
    MatCardModule,
    MatDivider,
    TranslatePipe
],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  protected store = inject(ProcureAccessStore);
  protected authService = inject(AuthService);
}
