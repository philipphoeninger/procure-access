// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  Injector,
  signal,
  viewChild,
  viewChildren,
} from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslatePipe } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ErrorSummaryItem } from '@app/core/errors/error-summary-item';
import { problemTypeName } from '@app/core/errors/problem-details';
import { ConfirmRequiredNotice } from '@app/features/identity/components/confirm-required-notice/confirm-required-notice';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { Notice } from '@app/shared/components/notice/notice';

import { ActiveShareLink, sharedListUrl, ShareLinkExpiry } from '../../models/share-links';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { EXPORT_DIALOG_PANEL_CLASS } from '../export-dialog/export-dialog';

/** What S15 needs to know: the saved list whose links it manages. */
export interface ShareDialogData {
  listId: string;
}

/** A failure the dialog shows in its error notice above the actions (S13 rules). */
type ShareError = 'failed' | 'antiforgery';

/** The id of the date field, for the error summary and the first focus. */
const EXPIRY_ID = 'pa-share-expiry';

/**
 * S15 · Freigabelink erstellen (R-SAV-04, SD15; UX copy §5.6; Handoff §5.3: no Figma frame, the
 * S13 dialog rules). Opened from a saved list by Editor and Owner (owner decision S5-24 2a).
 *
 * - On opening it loads the dates a new link may end on today and the active links
 *   (`GET /configurations/{id}/share-links`): the server works the dates out in its time zone
 *   (owner decision S5-24 1a, A (a)), so the dialog does no date arithmetic. Then the first focus
 *   goes to „Gültig bis“, the browser's own date field (owner decision S5-24 B (a)) with the 🔧
 *   default and `min`/`max`.
 * - „Link erstellen“ checks the date on submit (Handoff §6.4): required, from tomorrow to the
 *   maximum; otherwise the error summary takes the focus and the field shows
 *   `criteria.share.expiry.error` with the last allowed day. The server's field error at
 *   `expiresOn` shows the same text.
 * - A created link stays in the dialog (owner decision S5-24 5a): the read-only field „Freigabelink“
 *   with the hint that it shows only now (the server keeps only its hash, TEN-08) takes the focus;
 *   „Link kopieren“ announces `criteria.share.copied`, or shows `criteria.share.copyFailed` and
 *   selects the link when the browser refuses the clipboard. The list of links is loaded again.
 * - „Gültige Links“ lists every active link with „Erstellt am …, gültig bis …“ and „Link ungültig
 *   machen“, which revokes at once, announces `criteria.share.revoked` and moves the focus to the
 *   next link's button, else to the heading (owner decision S5-24 3a).
 * - 403 `email-unconfirmed` (SEC-10) shows `pa-confirm-required-notice`, which takes the focus;
 *   `antiforgery` its text; every other failure `criteria.share.error` under
 *   `criteria.share.error.title`. Requests are marked `CALLER_SHOWS_FAILURES`.
 *
 * Esc or „Schließen“ close the dialog; MatDialog returns the focus to „Freigabelink erstellen“.
 * Below 600 px the dialog fills the screen and stacks its actions in DOM order (S13-M).
 *
 * @spec docs/SPEC_v2.md R-SAV-04, SD15, §8.10; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants TEN-08, TEN-12, SEC-10, A11Y-09
 */
