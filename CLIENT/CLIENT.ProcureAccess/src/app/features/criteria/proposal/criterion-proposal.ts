import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
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
import { ProposalApiService } from '@app/features/proposal/services/api/proposal-api.service';
import { ProposalDto } from '@app/features/proposal/models/proposal.dto';
import { CriterionDto } from '../models/criterion.dto';
import { ProposalStatus } from '@app/features/proposal/models/proposal-status.enum';
import { ApproveProposal } from '@app/features/proposal/models/approve-proposal-request.model';
import { CreateCriterionDto } from '../models/create-criterion.dto';
import { ReviewProposal } from '@app/features/proposal/models/review-proposal-request.model';
import { UpsertProposal } from '@app/features/proposal/models/upsert-proposal-request.model';
import { HasPermissionDirective } from '@app/features/identity/directives/has-permission.directive';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'pa-criterion-proposal',
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
    HasPermissionDirective,
    TranslatePipe
],
  templateUrl: './criterion-proposal.html',
  styleUrl: './criterion-proposal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CriterionProposal {
  protected store = inject(ProcureAccessStore);
  protected proposalApiService = inject(ProposalApiService);
  private route = inject(ActivatedRoute);
  readonly dialog = inject(MatDialog);

  readonly proposalId = signal<number | undefined>(undefined);

  protected proposal = signal<ProposalDto>(new ProposalDto(
    0,
    this.store.user()?.id!,
    undefined,
    undefined,
    new CriterionDto(
      0,
      "",
      "",
      1
    ),
    ProposalStatus.pending,
    undefined,
    new Date(),
    undefined
  ));
  productPartsControl = new FormControl<number[]>(
     [this.proposal().criterion!.criteriaFilterId]
  );

  public formErrorMessage?: string;

  constructor(protected snackbarService: SnackbarService) {
    let routeId = this.route.snapshot.paramMap.get('id');
    if (routeId == null || !+routeId) return; //gate
    this.proposalId.set(+routeId);
  }

  async ngOnInit() {
    await this.store.loadProposals();

    let stateProposal = this.store.getProposalById(this.proposalId()!);
    if (stateProposal) {
      this.proposal.set(stateProposal);
      this.productPartsControl.setValue([stateProposal.criterion!.criteriaFilterId]);
    }
  }

  onSubmit(form: NgForm, event: Event) {
    let criterionId = this.proposal().criterion?.id;
    criterionId = criterionId != undefined && criterionId > 0 ? criterionId : undefined;
    
    let upsertCriterion: CreateCriterionDto = new CreateCriterionDto(
      this.proposal().criterion?.name!,
      this.proposal().criterion?.description!,
      this.productPartsControl.value![0]
    );

    // TODO: only send changed values
    if (this.proposalId()) {
      let reviewCommand = new ReviewProposal(
        this.proposalId()!,
        undefined,
        upsertCriterion
      );
      this.store.reviewProposal(reviewCommand);
    } else {
      let upsertCommand = new UpsertProposal(
        this.proposalId(),
        undefined,
        undefined,
        criterionId, // needed later, if user can make proposals for existing criteria
        upsertCriterion
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

  approve(isApproved: boolean) {
    let approveCommand = new ApproveProposal(
      this.proposalId()!,
      isApproved,
      ""
    );
    this.store.approveProposal(approveCommand);
  }
}
