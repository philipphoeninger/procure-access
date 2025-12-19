import { Component, inject } from '@angular/core';
import {
  FormControl,
  FormGroupDirective,
  FormsModule,
  NgForm,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { finalize, map } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { LoginModel } from '../models/login.model';
import { SnackbarService } from '@app/core/services/snackbar.service';

/** Error when invalid control is dirty, touched, or submitted. */
export class MyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(
    control: FormControl | null,
    form: FormGroupDirective | NgForm | null,
  ): boolean {
    const isSubmitted = form && form.submitted;
    return !!(
      control &&
      control.invalid &&
      (control.dirty || control.touched || isSubmitted)
    );
  }
}

@Component({
  selector: 'pa-register',
  imports: [
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  public registerErrorMessage?: string;
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
  protected confirmPassword?: string;
  public keepSignedIn: boolean = false;

  emailFormControl = new FormControl('', [
    Validators.required,
    Validators.email,
  ]);

  matcher = new MyErrorStateMatcher();

  constructor(
    private router: Router,
    protected authService: AuthService,
    protected snackbarService: SnackbarService
  ) {}

  ngOnInit() {}

  onSubmit(form: NgForm, event: Event) {
    event.preventDefault();

    let registerCommand = new LoginModel(
      this.emailFormControl.value!,
      this.username!,
      this.password!
    );

    // TODO: start spinner
    this.authService
      .register(registerCommand)
      .pipe(
        map((response: any) => {
          if (response.succeeded) {
            this.router.navigateByUrl('/(login:auth)');
            this.snackbarService.showInfo('A registration has been sent to your email address and needs to be confirmed.\nPlease confirm it and come back to login.');
          } else {
            this.snackbarService.showInfo('The registration could not be completed. Please check your inputs and try again.');
          }
        }),
        finalize(() => {
          // TODO: stop spinner
        }),
      )
      .subscribe();
  }
}
