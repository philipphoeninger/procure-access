// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { CandidateTab } from '../../../models/candidates';
import { candidateTabTitle } from '../candidate-detail';

/** The route data key naming the tab a placeholder stands for. */
export const CANDIDATE_TAB_DATA = 'candidateTab';

/**
 * The tabs Prüfnachweis (S34a, S6-17) and Einschätzung nach Kriterien (S34b, S6-19) until their
 * tasks build them: the tab is a page of its own with its title (R-CAN-03), and shows nothing yet —
 * no text of its own, since the UX copy has none for a tab that is not built.
 */
@Component({
  selector: 'pa-candidate-tab-placeholder',
  template: '',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateTabPlaceholder {
  constructor() {
    candidateTabTitle(inject(ActivatedRoute).snapshot.data[CANDIDATE_TAB_DATA] as CandidateTab);
  }
}
