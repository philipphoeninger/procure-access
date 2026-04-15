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
import { ProposalStatus } from '../models/proposal-status.enum';
import { ReviewProposal } from '@app/features/proposal/models/review-proposal-request.model';
import { ApproveProposal } from '@app/features/proposal/models/approve-proposal-request.model';
import { HasPermissionDirective } from '@app/features/identity/directives/has-permission.directive';

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
    HasPermissionDirective
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

  selectedCriteriaFilterIds: WritableSignal<number[]> = signal([]);

  public formErrorMessage?: string;

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

  onSubmit(form: NgForm, event: Event) {
    let productId = this.proposal().product?.id;
    productId = productId != undefined && productId > 0 ? productId : undefined;
    
    let upsertProduct: CreateProductDto = new CreateProductDto(
      this.proposal().product?.name!,
      this.proposal().product?.link,
      this.proposal().product?.description
    );

    // TODO: only send changed values
    if (this.proposalId()) {
      let reviewCommand = new ReviewProposal(
        this.proposalId()!,
        upsertProduct,
        undefined
      );
      this.store.reviewProposal(reviewCommand);
    } else {
      let upsertCommand = new UpsertProposal(
        this.proposalId(),
        productId, // needed later, if user can make proposals for existing products
        upsertProduct,
        undefined,
        undefined
      );
      this.store.upsertProposal(upsertCommand);
    }

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

  approve(isApproved: boolean) {
    let approveCommand = new ApproveProposal(
      this.proposalId()!,
      isApproved,
      ""
    );
    this.store.approveProposal(approveCommand);
  }
}
