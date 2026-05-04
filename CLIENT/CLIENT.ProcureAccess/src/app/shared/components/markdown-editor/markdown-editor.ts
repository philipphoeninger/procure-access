import {
  AfterViewInit,
  Component,
  effect,
  ElementRef,
  EventEmitter,
  model,
  Output,
  ViewChild,
} from '@angular/core';

import EasyMDE from 'easymde';

@Component({
  selector: 'app-markdown-editor',
  standalone: true,
  templateUrl: './markdown-editor.html'
//   encapsulation: ViewEncapsulation.None
})
export class MarkdownEditor implements AfterViewInit {
  @ViewChild('editor') editorElement!: ElementRef;

  value = model('');
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
      autoDownloadFontAwesome: false,
      status: false
    });

    this.editor.codemirror.on('change', () => {
      this.valueChange.emit(this.editor.value());
    });
  }
}
