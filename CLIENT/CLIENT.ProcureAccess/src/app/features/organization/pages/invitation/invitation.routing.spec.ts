// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Location } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, RouterOutlet, TitleStrategy } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@app/features/identity/services/auth.service';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { InvitationDetails } from '../../models/organization';
import { OrganizationApiService } from '../../services/api/organization-api.service';
import { INVITATION_ROUTES } from './invitation.routing';
import { InvitationPageState } from './invitation-page-state';

@Component({
  template: '<h1>Startseite</h1>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HomePage {}

@Component({
  template: '<h1>Mitglieder</h1>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class MembersStub {}

@Component({
  imports: [RouterOutlet],
  template: '<main id="main"><router-outlet /><router-outlet name="login" /></main>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class Host {}

const base = 'https://procureaccess.de/problems/';

const OPEN: InvitationDetails = {
  organizationName: 'Stadt Musterhausen',
  role: 'editor',
  addressMatches: null,
  invitedEmail: null,
  alreadyMember: false,
  organizationId: null,
};

describe('Page of an invitation link (R-ORG-02, /organisation/einladung)', () => {
  let viewInvitation: ReturnType<typeof vi.fn>;
  let acceptInvitation: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let organizationJoined: ReturnType<typeof vi.fn>;
  let announcePageTitle: ReturnType<typeof vi.fn>;
  let showLogin: ReturnType<typeof vi.fn>;
  let showRegister: ReturnType<typeof vi.fn>;
  let signedIn: ReturnType<typeof signal<boolean>>;

  async function openLink(url: string, options: { signedIn?: boolean; view?: InvitationDetails | { rejectWith: unknown } } = {}) {
    const view = options.view ?? OPEN;
    viewInvitation = 'rejectWith' in view ? vi.fn().mockRejectedValue(view.rejectWith) : vi.fn().mockResolvedValue(view);
    acceptInvitation = vi.fn().mockResolvedValue({ organizationId: 'org-1' });
    announce = vi.fn().mockResolvedValue(undefined);
    organizationJoined = vi.fn().mockResolvedValue(undefined);
    announcePageTitle = vi.fn().mockResolvedValue(undefined);
    showLogin = vi.fn().mockResolvedValue(true);
    showRegister = vi.fn();
    signedIn = signal(options.signedIn ?? false);
    const rendered = await render(Host, {
      routes: [
        { path: '', component: HomePage },
        { path: 'organisation/:id/mitglieder', component: MembersStub },
        { path: 'organisation/einladung', children: INVITATION_ROUTES },
      ],
      providers: [
        provideTestTranslations(),
        { provide: TitleStrategy, useExisting: PageTitleStrategy },
        { provide: OrganizationApiService, useValue: { viewInvitation, acceptInvitation } },
        { provide: AuthService, useValue: { showLogin, showRegister } },
        { provide: A11yAnnouncerService, useValue: { announce, announcePageTitle } },
        {
          provide: ProcureAccessStore,
          useValue: {
            signedIn,
            session: () => (signedIn() ? { id: 'u1', email: 'neu@example.test' } : null),
            sessionReady: () => Promise.resolve(),
            incrementLoadingCount: vi.fn(),
            decrementLoadingCount: vi.fn(),
            resendConfirmation: vi.fn(),
            organizationJoined,
          },
        },
      ],
    });
    const router = TestBed.inject(Router);
    await router.navigateByUrl(url);
    await settle(rendered.fixture);
    return { ...rendered, router, location: TestBed.inject(Location) };
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  afterEach(() => {
    TestBed.inject(InvitationPageState).token = null;
  });

  it('takes the token out of the address and shows organisation and role with „Anmelden“ and „Konto erstellen“ when signed out', async () => {
    const { router, location } = await openLink('/organisation/einladung?token=abc_DEF-123');

    expect(router.url).toBe('/organisation/einladung');
    expect(location.path()).toBe('/organisation/einladung');
    expect(viewInvitation).toHaveBeenCalledWith('abc_DEF-123');
    expect(screen.getByRole('heading', { level: 1, name: 'Einladung in eine Organisation' })).toBeTruthy();
    expect(screen.getByText('Sie wurden in die Organisation Stadt Musterhausen eingeladen. Ihre Rolle: Bearbeiten.')).toBeTruthy();
    expect(screen.getByText(/Melden Sie sich mit der eingeladenen E-Mail-Adresse an/)).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Einladung annehmen' })).toBeNull();
    expect(document.title).toBe('Einladung in eine Organisation – ProcureAccess');
    await expectNoAxeViolations();
  });

  it('opens S01 over the page for „Anmelden“ and S02 for „Konto erstellen“', async () => {
    const { fixture } = await openLink('/organisation/einladung?token=t1');

    screen.getByRole('button', { name: 'Anmelden' }).click();
    await fixture.whenStable();
    screen.getByRole('button', { name: 'Konto erstellen' }).click();
    await fixture.whenStable();

    expect(showLogin).toHaveBeenCalledOnce();
    expect(showRegister).toHaveBeenCalledOnce();
  });

  it('reads the invitation again on the next navigation after a sign-in and then offers „Einladung annehmen“', async () => {
    const { fixture, router } = await openLink('/organisation/einladung?token=t1');
    viewInvitation.mockResolvedValue({ ...OPEN, addressMatches: true });

    // S01 closes after the sign-in: a navigation of the login outlet
    signedIn.set(true);
    await router.navigate([{ outlets: { login: null } }]);
    await settle(fixture);

    expect(viewInvitation).toHaveBeenLastCalledWith('t1');
    expect(screen.getByRole('button', { name: 'Einladung annehmen' })).toBeTruthy();
    expect(screen.queryByText(/Melden Sie sich mit der eingeladenen E-Mail-Adresse an/)).toBeNull();
  });

  it('accepts the invitation, announces it and opens the members page', async () => {
    const { fixture, router } = await openLink('/organisation/einladung?token=t1', {
      signedIn: true,
      view: { ...OPEN, addressMatches: true },
    });

    screen.getByRole('button', { name: 'Einladung annehmen' }).click();
    await settle(fixture);

    expect(acceptInvitation).toHaveBeenCalledExactlyOnceWith('t1');
    expect(organizationJoined).toHaveBeenCalledExactlyOnceWith('org-1');
    expect(announce).toHaveBeenCalledWith('org.invite.accepted', { org: 'Stadt Musterhausen' });
    expect(router.url).toBe('/organisation/org-1/mitglieder');
    expect(TestBed.inject(InvitationPageState).token).toBeNull();
  });

  it('shows the notice with „erneut senden“ and the focus for an unconfirmed address and keeps the button (SEC-10)', async () => {
    const { fixture } = await openLink('/organisation/einladung?token=t1', { signedIn: true, view: { ...OPEN, addressMatches: true } });
    acceptInvitation.mockRejectedValue(new HttpErrorResponse({ status: 403, error: { type: `${base}email-unconfirmed` } }));

    screen.getByRole('button', { name: 'Einladung annehmen' }).click();
    await settle(fixture);

    expect(screen.getByText(/Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse/)).toBeTruthy();
    expect(document.activeElement?.contains(screen.getByRole('button', { name: 'Bestätigungs-E-Mail erneut senden' }))).toBe(true);
    expect(screen.getByRole('button', { name: 'Einladung annehmen' })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('names the invited address and offers no button when the signed-in address is another one', async () => {
    await openLink('/organisation/einladung?token=t1', {
      signedIn: true,
      view: { ...OPEN, addressMatches: false, invitedEmail: 'eingeladen@example.test' },
    });

    expect(
      screen.getByText(
        'Diese Einladung gilt für eine andere E-Mail-Adresse. Melden Sie sich mit eingeladen@example.test an oder bitten Sie um eine neue Einladung.',
      ),
    ).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Einladung annehmen' })).toBeNull();
    await expectNoAxeViolations();
  });

  it('names the invited address from a refused accept (403 invitation-mismatch)', async () => {
    const { fixture } = await openLink('/organisation/einladung?token=t1', { signedIn: true, view: { ...OPEN, addressMatches: true } });
    acceptInvitation.mockRejectedValue(
      new HttpErrorResponse({ status: 403, error: { type: `${base}invitation-mismatch`, invitedEmail: 'eingeladen@example.test' } }),
    );

    screen.getByRole('button', { name: 'Einladung annehmen' }).click();
    await settle(fixture);

    expect(screen.getByText(/Melden Sie sich mit eingeladen@example\.test an/)).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Einladung annehmen' })).toBeNull();
  });

  it('shows „Einladung nicht mehr gültig“ in place with title, focus and announcement when accepting refuses the token', async () => {
    const { fixture } = await openLink('/organisation/einladung?token=t1', { signedIn: true, view: { ...OPEN, addressMatches: true } });
    acceptInvitation.mockRejectedValue(new HttpErrorResponse({ status: 400, error: { type: `${base}validation`, errors: { token: ['x'] } } }));

    screen.getByRole('button', { name: 'Einladung annehmen' }).click();
    await settle(fixture);
    await settle(fixture);

    const heading = screen.getByRole('heading', { level: 1, name: 'Einladung nicht mehr gültig' });
    expect(document.activeElement).toBe(heading);
    expect(document.title).toBe('Einladung nicht mehr gültig – ProcureAccess');
    expect(announcePageTitle).toHaveBeenLastCalledWith('Einladung nicht mehr gültig – ProcureAccess');
    expect(TestBed.inject(InvitationPageState).token).toBeNull();
  });

  it('shows „Einladung nicht mehr gültig“ with „Zur Startseite“ for a link the server refuses', async () => {
    await openLink('/organisation/einladung?token=t1', {
      view: { rejectWith: new HttpErrorResponse({ status: 400, error: { type: `${base}validation`, errors: { token: ['x'] } } }) },
    });

    expect(screen.getByRole('heading', { level: 1, name: 'Einladung nicht mehr gültig' })).toBeTruthy();
    expect(screen.getByText(/Diese Einladung ist nicht mehr gültig/)).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Zur Startseite' }).getAttribute('href')).toBe('/');
    expect(document.title).toBe('Einladung nicht mehr gültig – ProcureAccess');
    await expectNoAxeViolations();
  });

  it('shows „Einladung nicht mehr gültig“ without asking the server after a reload, which lost the token', async () => {
    await openLink('/organisation/einladung');

    expect(viewInvitation).not.toHaveBeenCalled();
    expect(screen.getByRole('heading', { level: 1, name: 'Einladung nicht mehr gültig' })).toBeTruthy();
  });

  it('shows the S80 server text for any other failure', async () => {
    await openLink('/organisation/einladung?token=t1', { view: { rejectWith: new HttpErrorResponse({ status: 500 }) } });

    expect(screen.getByRole('heading', { level: 1, name: 'Etwas ist schiefgelaufen' })).toBeTruthy();
    expect(document.title).toBe('Etwas ist schiefgelaufen – ProcureAccess');
  });

  it('opens the members page for a member who opens the accepted link again (owner decision S5-17 D7 (a))', async () => {
    const { router } = await openLink('/organisation/einladung?token=t1', {
      signedIn: true,
      view: { ...OPEN, addressMatches: true, alreadyMember: true, organizationId: 'org-1' },
    });

    expect(router.url).toBe('/organisation/org-1/mitglieder');
    expect(screen.getByRole('heading', { level: 1, name: 'Mitglieder' })).toBeTruthy();
  });
});
