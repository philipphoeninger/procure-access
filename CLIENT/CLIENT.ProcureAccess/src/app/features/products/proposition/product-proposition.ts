import { ChangeDetectionStrategy, Component, inject, model, signal, ViewChild, WritableSignal } from '@angular/core';
import { FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { catchError, finalize, map, throwError } from 'rxjs';
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatListModule } from '@angular/material/list';
import { FiltersSelection } from "@app/features/filters/selection/filters-selection";

@Component({
  selector: 'pa-product-proposition',
  imports: [
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatCheckboxModule,
    RouterModule,
    MatListModule,
    FiltersSelection
],
  templateUrl: './product-proposition.html',
  styleUrl: './product-proposition.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductProposition {
  protected store = inject(ProcureAccessStore);

  readonly dialog = inject(MatDialog);

  protected name? = model('');
  protected link? = model('');
  protected description? = model('');

    selectedCriteriaFilterIds: WritableSignal<number[]> = signal([]);

  public formErrorMessage?: string;


  constructor(protected snackbarService: SnackbarService) {}

  ngOnInit() {}

onSubmit(form: NgForm, event: Event) {
    // TODO: send new product to BE

    
    // event.preventDefault();
    // let loginCommand = new LoginModel(this.email!(), this.password!());

    // this.store.incrementLoadingCount();
    // this.authService
    //   .login(loginCommand)
    //   .pipe(
    //     map((response: HttpResponse<{ token: string, email: string, uiCustomization: UICustomization }>) => {
    //       if (!response.body?.token) {
    //         this.snackbarService.showInfo('Login failed. Please try again.');
    //         return;
    //       } //gate
    //       localStorage.setItem(this.jwtName, response.body.token);
    //       let user: User = new User(loginCommand.email, false);
    //       this.store.setUser(user);
    //       this.store.setUICustomization(response.body.uiCustomization);
    //       this.router.navigateByUrl('/home');
    //     }),
    //     catchError((error: HttpErrorResponse) => {
    //       this.snackbarService.showInfo('No login found for the given information. Please check your inputs and try again.');
    //       return throwError(() => new Error(error.message));
    //     }),
    //     finalize(() => {
    //       this.store.decrementLoadingCount();
    //     }),
    //   )
    //   .subscribe();
  }

    fdsa(event: number[]) {
      this.selectedCriteriaFilterIds.set(event);
    }
}
