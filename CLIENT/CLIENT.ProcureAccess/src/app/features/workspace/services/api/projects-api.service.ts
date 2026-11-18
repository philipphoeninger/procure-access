// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

import { API_URL } from '@app/app.config';
import { CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

import { Project, ProjectChange, ProjectCriteriaList, ProjectSummary } from '../../models/projects';

/**
 * Client of the project endpoints of the Workspace module (ARCHITECTURE §7.2; S6-11). Every request
 * is marked {@link CALLER_SHOWS_FAILURES}: S30 shows a failed list as the S80 server text in place,
 * S31 every failure of creating in its error notice, S32 a failed read as S80 in place and the
 * dialog „Projekt bearbeiten“ a failed change in its error notice (S6-14); a failed assignment of a
 * list after creating the project shows the general notice on S32 (owner decision S6-12 D5 (b)).
 */
@Injectable({ providedIn: 'root' })
export class ProjectsApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  /** Every project of the organisation, also the archived ones, the most recently changed first (R-PRJ-01). */
  list(organizationId: string): Promise<ProjectSummary[]> {
    return lastValueFrom(
      this.http.get<ProjectSummary[]>(`${this.apiUrl}/v1/organizations/${encodeURIComponent(organizationId)}/projects`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /** Creates a project of the organisation with the status „Laufend“ (R-PRJ-02); Editor or Owner, confirmed address. */
  create(organizationId: string, name: string): Promise<Project> {
    return lastValueFrom(
      this.http.post<Project>(
        `${this.apiUrl}/v1/organizations/${encodeURIComponent(organizationId)}/projects`,
        { name },
        { context: new HttpContext().set(CALLER_SHOWS_FAILURES, true) },
      ),
    );
  }

  /** A project of an organisation the user belongs to; 404 for any other (TEN-01). */
  get(id: string): Promise<Project> {
    return lastValueFrom(
      this.http.get<Project>(`${this.apiUrl}/v1/projects/${encodeURIComponent(id)}`, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /**
   * Changes the name, the status or both (R-PRJ-05; `PATCH /api/v1/projects/{id}`); Editor or Owner,
   * confirmed address. An archived project changes only its status (409 `project-archived`).
   */
  change(id: string, change: ProjectChange): Promise<Project> {
    return lastValueFrom(
      this.http.patch<Project>(`${this.apiUrl}/v1/projects/${encodeURIComponent(id)}`, change, {
        context: new HttpContext().set(CALLER_SHOWS_FAILURES, true),
      }),
    );
  }

  /**
   * Gives the project its own copy of an organisation list, replacing a copy it had (R-PRJ-02,
   * R-PRJ-04; `PUT /api/v1/projects/{id}/criteria-configuration`, S6-12); Editor or Owner.
   */
  assignCriteriaList(projectId: string, sourceConfigurationId: string): Promise<ProjectCriteriaList> {
    return lastValueFrom(
      this.http.put<ProjectCriteriaList>(
        `${this.apiUrl}/v1/projects/${encodeURIComponent(projectId)}/criteria-configuration`,
        { sourceConfigurationId },
        { context: new HttpContext().set(CALLER_SHOWS_FAILURES, true) },
      ),
    );
  }
}
