import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { API_URL } from "@app/app.config";
import { lastValueFrom } from 'rxjs';
import { UICustomization } from '../models/uiCustomization.model';

@Injectable({ providedIn: 'root' })
export class SettingsApiService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    private http: HttpClient
  ) {}

  getUICustomization(): Promise<UICustomization> {
    return lastValueFrom(
      this.http.get<UICustomization>(`${this.apiUrl}/UICustomization`)
    );
  }

  updateUICustomization(command: UICustomization): Promise<UICustomization> {
    return lastValueFrom(
      this.http.put<UICustomization>(`${this.apiUrl}/UICustomization`, command)
    );
  }
}
