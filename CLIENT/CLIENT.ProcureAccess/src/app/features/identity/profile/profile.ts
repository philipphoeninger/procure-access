import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule} from '@angular/material/list';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { UpdateUserDialog } from '../dialogs/update-user-dialog/update-user-dialog';

export interface UserInformation {
  position: number,
  name: string,
  value: string | undefined,
  actions: any
}

@Component({
  selector: 'pa-profile',
  imports: [MatListModule, MatDividerModule, MatTableModule, MatButtonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Profile {
  protected store = inject(ProcureAccessStore);

  displayedColumns: string[] = ['name', 'value', 'actions'];
  dataSource: UserInformation[] = [];

  readonly dialog = inject(MatDialog);

  ngOnInit() {
    let userInformation: UserInformation[] = [
      { position: 1, name: 'Username', value: this.store.user()?.username, actions: '' },
      { position: 2, name: 'Email', value: this.store.user()?.email, actions: '' },
      { position: 3, name: 'Password', value: '********', actions: '' },
      { position: 4, name: '2FA enabled', value: this.store.user()?.twoFAEnabled ? 'Yes' : 'No', actions: '' }
    ];
    this.dataSource = userInformation;
  }

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
}
