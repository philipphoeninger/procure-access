import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { httpAppConfig } from '../../../app.config';
import { lastValueFrom } from 'rxjs';
import { UICustomization } from '../models/uiCustomization.model';

@Injectable({ providedIn: 'root' })
export class SettingsApiService {
  constructor(
    private http: HttpClient
  ) {}

  updateUICustomization(command: UICustomization): Promise<any> {
    return lastValueFrom(
      this.http.put<any>(`${httpAppConfig.apiEndpoint}/UICustomization`, command)
    );
  }
}
