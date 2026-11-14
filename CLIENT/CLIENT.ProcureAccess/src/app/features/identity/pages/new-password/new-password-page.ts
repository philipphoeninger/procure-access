// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  effect,
  ElementRef,
  inject,
  Injector,
  linkedSignal,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { map } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemFieldErrors, problemTypeName } from '@app/core/errors/problem-details';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { LOGIN_PATH, RESET_PASSWORD_PATH } from '../../models/login-navigation';
import { PASSWORD_RULE } from '../../models/password-rule';
import { AuthService } from '../../services/auth.service';
import { NEW_PASSWORD_VIEW_KEY, NewPasswordState, NewPasswordView } from './new-password.routing';

/** A required text field (the lint rule `unbound-method` refuses the static `Validators.required`). */
function required(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.length === 0 ? { required: true } : null;
}

/**
 * S03, the new-password step at `/passwort/neu` (SPEC R-AUTH-04; UX copy §5.2, U120–U123; Handoff
 * §5.3: no Figma frame, form card as in S02). The route's guard has taken the token out of the URL.
 *
 * With a token: H1 `auth.reset.title`, the card with „Neues Passwort“, the hint of the 🔧 minimum
 * and „Passwort anzeigen“, then „Passwort speichern“. Checked on submit (required, 🔧 minimum to
 * 128 characters, Handoff §6.4); the server checks the same first, so a refused password keeps the
 * link. Success replaces the form with `auth.reset.done.title` (focused), `auth.reset.done` and
 * „Zur Anmeldung“ (S01 over the start page). The server's refusal of the token shows „Link nicht
 * mehr gültig“ at the same address with page title, H1 focus and announcement as on a route change
 * (`PageTitleStrategy.changePageInPlace`, A11Y-03).
 *
 * Without a token (expired or used link, a reload): „Link nicht mehr gültig“ like S80, with „Neuen
 * Link anfordern“ to the request step.
 *
 * @spec docs/SPEC_v2.md R-AUTH-04; docs/design_handoff_procureaccess_v1.md §5.3, §6.4, §6.5
 * @invariants API-02, A11Y-03, A11Y-09
 */
@Component({
  selector: 'pa-new-password-page',
  imports: [ErrorSummary, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, ReactiveFormsModule, RouterLink, TranslatePipe],
  templateUrl: './new-password-page.html',
  styleUrl: '../reset-password-pages.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NewPasswordPage {
  private readonly authService = inject(AuthService);
  private readonly translate = inject(TranslateService);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly state = inject(NewPasswordState);
  private readonly injector = inject(Injector);

  /** The view the resolver put into the route data; a second link opened here brings a new one. */
  private readonly routeView = toSignal(
    inject(ActivatedRoute).data.pipe(map((data) => data[NEW_PASSWORD_VIEW_KEY] as NewPasswordView)),
    { requireSync: true },
  );
  protected readonly view = linkedSignal(() => this.routeView());

  protected readonly rule = inject(PASSWORD_RULE);
  protected readonly password = new FormControl('', {
    nonNullable: true,
    validators: [required, Validators.minLength(this.rule.minLength), Validators.maxLength(this.rule.maxLength)],
  });
  protected readonly busy = signal(false);
  protected readonly passwordShown = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  protected readonly passwordId = 'pa-new-password';
  protected readonly requestPath = `/${RESET_PASSWORD_PATH}`;
  /** S01 in the `login` outlet over the start page (the reset page has nothing left to show after it). */
  protected readonly loginLink = inject(Router).parseUrl(`/(login:${LOGIN_PATH})`);

  private readonly submitted = signal(false);
  private readonly doneHeading = viewChild<ElementRef<HTMLElement>>('doneHeading');

  /** Field errors show after the first submit (Handoff §6.4), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  constructor() {
    // A second link opened on the page arrives at the same address, which the router's title update
    // ignores: start the form afresh and give the new view its title, H1 focus and announcement.
    // The resolver hands over the same object while nothing changed (e.g. S01 opening over the page).
    let first = true;
    effect(() => {
      const view = this.routeView();
      if (first) {
        first = false;
        return;
      }
      untracked(() => {
        this.password.reset('');
        this.submitted.set(false);
        this.summary.set([]);
        this.pageTitle.changePageInPlace(view.kind === 'invalid' ? 'pageTitle.reset.invalid' : 'pageTitle.reset');
      });
    });
  }

  protected togglePassword(): void {
    this.passwordShown.update((shown) => !shown);
  }

  /** The message at the field, also the line of the error summary (E12). */
  protected passwordMessage(): TextKey | null {
    const current = this.password.value.length;
    if (this.password.hasError('server')) return { key: 'errors.field.server', params: { message: this.password.getError('server') } };
    if (this.password.hasError('required')) {
      return { key: 'errors.field.required', params: { field: this.translate.instant('auth.reset.newPassword') as string } };
    }
    if (this.password.hasError('minlength')) return { key: 'errors.field.minLength', params: { min: this.rule.minLength, current } };
    if (this.password.hasError('maxlength')) return { key: 'errors.field.maxLength', params: { max: this.rule.maxLength, current } };
    return null;
  }

  protected async submit(event: Event, token: string): Promise<void> {
    event.preventDefault();
    if (this.busy()) {
      return;
    }
    this.submitted.set(true);
    const message = this.passwordMessage();
    if (message) {
      this.showErrors([{ message, field: { id: this.passwordId, labelKey: 'auth.reset.newPassword' } }]);
      return;
    }
    this.busy.set(true);
    try {
      await this.authService.completePasswordReset(token, this.password.value);
    } catch (error: unknown) {
      this.busy.set(false);
      this.refused(error);
      return;
    }
    this.busy.set(false);
    this.showErrors([]);
    this.state.view = { kind: 'done' };
    this.view.set({ kind: 'done' });
    afterNextRender(() => this.doneHeading()?.nativeElement.focus(), { injector: this.injector });
  }

  private refused(error: unknown): void {
    const type = problemTypeName(error);
    if (type === 'validation' && 'token' in problemFieldErrors(error)) {
      // The link is used up or expired: „Link nicht mehr gültig“ at the same address, with its
      // title, H1 focus and announcement; the resolvers name it on the next navigation too.
      this.summary.set([]);
      this.state.view = { kind: 'invalid' };
      this.view.set({ kind: 'invalid' });
      this.pageTitle.changePageInPlace('pageTitle.reset.invalid');
      return;
    }
    if (type === 'antiforgery') {
      this.showErrors([{ message: { key: 'errors.antiforgery' } }]);
      return;
    }
    if (type === 'validation') {
      this.showErrors(
        applyServerErrors(error, (path) =>
          path === 'newPassword' ? { control: this.password, id: this.passwordId, labelKey: 'auth.reset.newPassword' } : undefined,
        ),
      );
    }
  }

  private showErrors(lines: ErrorSummaryItem[]): void {
    this.summary.set(lines);
    this.pageTitle.setPageTitleState({ error: lines.length > 0 });
  }
}
