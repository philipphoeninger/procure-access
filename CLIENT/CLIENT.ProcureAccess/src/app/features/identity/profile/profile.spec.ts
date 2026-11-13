// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { ProposalDto } from '@app/features/proposal/models/proposal.dto';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Profile } from './profile';

// #137: the icon of a product proposal is decoration (Handoff §1.8); screen readers must not read
// its ligature name „box“ as part of the list item. Only the store and the permission check are
// replaced.
const productProposal = (id: number, name: string, finishedAt?: Date) =>
  ({ id, product: { name }, finishedAt, status: 0 }) as unknown as ProposalDto;

const open = [productProposal(1, 'Offenes Beispielprodukt')];
const closed = [productProposal(2, 'Geschlossenes Beispielprodukt', new Date(2026, 10, 1))];

async function renderProfile() {
  await render(Profile, {
    providers: [
      provideTestTranslations(),
      provideRouter([]),
      {
        provide: ProcureAccessStore,
        useValue: {
          user: signal(null),
          hasPermission: () => false,
          openProposals: signal(open),
          openProposalsCount: signal(open.length),
          closedProposals: signal(closed),
          closedProposalsCount: signal(closed.length),
        },
      },
    ],
  });
}

/** The text assistive technology gets: without content hidden by `aria-hidden`. */
function exposedText(element: HTMLElement): string {
  const copy = element.cloneNode(true) as HTMLElement;
  copy.querySelectorAll('[aria-hidden="true"]').forEach((hidden) => hidden.remove());
  return copy.textContent ?? '';
}

describe('Profile', () => {
  it('hides the product icon of open and closed proposals from assistive technology', async () => {
    await renderProfile();

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(2);
    for (const [item, name] of [
      [items[0], 'Offenes Beispielprodukt'],
      [items[1], 'Geschlossenes Beispielprodukt'],
    ] as const) {
      expect(exposedText(item)).toContain(name);
      expect(exposedText(item)).not.toContain('box');
      expect(item.querySelector('.material-symbols-outlined:not(mat-icon)')).toBeNull();
    }
  });
});
