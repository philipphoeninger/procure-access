import { ChangeDetectionStrategy, Component, inject, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ColorPickerDirective } from 'ngx-color-picker';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'pa-settings',
  imports: [
    FormsModule,
    ColorPickerDirective,
    MatSlideToggleModule,
    MatListModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    TranslatePipe
],
  templateUrl: './settings.html',
  styleUrl: './settings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Settings {
  protected store = inject(ProcureAccessStore);
  protected snackbarService = inject(SnackbarService);

  protected updateUICustomization = model(this.store.uiCustomization());

  constructor() {}

  toggleColorMode() {
    this.store.toggleDarkMode();
  }

  toggleContrastMode() {
    this.store.toggleContrastMode();
  }

  saveSettings() {
    this.store.updateUICustomization(this.store.uiCustomization());
  }

  resetSettings() {
    // TODO
    this.snackbarService.showInfo("Settings resetted to standard");
  }
}
