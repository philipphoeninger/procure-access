import { ChangeDetectionStrategy, Component, inject, model } from '@angular/core';
import { FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { finalize, map } from 'rxjs/operators';
import { AuthService } from '@features/identity/services/auth.service';
import { LoginModel } from '@features/identity/models/login.model';
import { ForgotPasswordDialog } from '@features/identity/dialogs/forgot-password-dialog/forgot-password-dialog';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { User } from '../models/user.model';

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
  styleUrl: './login.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Login {
  protected store = inject(ProcureAccessStore);

  public loginErrorMessage?: string;
  public showLanguageSelection = true;

  protected logoPath = '/werte_it_logo.jpg';

  public languages = [
    { value: 'en', label: 'English' },
    { value: 'de', label: 'Deutsch' },
    { value: 'fr', label: 'Français' },
  ];
  public selectedLanguage = model('en');

  protected email? = model('');
  protected password? = model('');
  public keepSignedIn = model(false);

  readonly dialog = inject(MatDialog);
  private _snackBar = inject(MatSnackBar);

  constructor(
    protected authService: AuthService,
    private router: Router,
  ) {}

  ngOnInit() {}

  openForgotPwDialog(): void {
    const dialogRef = this.dialog.open(ForgotPasswordDialog, {
      data: { email: this.email!() },
      width: '420px'
    });

    dialogRef.afterClosed().subscribe((email: string) => {
      if (email !== undefined) {
        this.authService
          .forgotPassword(email)
          .pipe(
            map((response: any) => {
              this.showSnackbar('A passwort reset was sent to the given mail address.');
            })
          )
          .subscribe();
      }
    });
  }

  showSnackbar(message: string) {
    this._snackBar.open(message, 'Close', {
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }

  onSubmit(form: NgForm, event: Event) {
    event.preventDefault();
    let loginCommand = new LoginModel(this.email!(), '', this.password!());

    // TODO: start spinner
    this.authService
      .login(loginCommand)
      .pipe(
        map((response: { token: string, username: string }) => {
          if (response.token) {
            localStorage.setItem('procure-access-token', response.token);
            let user: User = new User(loginCommand.email, response.username, false);
            this.store.setUser(user);
            this.router.navigateByUrl('/home');
          } else {
            this.showSnackbar('No login found for the given information. Please check your inputs and try again.');
          }
        }),
        finalize(() => {
          // TODO: stop spinner
        }),
      )
      .subscribe();
  }
}