@Component({
  selector: 'pa-share-dialog',
  imports: [
    ConfirmRequiredNotice,
    ErrorSummary,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    Notice,
    ReactiveFormsModule,
    TranslatePipe,
  ],
  templateUrl: './share-dialog.html',
  styleUrl: './share-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShareDialog {
  /**
   * Opens S15 for the list. The focus waits on the dialog until it is open and the dates are
   * loaded, then goes into „Gültig bis“.
   */
  static open(dialog: MatDialog, data: ShareDialogData): MatDialogRef<ShareDialog> {
    return dialog.open(ShareDialog, {
      data,
      autoFocus: 'dialog',
      panelClass: EXPORT_DIALOG_PANEL_CLASS,
      maxWidth: '100vw',
    });
  }

  private readonly data = inject<ShareDialogData>(MAT_DIALOG_DATA);
  private readonly api = inject(ConfiguratorApiService);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly injector = inject(Injector);
  private readonly document = inject(DOCUMENT);
  /** Resolves once MatDialog has opened the dialog and set its own first focus. */
  private readonly opened = firstValueFrom(inject<MatDialogRef<ShareDialog>>(MatDialogRef).afterOpened());
  private readonly language = this.document.documentElement.lang || 'de';
  /** A calendar day `yyyy-MM-dd` as the date it names, wherever the browser is. */
  private readonly dayFormat = new Intl.DateTimeFormat(this.language, { dateStyle: 'medium', timeZone: 'UTC' });
  private readonly instantFormat = new Intl.DateTimeFormat(this.language, { dateStyle: 'medium' });

  private readonly expiryInput = viewChild<ElementRef<HTMLInputElement>>('expiryInput');
  private readonly linkInput = viewChild<ElementRef<HTMLInputElement>>('linkInput');
  private readonly confirmNotice = viewChild<ElementRef<HTMLElement>>('confirmNotice');
  private readonly linksHeading = viewChild<ElementRef<HTMLElement>>('linksHeading');
  private readonly revokeButtons = viewChildren<string, ElementRef<HTMLButtonElement>>('revokeButton', { read: ElementRef });

  protected readonly expiryId = EXPIRY_ID;
  protected readonly expiryHintId = 'pa-share-expiry-hint';
  protected readonly linkId = 'pa-share-link';
  protected readonly linksHeadingId = 'pa-share-links-heading';

  protected readonly status = signal<'loading' | 'ready' | 'loadFailed'>('loading');
  protected readonly expiry = signal<ShareLinkExpiry | null>(null);
  protected readonly links = signal<ActiveShareLink[]>([]);
  /** The link of the last „Link erstellen“, shown until the dialog closes or the link is revoked. */
  protected readonly created = signal<{ id: string; url: string } | null>(null);
  protected readonly copyFailed = signal(false);
  protected readonly busy = signal(false);
  protected readonly revoking = signal<string | null>(null);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  protected readonly error = signal<ShareError | null>(null);
  /** True after the server refused the unconfirmed address (SEC-10). */
  protected readonly confirmationRequired = signal(false);
  /** `{{maxDate}}` of `criteria.share.expiry.error`: the last day a link may have. */
  protected readonly maxDate = computed(() => {
    const expiry = this.expiry();
    return expiry === null ? '' : this.formatDay(expiry.latest);
  });

  protected readonly expiresOn = new FormControl('', {
    nonNullable: true,
    validators: [(control: AbstractControl<string>) => this.validExpiry(control)],
  });

  private readonly submitted = signal(false);
  private destroyed = false;

  /** The field error shows after the first submit (Handoff §6.4), a server error at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  constructor() {
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));
    void this.load(true);
  }

  /** „Erneut versuchen“ after the dates and links could not be loaded. */
  protected retry(): void {
    void this.load(true);
  }

  protected errorKey(error: ShareError): string {
    return error === 'antiforgery' ? 'errors.antiforgery' : 'criteria.share.error';
  }

  /** „Erstellt am {{created}}, gültig bis {{expires}}“ of one link. */
  protected itemParams(link: ActiveShareLink): Record<string, string> {
    return { created: this.instantFormat.format(new Date(link.createdAt)), expires: this.formatDay(link.expiresOn) };
  }

  protected async create(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy() || this.status() !== 'ready') return;
    this.submitted.set(true);
    if (this.expiresOn.invalid) {
      this.summary.set([
        {
          message: { key: 'criteria.share.expiry.error', params: { maxDate: this.maxDate() } },
          field: { id: EXPIRY_ID, labelKey: 'criteria.share.expiry' },
        },
      ]);
      return;
    }

    this.summary.set([]);
    this.error.set(null);
    this.busy.set(true);
    try {
      const link = await this.api.createShareLink(this.data.listId, this.expiresOn.value);
      if (this.destroyed) return;
      this.created.set({ id: link.id, url: sharedListUrl(this.document.location.origin, link.token) });
      this.copyFailed.set(false);
      afterNextRender(() => this.linkInput()?.nativeElement.focus(), { injector: this.injector });
      void this.load(false);
    } catch (error: unknown) {
      if (this.destroyed) return;
      if (problemTypeName(error) === 'validation') {
        // The server counts the day itself (TEN-12): its field error shows as the client's.
        this.expiresOn.setErrors({ server: true });
        this.summary.set([
          {
            message: { key: 'criteria.share.expiry.error', params: { maxDate: this.maxDate() } },
            field: { id: EXPIRY_ID, labelKey: 'criteria.share.expiry' },
          },
        ]);
      } else {
        this.refused(error);
      }
    } finally {
      this.busy.set(false);
    }
  }

  /** „Link kopieren“: the clipboard, or the selected link when the browser refuses it. */
  protected async copy(): Promise<void> {
    const created = this.created();
    if (created === null) return;
    try {
      // Without a clipboard (an insecure origin, an old browser) the link is selected instead.
      const clipboard = this.document.defaultView?.navigator.clipboard;
      if (clipboard === undefined) throw new Error('No clipboard');
      await clipboard.writeText(created.url);
      this.copyFailed.set(false);
      void this.announcer.announce('criteria.share.copied');
    } catch {
      this.copyFailed.set(true);
      const input = this.linkInput()?.nativeElement;
      input?.focus();
      input?.select();
    }
  }

  /** „Link ungültig machen“ (owner decision S5-24 3a): at once, then the focus to the next link or the heading. */
  protected async revoke(link: ActiveShareLink): Promise<void> {
    if (this.revoking() !== null) return;
    const index = this.links().findIndex((item) => item.id === link.id);
    this.error.set(null);
    this.revoking.set(link.id);
    try {
      await this.api.revokeShareLink(link.id);
      if (this.destroyed) return;
      this.links.update((links) => links.filter((item) => item.id !== link.id));
      if (this.created()?.id === link.id) this.created.set(null);
      void this.announcer.announce('criteria.share.revoked');
      afterNextRender(
        () => {
          const buttons = this.revokeButtons();
          const next = buttons[Math.min(index, buttons.length - 1)];
          (next ?? this.linksHeading())?.nativeElement.focus();
        },
        { injector: this.injector },
      );
    } catch (error: unknown) {
      if (!this.destroyed) this.refused(error);
    } finally {
      this.revoking.set(null);
    }
  }

  /**
   * Loads the dates and the active links. The first load fills the date with the default and then
   * puts the focus into the field; a later one (after „Link erstellen“) only refreshes them.
   */
  private async load(first: boolean): Promise<void> {
    if (first) this.status.set('loading');
    try {
      const answer = await this.api.shareLinks(this.data.listId);
      if (this.destroyed) return;
      this.expiry.set(answer.expiry);
      this.links.set(answer.links);
      this.expiresOn.updateValueAndValidity();
      if (first) {
        this.expiresOn.setValue(answer.expiry.default);
        this.status.set('ready');
        await this.opened;
        if (!this.destroyed) {
          afterNextRender(() => this.expiryInput()?.nativeElement.focus(), { injector: this.injector });
        }
      }
    } catch (error: unknown) {
      if (this.destroyed) return;
      if (first) {
        this.status.set('loadFailed');
      } else {
        this.refused(error);
      }
    }
  }

  private refused(error: unknown): void {
    switch (problemTypeName(error)) {
      case 'email-unconfirmed':
        this.confirmationRequired.set(true);
        afterNextRender(() => this.confirmNotice()?.nativeElement.focus(), { injector: this.injector });
        return;
      case 'antiforgery':
        this.error.set('antiforgery');
        return;
      default:
        this.error.set('failed');
    }
  }

  /** Required, and from tomorrow to the maximum (`yyyy-MM-dd` compares as text). */
  private validExpiry(control: AbstractControl<string>): ValidationErrors | null {
    const expiry = this.expiry();
    const value = control.value;
    if (value === '') return { required: true };
    if (expiry !== null && (value < expiry.earliest || value > expiry.latest)) {
      return { range: true };
    }
    return null;
  }

  private formatDay(day: string): string {
    return this.dayFormat.format(new Date(`${day}T00:00:00Z`));
  }
}
