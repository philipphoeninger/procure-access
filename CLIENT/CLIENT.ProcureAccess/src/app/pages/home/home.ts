import { Component, inject } from '@angular/core';
import { ProcureAccessStore } from '@app/core/state/app.store';

@Component({
  selector: 'app-home',
  imports: [],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  protected store = inject(ProcureAccessStore);
}
