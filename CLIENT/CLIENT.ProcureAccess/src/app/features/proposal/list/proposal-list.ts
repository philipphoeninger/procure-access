import { ChangeDetectionStrategy, Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { ProposalApiService } from '../services/api/proposal-api.service';
import { FormsModule } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { Router, RouterModule } from '@angular/router';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ProposalStatus } from '@app/features/proposal/models/proposal-status.enum';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'pa-proposal',
  imports: [
    FormsModule,
    MatTableModule, 
    MatButtonModule,
    MatSortModule,
    MatIconModule,
    RouterModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    TranslatePipe
  ],
  templateUrl: './proposal-list.html',
  styleUrl: './proposal-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProposalList implements OnInit {
  protected store = inject(ProcureAccessStore);
  protected proposalApiService = inject(ProposalApiService);
  private router = inject(Router);

  private _liveAnnouncer = inject(LiveAnnouncer);

  displayedColumns: string[] = [
    'actions',
    'id',
    'proposerId',
    'approverId',
    'productName',
    'criterionName',
    'status',
    'note',
    'createdAt',
    'finishedAt'
  ];
  dataSource = new MatTableDataSource(this.store.proposals());

  @ViewChild(MatSort)
  sort!: MatSort;

  ProposalStatus = ProposalStatus;

  constructor(protected snackbarService: SnackbarService) {}

  ngOnInit() {}

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

  // TODO: create helper method (is used somewhere exactly the same already)
    /** Announce the change in sort state for assistive technology. */
  announceSortChange(sortState: Sort) {
    // This example uses English messages. If your application supports
    // multiple language, you would internationalize these strings.
    // Furthermore, you can customize the message to add additional
    // details about the values being sorted.
    if (sortState.direction) {
      this._liveAnnouncer.announce(`Sorted ${sortState.direction}ending`);
    } else {
      this._liveAnnouncer.announce('Sorting cleared');
    }
  }

  openProposal(id: number, isProduct: boolean) {
    if (isProduct) this.router.navigateByUrl('/product-proposal/' + id);
    else this.router.navigateByUrl('/criterion-proposal/' + id);
  }
}
