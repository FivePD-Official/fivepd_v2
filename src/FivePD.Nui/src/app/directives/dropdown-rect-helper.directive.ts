import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[appDropdownRectHelperDirective]',
  standalone: true
})
export class DropdownRectHelperDirective {

  addedEventListener = false;

  constructor(
    private readonly elementRef: ElementRef
  ) {
    if (!this.addedEventListener) {
      this.setRect();
      this.parent.addEventListener('click', () => this.setRect());
      window.addEventListener('scroll', () => this.setRect(), true);
      this.addedEventListener = true;
    }
  }

  get parent() {
    return this.elementRef.nativeElement.parentElement;
  }

  get selectedItem() {
    return this.parent.querySelector('.dropdown-selected-item');
  }

  @HostListener('window:scroll')
  onScroll() {
    this.setRect();
  }

  private setRect(): void {
    const modal = document.querySelector('app-modal .main-modal-container');
    let modalRect;
    if (modal) {
      modalRect = modal.getBoundingClientRect();
    }

    const parentRect = this.parent.getBoundingClientRect();
    const selectedItemRect = this.selectedItem.getBoundingClientRect();
    this.elementRef.nativeElement.style.width = `${parentRect.width}px`;
    let top: number;
    /*
    when the lines below are uncommented: "&.dropdown--open .dropdown-items" => "@apply fixed" instead of "absolute"
    when these are used the modal cannot be scrolled when the dropdown is opened
     */

    if (modalRect) {
      top = parentRect.top - modalRect.top + selectedItemRect.height;
    } else {
      top = parentRect.top - selectedItemRect.height;
    }

    this.elementRef.nativeElement.style.top = `${top}px`;
  }
}
