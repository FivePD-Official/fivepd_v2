import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { Modal, ModalOptions, ModalService } from '@services/modal/modal.service';
import { FormSubmitEvent } from '@models';

type RemoveFocusTrap = () => void;

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.sass']
})
export class ModalComponent implements OnInit, AfterViewInit {

  @Input() modal!: Modal;

  @ViewChild('modalContainer') modalContainer!: ElementRef;
  removeFocusTrap?: RemoveFocusTrap;

  constructor(
    public modalService: ModalService
  ) { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.removeFocusTrap = this.trapFocus();
  }

  ngOnDestroy(): void {
    if (this.removeFocusTrap) {
      this.removeFocusTrap();
    }
  }

  onSubmit(submitEvent?: FormSubmitEvent): void {
    if (submitEvent?.value) {
      this.modalService.submit(submitEvent.value);
    } else {
      this.modalService.submit();
    }
  }

  private trapFocus(): RemoveFocusTrap {
    const focusableEls: HTMLElement[] = [...this.modalContainer.nativeElement.querySelectorAll('input, textarea, button:not([disabled])')];
    const firstFocusableEl = focusableEls[0];
    const lastFocusableEl = focusableEls[focusableEls.length - 1];
    let currentFocus: Element | null = null;

    if (firstFocusableEl.nodeName !== 'BUTTON') {
      firstFocusableEl?.focus();
      currentFocus = firstFocusableEl;
    }

    const handleFocus = (e: FocusEvent) => {
      if (!e?.target) {
        return;
      }
      e.preventDefault();

      if (focusableEls.includes(e.target as HTMLElement)) {
        currentFocus = e.target as HTMLElement;
      } else {
        if (currentFocus === firstFocusableEl) {
          lastFocusableEl.focus();
        } else {
          firstFocusableEl.focus();
        }
        currentFocus = document.activeElement;
      }
    };

    document.addEventListener('focus', handleFocus, true);

    return () => {
      document.removeEventListener('focus', handleFocus, true);
    };
  };

}
