// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  AfterViewInit,
  Component,
  effect,
  ElementRef,
  EventEmitter,
  Output,
  ViewChild,
  ChangeDetectionStrategy,
  input,
  linkedSignal
} from '@angular/core';

import EasyMDE from 'easymde';

@Component({
  selector: 'app-markdown-editor',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './markdown-editor.html'
//   encapsulation: ViewEncapsulation.None
})
export class MarkdownEditor implements AfterViewInit {
  @ViewChild('editor') editorElement!: ElementRef;

  valueInput = input('', {alias: 'value'});
  value = linkedSignal(this.valueInput);
//   darkModeOn = input(false);
  @Output() valueChange = new EventEmitter<string>();

  private editor!: EasyMDE;

  constructor() {
    // effect(() => {
    //     if (this.darkModeOn()) {
    //         this.editorElement?.nativeElement.addClass('dark-theme');
    //     } else {
    //         this.editorElement?.nativeElement.removeClass('dark-theme');
    //     }
    // });
    effect(() => {
      const current = this.value();

      if (
        this.editor &&
        current !== this.editor.value()
      ) {
        this.editor.value(current ?? '');
      }
    });
  }

  ngAfterViewInit(): void {
    this.editor = new EasyMDE({
      element: this.editorElement.nativeElement,
      initialValue: this.value(),
      spellChecker: false,
      // Must stay false: otherwise EasyMDE loads Font Awesome from a CDN (CLAUDE §8). The toolbar
      // glyphs come from Material Symbols (styles/main.scss).
      autoDownloadFontAwesome: false,
      status: false
    });

    this.editor.codemirror.on('change', () => {
      this.valueChange.emit(this.editor.value());
    });
  }
}
