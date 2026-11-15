// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT, NgTemplateOutlet } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  ElementRef,
  inject,
  Injector,
  signal,
  viewChild,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { TextKey } from '@app/core/errors/error-summary-item';
import { problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { injectWideLayout } from '@app/layout/shell/wide-layout';
import { ErrorPage } from '@app/pages/error/error-page';
import { ConfirmDialog } from '@app/shared/components/confirm-dialog/confirm-dialog';
import { Notice } from '@app/shared/components/notice/notice';
import { InviteForm } from '../../components/invite-form/invite-form';
import {
  ORGANIZATION_PATH,
  ORGANIZATION_ROLES,
  OrganizationMember,
  OrganizationMembers,
  OrganizationRole,
  PendingInvitation,
  roleKey,
} from '../../models/organization';
import { OrganizationApiService } from '../../services/api/organization-api.service';
import { loadMembersView, MembersPageLoad, MembersPageView } from './members-page-load';

/**
 * S21 „Mitglieder von {{org}}“ at `/organisation/:id/mitglieder` (SPEC R-ORG-03, R-ORG-04, §8.9;
 * UX copy §5.7; Handoff §5.3: no Figma frame — `mat-table` from 600 px, a `<ul>` below, a role
 * select per member, errors in a notice above the list).
 *
 * Every member sees the members with their roles (Owners first, then by address) and „Organisation
 * verlassen“. An Owner also gets a role select (`org.members.role.label`, the hints as the
 * options' second line) and „Aus der Organisation entfernen“ (described by the member's address)
 * for every other member. A role change saves at once and is announced (`org.members.roleChanged`);
 * removing and leaving ask first (`ConfirmDialog`, UX copy U129, U130). Removing is announced
 * (`org.members.removed`); leaving announces `org.left` and opens S20. A refusal because the last
 * Owner would go (409 `last-owner`, TEN-11) or a missing antiforgery token shows the error notice
 * `org.members.error.title` above the list, and the list is read again, so a select shows the
 * stored role. When the focused control is gone after a change (the removed member's button, the
 * own select after demoting oneself), the focus moves to the H1.
 *
 * An Owner also sees „Offene Einladungen“ (while there are any; address, role, `org.invite.pending`
 * and „Einladung zurücknehmen“, described by the address, which revokes at once and announces
 * `org.invite.revoked`) and the form „Mitglied einladen“ (`pa-invite-form`; S5-17b, SPEC R-ORG-02,
 * UX copy U136, U137). Both lists are read again after every change.
 *
 * An unknown organisation, or one the user is no member of, shows „Seite nicht gefunden“ in place
 * (404, TEN-05); any other load failure the S80 server text.
 *
 * @spec docs/SPEC_v2.md R-ORG-02, R-ORG-03, R-ORG-04, §8.9; docs/design_handoff_procureaccess_v1.md §5.3, §2.2
 * @invariants TEN-11, A11Y-09
 */
@Component({
  selector: 'pa-members-page',
  imports: [
    ErrorPage,
    InviteForm,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatTableModule,
    NgTemplateOutlet,
    Notice,
    TranslatePipe,
  ],
  templateUrl: './members-page.html',
  styleUrl: './members-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembersPage {
  private readonly api = inject(OrganizationApiService);
  private readonly store = inject(ProcureAccessStore);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly document = inject(DOCUMENT);
  private readonly injector = inject(Injector);

  protected readonly view = signal<MembersPageView>(inject(MembersPageLoad).view);
  protected readonly wide = injectWideLayout();
  protected readonly roles = ORGANIZATION_ROLES;
  protected readonly roleKey = roleKey;
  /** The error notice above the list; null while there is none. */
  protected readonly error = signal<TextKey | null>(null);
  /** Counts the reloads, so the rows are rebuilt and every select shows the stored role again. */
  protected readonly version = signal(0);

  protected readonly organization = computed(() => {
    const view = this.view();
    return view.kind === 'members' ? view.data : null;
  });
  protected readonly members = computed(() => this.organization()?.members ?? []);
  /** The open invitations; only an Owner reads them (owner decision S5-17 D4 (a)). */
  protected readonly invitations = computed(() => {
    const view = this.view();
    return view.kind === 'members' ? (view.invitations ?? []) : [];
  });
  /** The members' addresses, so the invite form refuses a member's address before sending. */
  protected readonly memberEmails = computed(() => this.members().map((member) => member.email));
  private readonly ownId = computed(() => this.store.session()?.id ?? null);
  /** True while the signed-in user is an Owner: the role selects and „entfernen“ show. */
  protected readonly isOwner = computed(() => this.members().some((m) => m.userId === this.ownId() && m.role === 'owner'));
  protected readonly columns = computed(() => (this.isOwner() ? ['member', 'role', 'actions'] : ['member', 'role']));

  /** Rows are rebuilt after every reload, so a select shows the stored role again. */
  protected readonly trackRow = (_: number, member: OrganizationMember): string => `${member.userId}:${this.version()}`;

  private readonly heading = viewChild<ElementRef<HTMLElement>>('heading');

  constructor() {
    // The route change clears the title state; the name is known from the resolver.
    effect(() => {
      const name = this.organization()?.organization.name;
      if (name !== undefined) {
        this.pageTitle.setPageTitleState({ params: { org: name } });
      }
    });
  }

  protected isSelf(member: OrganizationMember): boolean {
    return member.userId === this.ownId();
  }

  /** The id of the element with the invited address: the description of „Einladung zurücknehmen“. */
  protected invitationAddressId(invitation: PendingInvitation): string {
    return `pa-invitation-${invitation.id}`;
  }

  /** The id of the element with the member's address: the description of „entfernen“. */
  protected addressId(member: OrganizationMember): string {
    return `pa-member-${member.userId}`;
  }

  /** The address, or „Gelöschtes Konto“ (UX copy U127), as text for names and announcements. */
  protected address(member: OrganizationMember): string {
    return member.email ?? (this.translate.instant('org.members.deletedAccount') as string);
  }

  protected async changeRole(organization: OrganizationMembers, member: OrganizationMember, role: OrganizationRole): Promise<void> {
    this.error.set(null);
    try {
      await this.api.changeRole(organization.organization.id, member.userId, role);
    } catch (error: unknown) {
      await this.refused(organization, error);
      return;
    }
    void this.announcer.announce('org.members.roleChanged', {
      email: this.address(member),
      role: this.translate.instant(roleKey(role)) as string,
    });
    await this.reload(organization);
  }

  protected async remove(organization: OrganizationMembers, member: OrganizationMember): Promise<void> {
    this.error.set(null);
    const confirmed = await ConfirmDialog.confirm(this.dialog, {
      titleKey: 'org.members.remove.title',
      bodyKey: 'org.members.remove.body',
      bodyParams: { email: this.address(member), org: organization.organization.name },
      confirmKey: 'org.members.remove',
    });
    if (!confirmed) {
      return;
    }
    try {
      await this.api.remove(organization.organization.id, member.userId);
    } catch (error: unknown) {
      await this.refused(organization, error);
      return;
    }
    void this.announcer.announce('org.members.removed', { email: this.address(member) });
    await this.reload(organization);
  }

  protected async leave(organization: OrganizationMembers): Promise<void> {
    const ownId = this.ownId();
    if (ownId === null) {
      return;
    }
    this.error.set(null);
    const name = organization.organization.name;
    const confirmed = await ConfirmDialog.confirm(this.dialog, {
      titleKey: 'org.leave',
      bodyKey: 'org.leave.body',
      bodyParams: { org: name },
      confirmKey: 'org.leave',
    });
    if (!confirmed) {
      return;
    }
    try {
      await this.api.remove(organization.organization.id, ownId);
    } catch (error: unknown) {
      await this.refused(organization, error);
      return;
    }
    // Announced first, so it is read before the title of S20 (UX copy U130).
    void this.announcer.announce('org.left', { org: name });
    await this.router.navigate([ORGANIZATION_PATH]);
  }

  /** Revokes an open invitation at once (UX copy U137), announces it and reads the lists again. */
  protected async revoke(organization: OrganizationMembers, invitation: PendingInvitation): Promise<void> {
    this.error.set(null);
    try {
      await this.api.revokeInvitation(organization.organization.id, invitation.id);
    } catch (error: unknown) {
      // 404: no longer open (accepted, expired or revoked meanwhile); the reload shows it.
      await this.refused(organization, error);
      return;
    }
    void this.announcer.announce('org.invite.revoked', { email: invitation.email });
    await this.reload(organization);
  }

  /** A stored invitation (sent, or kept after a failed send): the list of open invitations is read again. */
  protected async invited(organization: OrganizationMembers): Promise<void> {
    await this.reload(organization);
  }

  /** „Fehler:“ in the page title while the invite form shows field errors (A11Y-03). */
  protected formErrors(organization: OrganizationMembers, error: boolean): void {
    this.pageTitle.setPageTitleState({ params: { org: organization.organization.name }, error });
  }

  /** Shows a refusal the page shows itself and reads the list again; other failures keep the interceptor's states. */
  private async refused(organization: OrganizationMembers, error: unknown): Promise<void> {
    switch (problemTypeName(error)) {
      case 'last-owner':
        this.error.set({ key: 'org.members.lastOwner' });
        break;
      case 'antiforgery':
        this.error.set({ key: 'errors.antiforgery' });
        break;
      default:
        break;
    }
    await this.reload(organization);
  }

  /** Reads the list again; the focus goes to the H1 when the focused control is gone. */
  private async reload(organization: OrganizationMembers): Promise<void> {
    this.view.set(await loadMembersView(this.api, organization.organization.id, this.ownId()));
    this.version.update((v) => v + 1);
    afterNextRender(
      () => {
        const active = this.document.activeElement;
        if (!active || active === this.document.body || !active.isConnected) {
          this.heading()?.nativeElement.focus();
        }
      },
      { injector: this.injector },
    );
  }
}
