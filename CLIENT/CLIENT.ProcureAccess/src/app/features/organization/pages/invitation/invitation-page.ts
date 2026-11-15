// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  inject,
  Injector,
  linkedSignal,
  signal,
  viewChild,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { map } from 'rxjs';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { TextKey } from '@app/core/errors/error-summary-item';
import { problemDetails, problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ConfirmRequiredNotice } from '@app/features/identity/components/confirm-required-notice/confirm-required-notice';
import { AuthService } from '@app/features/identity/services/auth.service';
import { ErrorPage } from '@app/pages/error/error-page';
import { Notice } from '@app/shared/components/notice/notice';
import { MEMBERS_SEGMENT, ORGANIZATION_PATH, roleKey } from '../../models/organization';
import { OrganizationApiService } from '../../services/api/organization-api.service';
import { INVITATION_VIEW_KEY, InvitationPageState, InvitationView } from './invitation-page-state';

type OpenView = Extract<InvitationView, { kind: 'open' }>;

/**
 * The page of an invitation link at `/organisation/einladung` (SPEC R-ORG-02, §8.9; UX copy §5.7,
 * U133 – U135; Handoff §5.3: no Figma frame, one column like the confirmation pages of S02).
 *
 * „Einladung in eine Organisation“ with `org.invite.accept.text` (organisation and role), then:
 * - signed out: `org.invite.signIn` with „Anmelden“ (S01 over the page, which stays; closing it
 *   reads the invitation again for the signed-in address) and „Konto erstellen“ (S02);
 * - signed in with the invited address: „Einladung annehmen“ with the button spinner. Accepted →
 *   `org.invite.accepted` is announced and the members page S21 opens; an unconfirmed address
 *   (403 `email-unconfirmed`, SEC-10) shows `pa-confirm-required-notice` with the focus, and the
 *   button stays for a second try after the confirmation; a link no longer valid (400) shows
 *   „Einladung nicht mehr gültig“ in place (title, H1 focus and announcement, A11Y-03);
 * - signed in with another address: the error notice `org.invite.mismatch` with the invited
 *   address (403 `invitation-mismatch`, C80), no button.
 *
 * An unknown, expired, revoked or used link shows „Einladung nicht mehr gültig“ with
 * `org.invite.invalid` and „Zur Startseite“; any other failure the S80 server text. A member who
 * opens an accepted link again goes to S21 (the resolver, owner decision S5-17 D7 (a)).
 *
 * @spec docs/SPEC_v2.md R-ORG-02, §8.9; docs/design_handoff_procureaccess_v1.md §5.3
 * @invariants SEC-10, A11Y-03, A11Y-09
 */
@Component({
  selector: 'pa-invitation-page',
  imports: [ConfirmRequiredNotice, ErrorPage, MatButtonModule, MatProgressSpinnerModule, Notice, RouterLink, TranslatePipe],
  templateUrl: './invitation-page.html',
  styleUrl: './invitation-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvitationPage {
  private readonly api = inject(OrganizationApiService);
  private readonly authService = inject(AuthService);
  private readonly store = inject(ProcureAccessStore);
  private readonly state = inject(InvitationPageState);
  private readonly router = inject(Router);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly injector = inject(Injector);

  private readonly loaded = toSignal(
    inject(ActivatedRoute).data.pipe(map((data) => data[INVITATION_VIEW_KEY] as InvitationView)),
    { requireSync: true },
  );

  /** The resolved view, or „nicht mehr gültig“ after accepting refused the token; a new resolve starts over. */
  protected readonly view = linkedSignal<InvitationView>(() => this.loaded());
  protected readonly signedIn = computed(() => this.store.signedIn());
  protected readonly roleKey = roleKey;

  protected readonly busy = signal(false);
  /** True after the server refused the unconfirmed address (SEC-10); cleared by a new resolve. */
  protected readonly confirmationRequired = linkedSignal(() => {
    this.loaded();
    return false;
  });
  /** The invited address of a refused accept (403 `invitation-mismatch`); cleared by a new resolve. */
  private readonly refusedFor = linkedSignal<string | null>(() => {
    this.loaded();
    return null;
  });
  /** A failure the page shows itself (a missing antiforgery token); cleared by a new resolve. */
  protected readonly failure = linkedSignal<TextKey | null>(() => {
    this.loaded();
    return null;
  });

  /** The invited address when the signed-in address is another one (the read, or a refused accept). */
  protected readonly mismatch = computed(() => {
    const view = this.view();
    if (view.kind !== 'open' || !this.signedIn()) return null;
    return this.refusedFor() ?? (view.details.addressMatches === false ? view.details.invitedEmail : null);
  });

  private readonly confirmNotice = viewChild<ElementRef<HTMLElement>>('confirmNotice');

  protected signIn(): void {
    void this.authService.showLogin();
  }

  protected register(): void {
    this.authService.showRegister();
  }

  protected async accept(view: OpenView): Promise<void> {
    if (this.busy()) {
      return;
    }
    this.busy.set(true);
    this.confirmationRequired.set(false);
    this.failure.set(null);
    let organizationId: string;
    try {
      ({ organizationId } = await this.api.acceptInvitation(view.token));
    } catch (error: unknown) {
      this.refused(error);
      return;
    } finally {
      this.busy.set(false);
    }
    this.state.token = null;
    // Announced first, so it is read before the title of S21 (as `org.left`, U130).
    void this.announcer.announce('org.invite.accepted', { org: view.details.organizationName });
    await this.router.navigate([ORGANIZATION_PATH, organizationId, MEMBERS_SEGMENT]);
  }

  private refused(error: unknown): void {
    switch (problemTypeName(error)) {
      case 'email-unconfirmed':
        this.confirmationRequired.set(true);
        afterNextRender(() => this.confirmNotice()?.nativeElement.focus(), { injector: this.injector });
        return;
      case 'invitation-mismatch':
        this.refusedFor.set(problemDetails(error)?.invitedEmail ?? null);
        return;
      case 'validation':
        this.state.token = null;
        this.view.set({ kind: 'invalid' });
        this.pageTitle.changePageInPlace('pageTitle.org.invite.invalid');
        return;
      case 'antiforgery':
        this.failure.set({ key: 'errors.antiforgery' });
        return;
      default:
      // Anything else keeps the system states of the error interceptor.
    }
  }
}
