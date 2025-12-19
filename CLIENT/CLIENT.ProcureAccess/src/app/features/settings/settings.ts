import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ColorPickerDirective } from 'ngx-color-picker';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { SnackbarService } from '@app/core/services/snackbar.service';

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
  protected fgColor = '#111111';
  protected bgColor = '#ffffff';
  protected textColor = '#000000';

  protected orientationControl = false;
  protected highContrastControl = false;

  constructor(
    protected snackbarService: SnackbarService
  ) {}

  saveSettings() {
    this.snackbarService.showInfo("Settings saved");
  }

  resetSettings() {
    this.snackbarService.showInfo("Settings resetted to standard");
  }
}
