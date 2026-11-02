// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpContext } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal, WritableSignal } from '@angular/core';
import { FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatListModule } from '@angular/material/list';
import { FiltersSelection } from "@app/features/filters/selection/filters-selection";
import { ProductDto } from '../models/product.dto';
import { ProposalDto } from '@app/features/proposal/models/proposal.dto';
import { ProposalApiService } from '@app/features/proposal/services/api/proposal-api.service';
import { CreateProductDto } from '../models/create-product.dto';
import { UpsertProposal } from '@app/features/proposal/models/upsert-proposal-request.model';
import { ProposalStatus } from '../../proposal/models/proposal-status.enum';
import { ReviewProposal } from '@app/features/proposal/models/review-proposal-request.model';
import { ApproveProposal } from '@app/features/proposal/models/approve-proposal-request.model';
import { HasPermissionDirective } from '@app/features/identity/directives/has-permission.directive';
import { TranslatePipe } from '@ngx-translate/core';
import { ErrorSummaryItem } from '@app/core/errors/error-summary-item';
import { applyServerErrors, FORM_SHOWS_FIELD_ERRORS, ServerErrorField } from '@app/core/errors/problem-details';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';

// Server field path (as sent in the request body) → form field (API-02).
const SERVER_FIELDS: Record<string, { control: string; id: string; labelKey: string }> = {
  'product.name': { control: 'name', id: 'product-proposal-name', labelKey: 'COMMON.NAME' },
  'product.link': { control: 'link', id: 'product-proposal-link', labelKey: 'COMMON.LINK' },
  'product.description': { control: 'description', id: 'product-proposal-description', labelKey: 'COMMON.DESCRIPTION' },
};

@Component({
  selector: 'pa-product-proposal',
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
    FiltersSelection,
    HasPermissionDirective,
    TranslatePipe,
    ErrorSummary
],
  templateUrl: './product-proposal.html',
  styleUrl: './product-proposal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductProposal {
  protected store = inject(ProcureAccessStore);
  protected proposalApiService = inject(ProposalApiService);
  private route = inject(ActivatedRoute);
  readonly dialog = inject(MatDialog);

  readonly proposalId = signal<number | undefined>(undefined);

  protected proposal = signal<ProposalDto>(new ProposalDto(
    0,
    this.store.user()?.id!,
    undefined,
    new ProductDto(
      0,
      "",
      undefined,
      undefined
    ),
    undefined,
    ProposalStatus.pending,
    undefined,
    new Date(),
    undefined
  ));

  public formErrorMessage?: string;
  /** Lines of the error summary; replaced on every failed submit (Handoff §6.4). */
  protected readonly errorSummary = signal<ErrorSummaryItem[]>([]);

  constructor(protected snackbarService: SnackbarService) {
    let routeId = this.route.snapshot.paramMap.get('id');
    if (routeId == null || !+routeId) return; //gate
    this.proposalId.set(+routeId);
  }

  async ngOnInit() {
    await this.store.loadProposals();

    let stateProposal = this.store.getProposalById(this.proposalId()!);
    if (stateProposal) this.proposal.set(stateProposal);
  }

  async onSubmit(form: NgForm, event: Event) {
    event.preventDefault();
    let productId = this.proposal().product?.id;
    productId = productId != undefined && productId > 0 ? productId : undefined;

    let upsertProduct: CreateProductDto = new CreateProductDto(
      this.proposal().product?.name!,
      this.proposal().product?.link,
      this.proposal().product?.description,
      this.store.selectedCriteriaFiltersIds()
    );

    // The form shows validation errors itself, so the error interceptor shows no snackbar for them.
    const context = new HttpContext().set(FORM_SHOWS_FIELD_ERRORS, true);

    try {
      // TODO: only send changed values
      if (this.proposalId()) {
        let reviewCommand = new ReviewProposal(
          this.proposalId()!,
          upsertProduct,
          undefined
        );
        await this.store.reviewProposal(reviewCommand, context);
      } else {
        let upsertCommand = new UpsertProposal(
          this.proposalId(),
          productId, // needed later, if user can make proposals for existing products
          upsertProduct,
          undefined,
          undefined
        );
        await this.store.upsertProposal(upsertCommand, context);
      }
      this.errorSummary.set([]);
    } catch (error) {
      // Other failures: the error interceptor has shown them already.
      this.errorSummary.set(applyServerErrors(error, path => this.serverField(form, path)));
    }
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

  private serverField(form: NgForm, path: string): ServerErrorField | undefined {
    const field = SERVER_FIELDS[path];
    const control = field && form.controls[field.control];
    return control ? { control, id: field.id, labelKey: field.labelKey } : undefined;
  }

  approve(isApproved: boolean) {
    let approveCommand = new ApproveProposal(
      this.proposalId()!,
      isApproved,
      ""
    );
    this.store.approveProposal(approveCommand);
  }
}
