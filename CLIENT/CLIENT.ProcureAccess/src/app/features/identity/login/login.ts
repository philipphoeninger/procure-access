import { ChangeDetectionStrategy, Component, Inject, inject, model } from '@angular/core';
import { FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
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
import { ForgotPasswordDialog } from '@features/identity/dialogs/forgot-password-dialog/forgot-password-dialog';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { JWT_NAME, REFRESH_TOKEN_NAME } from '@app/app.config';
import { UserDto } from '../models/user.dto';
import { TranslatePipe } from '@ngx-translate/core';

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
    MatCheckboxModule,
    TranslatePipe
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

  protected email? = model('');
  protected password? = model('');
  public keepSignedIn = model(false);

  readonly dialog = inject(MatDialog);

  constructor(
    @Inject(JWT_NAME) private jwtName: string,
    @Inject(REFRESH_TOKEN_NAME) private refreshTokenName: string,
    protected authService: AuthService,
    private router: Router,
    protected snackbarService: SnackbarService
  ) {}

  ngOnInit() {}

  openForgotPwDialog(): void {
    const dialogRef = this.dialog.open(ForgotPasswordDialog, {
      data: { email: this.email!() },
      width: '420px'
    });

    dialogRef.afterClosed().subscribe((email: string) => {
      if (email !== undefined) {
        this.store.incrementLoadingCount();
        this.authService
          .forgotPassword(email)
          .pipe(finalize(() => this.store.decrementLoadingCount()))
          .subscribe();
      }
    });
  }

  onSubmit(form: NgForm, event: Event) {
    event.preventDefault();
    let loginCommand = new LoginModel(this.email!(), this.password!());

    this.store.incrementLoadingCount();
    this.authService
      .login(loginCommand)
      .pipe(
        map((response) => {
          if (!response.accessToken) return; //gate
          this.authService.loadUserFromToken(response.accessToken);
          localStorage.setItem(this.jwtName, response.accessToken);
          localStorage.setItem(this.refreshTokenName, response.refreshToken);
          let user: UserDto = new UserDto(response.user.id, response.user.email);
          this.store.setUser(user);
          // set settings
          this.store.setUICustomization(response.user.uiCustomization);
          this.store.setLanguage(response.user.uiCustomization.language);
          this.store.removeSettingsFromLocalStorage();
          this.store.loadProposals();
          this.router.navigateByUrl('/home');
        }),
        finalize(() => this.store.decrementLoadingCount())
      )
      .subscribe();
  }
}
