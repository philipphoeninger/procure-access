// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import { afterNextRender, DestroyRef, inject, Injectable, Injector } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRouteSnapshot, PRIMARY_OUTLET, RouterStateSnapshot, TitleStrategy } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { A11yAnnouncerService } from './a11y-announcer.service';

/** Route data key: the `pageTitle.*` key of the page (UX copy §5.16). */
export const PAGE_TITLE_KEY = 'pageTitleKey';

const HOME_PAGE_TITLE = 'pageTitle.home';

/**
 * Page title, focus and announcement on a route change (R-SHL-04, A11Y-03, Handoff §3.4, §7).
 *
 * - `<title>` = `shell.pageTitle` with the `pageTitle.*` key of the deepest primary route that
 *   names one (`data[PAGE_TITLE_KEY]`), so the page comes first; the start page uses
 *   `shell.pageTitle.home`. A page without a key gets the app name, the `<title>` of
 *   `index.html`. The title follows a language change.
 * - When the primary path changes, the focus moves to the H1 in `main#main` (or to `main` on a
 *   page without an H1) and the title is announced. The first page load only sets the title, so
 *   the skip link stays the first stop of the Tab key. Dialogs, the legacy login outlet and
 *   query parameters leave title and focus alone.
 *
 * @spec docs/SPEC_v2.md R-SHL-04; docs/design_handoff_procureaccess_v1.md §3.4, §7
 * @invariants A11Y-03
 */
@Injectable({ providedIn: 'root' })
export class PageTitleStrategy extends TitleStrategy {
  private readonly title = inject(Title);
  private readonly translate = inject(TranslateService);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly document = inject(DOCUMENT);
  private readonly injector = inject(Injector);
  /** The `<title>` of `index.html` (the app name): the title of a page without a key. */
  private readonly appName = this.title.getTitle();

  private pageKey: string | undefined;
  private primaryPath: string | undefined;
  /** Counts title updates, so a late translation never overwrites a newer title. */
  private updates = 0;

  constructor() {
    super();
    const subscription = this.translate.onLangChange.subscribe(() => void this.applyTitle());
    inject(DestroyRef).onDestroy(() => subscription.unsubscribe());
  }

  override updateTitle(snapshot: RouterStateSnapshot): void {
    const path = primaryPath(snapshot.root);
    if (path === this.primaryPath) return;
    const firstPage = this.primaryPath === undefined;
    this.primaryPath = path;
    this.pageKey = deepestPageTitleKey(snapshot.root);

    const title = this.applyTitle();
    if (firstPage) return;
    afterNextRender(
      () => {
        this.focusMainHeading();
        void title.then((text) => (text ? this.announcer.announcePageTitle(text) : undefined));
      },
      { injector: this.injector },
    );
  }

  /**
   * Sets `<title>` for the current page. Resolves to the title, or to `undefined` if a newer
   * update won or no language is active yet (then the language change sets it).
   */
  private async applyTitle(): Promise<string | undefined> {
    const update = ++this.updates;
    const pageKey = this.pageKey;
    if (pageKey && !this.translate.getCurrentLang()) return undefined;
    let text = this.appName;
    if (pageKey) {
      const page = (await firstValueFrom(this.translate.get(pageKey))) as string;
      const frame = pageKey === HOME_PAGE_TITLE ? 'shell.pageTitle.home' : 'shell.pageTitle';
      text = (await firstValueFrom(this.translate.get(frame, { page }))) as string;
    }
    if (update !== this.updates) return undefined;
    this.title.setTitle(text);
    return text;
  }

  private focusMainHeading(): void {
    const main = this.document.getElementById('main');
    if (!main) return; // the legacy login outlet replaces the shell
    const target = main.querySelector('h1') ?? main;
    if (!target.hasAttribute('tabindex')) target.setAttribute('tabindex', '-1');
    target.focus();
  }
}

/** The path of the primary outlet only: `/home(login:auth)` and `/home?x=1` are both `home`. */
function primaryPath(root: ActivatedRouteSnapshot): string {
  const segments: string[] = [];
  for (let route: ActivatedRouteSnapshot | undefined = root; route; route = primaryChild(route)) {
    segments.push(...route.url.map((segment) => segment.path));
  }
  return segments.join('/');
}

function deepestPageTitleKey(root: ActivatedRouteSnapshot): string | undefined {
  let key: string | undefined;
  for (let route: ActivatedRouteSnapshot | undefined = root; route; route = primaryChild(route)) {
    key = (route.data[PAGE_TITLE_KEY] as string | undefined) ?? key;
  }
  return key;
}

function primaryChild(route: ActivatedRouteSnapshot): ActivatedRouteSnapshot | undefined {
  return route.children.find((child) => child.outlet === PRIMARY_OUTLET);
}
