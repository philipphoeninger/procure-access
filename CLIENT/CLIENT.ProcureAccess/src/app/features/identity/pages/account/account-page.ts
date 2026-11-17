// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { problemDetails, problemTypeName } from '@app/core/errors/problem-details';
import { FileSaver } from '@app/core/files/file-saver';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ErrorPage } from '@app/pages/error/error-page';
import { Notice } from '@app/shared/components/notice/notice';
import { ConfirmRequiredNotice } from '../../components/confirm-required-notice/confirm-required-notice';
import { DeleteAccountDialog } from '../../dialogs/delete-account-dialog/delete-account-dialog';
import { AccountApiService } from '../../services/api/account-api.service';
import { AccountLoad } from './account-load';

/**
 * S61 „Konto“ at `/konto` (SPEC R-AUTH-09, R-AUTH-10, §8.4, §8.13; UX copy §5.14; Handoff §5.3:
 * no Figma frame, form card as in S51).
 *
 * H1, for an unconfirmed address the notice `auth.confirm.required` with „erneut senden“, and two
 * cards. „Ihre Daten herunterladen“ hands the JSON file of the personal data export to the
 * browser's download. „Konto löschen“ opens the dialog „Konto endgültig löschen?“; once the user
 * typed the address, the account is deleted, every in-memory state of the user goes and the start
 * page shows with the focus on its H1, as after the sign-out (owner decision S5-28 P3 (a)). A
 * refusal — the user is the only member with „Verwalten“ of an organisation with other members
 * (409 `sole-owner`, TEN-11) — shows the error notice `account.delete.error.title` with one line
 * per organisation. Every other failure keeps the system states of the error interceptor. A failed
 * read when the page opens shows the S80 server text in place.
 *
 * @spec docs/SPEC_v2.md R-AUTH-09, R-AUTH-10, §8.13; docs/design_handoff_procureaccess_v1.md §5.3
 * @invariants TEN-11, A11Y-09
 */
@Component({
  selector: 'pa-account-page',
  imports: [ConfirmRequiredNotice, ErrorPage, MatButtonModule, MatProgressSpinnerModule, Notice, TranslatePipe],
  templateUrl: './account-page.html',
  styleUrl: './account-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountPage {
  private readonly api = inject(AccountApiService);
  private readonly store = inject(ProcureAccessStore);
  private readonly dialog = inject(MatDialog);
  private readonly fileSaver = inject(FileSaver);
  private readonly router = inject(Router);
  private readonly pageTitle = inject(PageTitleStrategy);

  /** The account as read when the page opened; null when the read failed (S80 in place). */
  protected readonly account = inject(AccountLoad).account;

  protected readonly exporting = signal(false);
  protected readonly deleting = signal(false);
  /** The organisations of a refused deletion (409 `sole-owner`); empty while there is none. */
  protected readonly refusedBy = signal<readonly string[]>([]);

  protected async download(): Promise<void> {
    if (this.exporting()) {
      return;
    }
    this.exporting.set(true);
    try {
      const file = await this.api.export();
      this.fileSaver.save(file.content, file.fileName);
    } catch {
      // The error interceptor showed the failure.
    } finally {
      this.exporting.set(false);
    }
  }

  protected async delete(): Promise<void> {
    if (this.deleting() || !this.account) {
      return;
    }
    this.refusedBy.set([]);
    if (!(await DeleteAccountDialog.confirm(this.dialog, { email: this.account.email }))) {
      return;
    }

    this.deleting.set(true);
    try {
      await this.api.delete();
    } catch (error: unknown) {
      if (problemTypeName(error) === 'sole-owner') {
        this.refusedBy.set(problemDetails(error)?.organizations ?? []);
      }
      // Every other failure: the error interceptor showed it.
      return;
    } finally {
      this.deleting.set(false);
    }

    // The server ended the session with the deletion; nothing of the user stays here either.
    this.store.forgetUser();
    await this.router.navigateByUrl('/');
    this.pageTitle.focusPageHeading();
  }
}
