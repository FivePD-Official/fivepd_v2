import {AfterViewInit, Component, ElementRef, ViewChild, inject} from '@angular/core';
import {CommonModule} from '@angular/common';
import {DebugService} from '@services/debug/debug.service';
import {ModalRef} from '@services/modals/modal/modal-ref';
import {map} from 'rxjs';

@Component({
  selector: 'app-debug-viewer',
  imports: [CommonModule],
  templateUrl: './debug-view.component.html'
})
export class DebugViewComponent implements AfterViewInit {
  private readonly _debugService = inject(DebugService);
  private readonly _modalRef = inject(ModalRef);

  protected readonly debugInfo$ = this._debugService.debugInfo().pipe(
    map(info => JSON.stringify(info, null, 2))
  );

  @ViewChild('ta') textarea!: ElementRef<HTMLTextAreaElement>;

  ngAfterViewInit() {
    this.debugInfo$.subscribe(() => {
      queueMicrotask(() => this.resize());
    });
  }

  private resize() {
    const el = this.textarea.nativeElement;
    el.style.height = 'auto';
    el.style.height = `${el.scrollHeight}px`;
  }

  // Copy text to clipboard
  copyText() {
    const el = this.textarea.nativeElement;
    el.select();
    navigator.clipboard.writeText(el.value)
      .catch(err => console.error('Failed to copy:', err));
  }
}
