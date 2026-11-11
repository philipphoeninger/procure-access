// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, effect, inject } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { Title } from '@angular/platform-browser';
import { provideRouter, Router, RouterOutlet, Routes, TitleStrategy } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { screen } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { A11yAnnouncerService } from './a11y-announcer.service';
import { PAGE_TITLE_KEY, PageTitleStrategy } from './page-title.strategy';

@Component({
  template: '<h1>Impressum</h1><p>Text</p>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class PageWithHeading {}

@Component({
  template: '<p>Seite ohne Überschrift</p>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class PageWithoutHeading {}

@Component({
  template: '<p>Anmeldung</p>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class LoginOutlet {}

@Component({
  template: '<p>Dialog</p>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class SomeDialog {}

/** Fills its title parameter while it renders, as S10 does with its step number. */
@Component({
  template: '<h1>Kriterien zusammenstellen</h1>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class PageWithTitleParams {
  constructor() {
    const strategy = inject(PageTitleStrategy);
    effect(() => strategy.setPageTitleState({ params: { n: 2 } }));
  }
}

/** The shell's `main#main` with the primary outlet, plus the legacy login outlet. */
@Component({
  imports: [RouterOutlet],
  template: `
    <a href="#main">Zum Inhalt springen</a>
    <main id="main" tabindex="-1"><router-outlet /></main>
    <router-outlet name="login" />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class Shell {}

const ROUTES: Routes = [
  { path: 'home', component: PageWithoutHeading, data: { [PAGE_TITLE_KEY]: 'pageTitle.home' } },
  { path: 'impressum', component: PageWithHeading, data: { [PAGE_TITLE_KEY]: 'pageTitle.legal.imprint' } },
  { path: 'settings', component: PageWithoutHeading, data: { [PAGE_TITLE_KEY]: 'pageTitle.settings' } },
  {
    path: 'konto',
    data: { [PAGE_TITLE_KEY]: 'pageTitle.settings' },
    children: [{ path: '', component: PageWithoutHeading, data: { [PAGE_TITLE_KEY]: 'pageTitle.account' } }],
  },
  { path: 'legacy', component: PageWithoutHeading },
  { path: 'kriterien', component: PageWithTitleParams, data: { [PAGE_TITLE_KEY]: 'pageTitle.criteria.step' } },
  { path: 'auth', component: LoginOutlet, outlet: 'login' },
];

describe('PageTitleStrategy', () => {
  let announced: string[];

  async function startOn(url: string) {
    document.title = 'ProcureAccess'; // the <title> of index.html
    announced = [];
    TestBed.configureTestingModule({
      providers: [
        provideRouter(ROUTES),
        provideTestTranslations(),
        { provide: TitleStrategy, useExisting: PageTitleStrategy },
      ],
    });
    vi.spyOn(TestBed.inject(A11yAnnouncerService), 'announcePageTitle').mockImplementation((title) => {
      announced.push(title);
      return Promise.resolve();
    });
    const fixture = TestBed.createComponent(Shell);
    await TestBed.inject(Router).navigateByUrl(url);
    return fixture;
  }

  /** Waits for the render after a navigation and for the translated title. */
  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
  }

  afterEach(() => {
    (document.activeElement as HTMLElement | null)?.blur();
  });

  it('PageWithKey_Title_IsPageThenAppName', async () => {
    const fixture = await startOn('/impressum');
    await settle(fixture);

    expect(TestBed.inject(Title).getTitle()).toBe('Impressum – ProcureAccess');
  });

  it('StartPage_Title_UsesHomeFrameWithAppNameFirst', async () => {
    const fixture = await startOn('/home');
    await settle(fixture);

    expect(document.title).toBe('ProcureAccess – Barrierefreie Software beschaffen');
  });

  it('NestedRoutes_Title_TakesTheDeepestKey', async () => {
    const fixture = await startOn('/konto');
    await settle(fixture);

    expect(document.title).toBe('Konto – ProcureAccess');
  });

  it('PageWithoutKey_Title_IsTheAppName', async () => {
    const fixture = await startOn('/impressum');
    await settle(fixture);
    await TestBed.inject(Router).navigateByUrl('/legacy');
    await settle(fixture);

    expect(document.title).toBe('ProcureAccess');
  });

  it('FirstPageLoad_NoFocusMoveAndNoAnnouncement', async () => {
    const fixture = await startOn('/impressum');
    await settle(fixture);

    expect(document.activeElement).toBe(document.body);
    expect(announced).toEqual([]);
  });

  it('RouteChange_FocusOnH1AndTitleAnnounced INV:A11Y-03', async () => {
    const fixture = await startOn('/settings');
    await settle(fixture);

    await TestBed.inject(Router).navigateByUrl('/impressum');
    await settle(fixture);

    const heading = screen.getByRole('heading', { level: 1, name: 'Impressum' });
    expect(document.activeElement).toBe(heading);
    expect(heading.getAttribute('tabindex')).toBe('-1');
    expect(document.title).toBe('Impressum – ProcureAccess');
    await vi.waitFor(() => expect(announced).toEqual(['Impressum – ProcureAccess']));
  });

  it('RouteChangeToPageWithoutH1_FocusOnMainAndTitleAnnounced INV:A11Y-03', async () => {
    const fixture = await startOn('/impressum');
    await settle(fixture);

    await TestBed.inject(Router).navigateByUrl('/settings');
    await settle(fixture);

    expect(document.activeElement).toBe(screen.getByRole('main'));
    await vi.waitFor(() => expect(announced).toEqual(['Einstellungen – ProcureAccess']));
  });

  it('LoginOutletOrQueryParams_TitleFocusAndAnnouncementUnchanged', async () => {
    const fixture = await startOn('/settings');
    await settle(fixture);

    await TestBed.inject(Router).navigateByUrl('/settings(login:auth)');
    await settle(fixture);
    await TestBed.inject(Router).navigateByUrl('/settings?tab=2');
    await settle(fixture);

    expect(document.title).toBe('Einstellungen – ProcureAccess');
    expect(document.activeElement).toBe(document.body);
    expect(announced).toEqual([]);
  });

  it('DialogOpened_TitleUnchanged', async () => {
    const fixture = await startOn('/settings');
    await settle(fixture);

    TestBed.inject(MatDialog).open(SomeDialog);
    await settle(fixture);

    expect(document.title).toBe('Einstellungen – ProcureAccess');
    TestBed.inject(MatDialog).closeAll();
  });

  it('LanguageChanged_TitleTranslatedAgainWithoutAnnouncement', async () => {
    const fixture = await startOn('/settings');
    await settle(fixture);
    const translate = TestBed.inject(TranslateService);
    translate.setTranslation('en', { shell: { pageTitle: '{{page}} | ProcureAccess' }, pageTitle: { settings: 'Settings' } });

    translate.use('en');
    await settle(fixture);

    expect(document.title).toBe('Settings | ProcureAccess');
    expect(announced).toEqual([]);
  });

  it('RouteChangeToPageThatFillsItsParams_AnnouncesTheFilledTitle INV:A11Y-03', async () => {
    const fixture = await startOn('/settings');
    await settle(fixture);

    await TestBed.inject(Router).navigateByUrl('/kriterien');
    await settle(fixture);

    expect(document.title).toBe('Kriterien zusammenstellen – Schritt 2 von 4 – ProcureAccess');
    await vi.waitFor(() =>
      expect(announced).toEqual(['Kriterien zusammenstellen – Schritt 2 von 4 – ProcureAccess']),
    );
  });

  it('PageTitleState_ParamsAndErrorPrefix_FollowWithoutAnnouncement', async () => {
    const fixture = await startOn('/kriterien');
    await settle(fixture);
    const strategy = TestBed.inject(PageTitleStrategy);

    strategy.setPageTitleState({ params: { n: 3 }, error: true });
    await settle(fixture);
    expect(document.title).toBe('Fehler: Kriterien zusammenstellen – Schritt 3 von 4 – ProcureAccess');

    strategy.setPageTitleState({ params: { n: 3 } });
    await settle(fixture);
    expect(document.title).toBe('Kriterien zusammenstellen – Schritt 3 von 4 – ProcureAccess');
    expect(announced).toEqual([]);
  });

  it('RouteChange_ClearsThePageTitleStateOfThePreviousPage', async () => {
    const fixture = await startOn('/impressum');
    await settle(fixture);
    TestBed.inject(PageTitleStrategy).setPageTitleState({ error: true });
    await settle(fixture);
    expect(document.title).toBe('Fehler: Impressum – ProcureAccess');

    await TestBed.inject(Router).navigateByUrl('/settings');
    await settle(fixture);

    expect(document.title).toBe('Einstellungen – ProcureAccess');
  });
});
