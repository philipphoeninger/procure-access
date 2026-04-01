import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { API_URL, JWT_NAME, REFRESH_TOKEN_NAME } from "@app/app.config";
import { Observable } from 'rxjs';
import { LoginModel } from '../models/login.model';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Router } from '@angular/router';
import { ResetPasswordModel } from '../models/reset-password.model';
import { AuthResponse } from '../models/auth-response.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    @Inject(JWT_NAME) private jwtName: string,
    @Inject(REFRESH_TOKEN_NAME) private refreshTokenName: string,
    private http: HttpClient,
    private router: Router
  ) {}

  login(command: LoginModel): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/signIn`, command);
  }

  register(command: LoginModel): Observable<{ succeeded: boolean, errors: [] }> {
    return this.http.post<any>(`${this.apiUrl}/signUp`, command);
  }

  public showLogin() {
    this.router.navigate(['', { outlets: { login: ['auth'] } }]);
  }

  public showRegister() {
    this.router.navigate(['', { outlets: { login: ['register'] } }]);
  }

  public logout() {
    localStorage.removeItem(this.jwtName);
    localStorage.removeItem(this.refreshTokenName);
    this.router.navigateByUrl('/home'); // /(login:auth)
  }

  public isAuthenticated(): boolean {
    const token = localStorage.getItem(this.jwtName);
    const helper = new JwtHelperService();
    const isExpired = helper.isTokenExpired(token);
    return !isExpired;
  }

  public forgotPassword(email: string) {
    return this.http.post<any>(`${this.apiUrl}/forgot-password`, { email }, { observe: 'response' });
  }

  public resetPassword(command: ResetPasswordModel) {
    return this.http.post<{ succeeded: boolean }>(`${this.apiUrl}/reset-password`, command);
  }

  public confirmEmail(userId: string, token: string) {
    return this.http.get<any>(`${this.apiUrl}/confirm-email`, {
        params: {
          userId,
          token
        },
        observe: 'response' 
      });
  }
}
