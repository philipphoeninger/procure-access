import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { API_URL, JWT_NAME } from "@app/app.config";
import { Observable } from 'rxjs';
import { LoginModel } from '../models/login.model';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    @Inject(JWT_NAME) private jwtName: string,
    private http: HttpClient,
    private router: Router
  ) {}

  login(command: LoginModel): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/signIn`, command, { observe: 'response' });
  }

  register(command: LoginModel): Observable<any> {
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
    this.router.navigateByUrl('/home'); // /(login:auth)
  }

  public isAuthenticated(): boolean {
    const token = localStorage.getItem(this.jwtName);
    const helper = new JwtHelperService();
    const isExpired = helper.isTokenExpired(token);
    return !isExpired;
  }

  public forgotPassword(email: string) {
    return this.http.post<any>(`${this.apiUrl}/forgotPassword`, { email });
  }
}
