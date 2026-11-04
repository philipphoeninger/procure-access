// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  linkedSignal,
  signal,
  viewChild,
} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { TranslatePipe } from '@ngx-translate/core';
import { Footer } from '@layout/footer/footer';
import { Header } from '@layout/header/header';
import { MainNav } from '@layout/main-nav/main-nav';
import { LoadingBar } from '@layout/loading-bar/loading-bar';
import { injectWideLayout } from '@layout/shell/wide-layout';
import { Notice } from '@app/shared/components/notice/notice';
import { ConnectionStatus } from './core/errors/system-errors';

/**
 * App shell: skip link, header, main, footer (R-SHL-01, Handoff §2.1, §7) and the compact menu;
 * the loading bar and the offline notice at the top of `main` (R-SHL-08, R-SHL-09, Handoff §6.2, §6.5).
 * The legacy login outlet still replaces the whole shell while it is open.
 *
 * @spec docs/design_handoff_procureaccess_v1.md §2.1, §3.1, §3.2, §6.2, §6.5, §7
 */
@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    MatButtonModule,
    MatSidenavModule,
    TranslatePipe,
    Header,
    MainNav,
    Footer,
    LoadingBar,
    Notice,
  ],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './app.scss',
})
export class App {
  protected readonly offline = inject(ConnectionStatus).offline;
  protected readonly showShell = signal(true);
  protected readonly wide = injectWideLayout();
  /** The compact menu; it closes whenever the layout switches. */
  protected readonly menuOpen = linkedSignal({ source: this.wide, computation: () => false });

  private readonly main = viewChild.required<ElementRef<HTMLElement>>('main');

  /** Skip link: focus `main` without changing the URL (the router would see `#main`). */
  protected skipToMain(event: Event): void {
    event.preventDefault();
    this.main().nativeElement.focus();
  }
}
