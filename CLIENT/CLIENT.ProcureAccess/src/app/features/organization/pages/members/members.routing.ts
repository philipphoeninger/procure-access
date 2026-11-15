// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { ResolveFn, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { authGuard } from '@app/core/guards/auth.guard';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationApiService } from '../../services/api/organization-api.service';
import { MembersPage } from './members-page';
import { failedView, MembersPageLoad } from './members-page-load';

/**
 * Loads the members before S21 shows (SPEC §8.9 `GET …/members`), so the route change titles,
 * focuses and announces „Mitglieder von {{org}}“ with the name already known (A11Y-03). A 404 —
 * no such organisation or no member of it, which the server does not tell apart (TEN-05) — and a
 * malformed id (400) show „Seite nicht gefunden“ in place; any other failure the S80 server text.
 * The loading bar shows meanwhile.
 */
export const membersPageResolver: ResolveFn<string> = async (route) => {
  const load = inject(MembersPageLoad);
  const api = inject(OrganizationApiService);
  const store = inject(ProcureAccessStore);
  store.incrementLoadingCount();
  try {
    const data = await api.members(route.paramMap.get('id') ?? '');
    load.view = { kind: 'members', data };
    return 'pageTitle.org.members';
  } catch (error: unknown) {
    load.view = failedView(error);
    return load.view.kind === 'notFound' ? 'pageTitle.notFound' : 'pageTitle.server';
  } finally {
    store.decrementLoadingCount();
  }
};

/** S21 at `/organisation/:id/mitglieder`, lazy; signed in only (owner decision S5-16 6a). */
export const MEMBERS_ROUTES: Routes = [
  {
    path: '',
    component: MembersPage,
    canActivate: [authGuard],
    providers: [MembersPageLoad],
    resolve: { [PAGE_TITLE_KEY]: membersPageResolver },
  },
];
