import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { API_URL } from "@app/app.config";
import { lastValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class IdentityApiService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    private http: HttpClient
  ) {}

    deleteAccount(): Promise<any>  {
        return lastValueFrom(
            this.http.delete<any>(`${this.apiUrl}/Users`)
        );
    }
}
