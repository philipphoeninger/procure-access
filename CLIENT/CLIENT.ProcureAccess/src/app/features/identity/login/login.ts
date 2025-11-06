import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { finalize, map } from 'rxjs/operators';
import { AuthService } from '@features/identity/services/auth.service';
import { LoginModel } from '@features/identity/models/login.model';

@Component({
  selector: 'pa-login',
  imports: [
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  public loginErrorMessage?: string;
  public showLanguageSelection = true;

  protected logoPath = '/werte_it_logo.jpg';

  public languages = [
    { value: 'en', label: 'English' },
    { value: 'de', label: 'Deutsch' },
    { value: 'fr', label: 'Français' },
  ];
  public selectedLanguage = 'en';

  protected username?: string;
  protected password?: string;
  public keepSignedIn: boolean = false;

  constructor(
    protected authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit() {}

  onSubmit(form: NgForm, event: Event) {
    event.preventDefault();
    let loginCommand = new LoginModel(this.username!, this.password!);

    // TODO: start spinner
    this.authService
      .login(loginCommand)
      .pipe(
        map((response: any) => {
          // TODO: check if login succeeded (check status code)
          localStorage.setItem('procure-access-token', response.token);
          this.router.navigateByUrl('/home');
        }),
        finalize(() => {
          // TODO: stop spinner
        }),
      )
      .subscribe();
  }

}
