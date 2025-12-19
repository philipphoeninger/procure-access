import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatCardModule } from '@angular/material/card';
import { MatDivider } from "@angular/material/divider";

@Component({
  selector: 'app-home',
  imports: [
    MatButtonModule,
    MatIconModule,
    RouterModule,
    MatCardModule,
    MatDivider
],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  protected store = inject(ProcureAccessStore);
}
