// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Injectable } from '@angular/core';

import { Project } from '../../models/projects';

/** What S32 shows: the project, or the S80 page in place. */
export type ProjectView = { kind: 'project'; project: Project } | { kind: 'notFound' } | { kind: 'server' };

/** The view the resolver loaded, handed to the page; one per route (route-level provider). */
@Injectable()
export class ProjectLoad {
  view: ProjectView = { kind: 'notFound' };
}
