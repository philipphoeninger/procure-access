import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule} from '@angular/material/list';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { UpdateUserDialog } from '../dialogs/update-user-dialog/update-user-dialog';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { DeleteAccountDialog } from '../dialogs/delete-account-dialog/delete-account-dialog';
import { MatIconModule } from "@angular/material/icon";
import { RouterModule } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { HasNotPermissionDirective } from '../directives/has-not-permission.directive';
import { ProposalStatus } from '@app/features/proposal/models/proposal-status.enum';
import { TranslatePipe } from '@ngx-translate/core';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface UserInformation {
  position: number,
  name: string,
  value: string | undefined,
  actions: any
}

@Component({
  selector: 'pa-profile',
  imports: [
    MatListModule, 
    MatDividerModule, 
    MatTableModule, 
    MatButtonModule, 
    MatIconModule,
    RouterModule,
    MatMenuModule,
    HasNotPermissionDirective,
    MatTooltipModule,
    TranslatePipe
  ],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Profile {
  protected store = inject(ProcureAccessStore);
  ProposalStatus = ProposalStatus;

  displayedColumns: string[] = ['name', 'value', 'actions'];
  userInformation: UserInformation[] = [
      { position: 1, name: 'Email', value: this.store.user()?.email, actions: '' },
      { position: 2, name: 'Password', value: '********', actions: '' }
      // { position: 3, name: '2FA enabled', value: this.store.user()?.twoFAEnabled ? 'Yes' : 'No', actions: '' }
    ];
  dataSource = new MatTableDataSource<UserInformation>(this.userInformation);

  readonly dialog = inject(MatDialog);

  constructor(
      protected snackbarService: SnackbarService
  ) {}

  ngOnInit() {}

  openChangeUserDialog(element: any): void {
    const dialogRef = this.dialog.open(UpdateUserDialog, {
      data: { property: element.name, value: element.value },
      width: '420px'
    });

    dialogRef.afterClosed().subscribe((newValue: string) => {
      console.log(newValue);
      // TODO
      // if (email !== undefined) {
      //   this.authService
      //     .forgotPassword(email)
      //     .pipe(
      //       map((response: any) => {
      //         this.showSnackbar('A passwort reset was sent to the given mail address.');
      //       })
      //     )
      //     .subscribe();
      // }
    });
  }

  deleteAccount() {
    this.dialog.open(DeleteAccountDialog, {
      data: { email: this.store.user()?.email },
      width: '420px'
    })
    .afterClosed().subscribe((toDelete: boolean) => {
      if (toDelete) this.store.deleteAccount();
    });
  }
}
