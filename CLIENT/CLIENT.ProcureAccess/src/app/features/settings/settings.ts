import { Component, inject, model, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ColorPickerDirective } from 'ngx-color-picker';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { ProcureAccessStore } from '@app/core/state/app.store';

@Component({
  selector: 'pa-settings',
  imports: [
    FormsModule,
    ColorPickerDirective,
    MatSlideToggleModule,
    MatListModule,
    MatDividerModule,
    MatButtonModule
  ],
  templateUrl: './settings.html',
  styleUrl: './settings.scss'
})
export class Settings {
  protected store = inject(ProcureAccessStore);
  protected snackbarService = inject(SnackbarService);

  protected updateUICustomization = model(this.store.uiCustomization());

  constructor() {}

  ngOnInit() {}

  saveSettings() {
    this.store.updateUICustomization(this.updateUICustomization());
    // this.snackbarService.showInfo(
    //   this.store.isAuthenticated() ? "Settings saved" : "Settings saved as cookie"
    // );
  }

  resetSettings() {
    this.snackbarService.showInfo("Settings resetted to standard");
  }
}
