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
    const parentRect = this.parent.getBoundingClientRect();
    const selectedItemRect = this.selectedItem.getBoundingClientRect();
    this.elementRef.nativeElement.style.width = `${parentRect.width}px`;
    this.elementRef.nativeElement.style.top = `${parentRect.top - selectedItemRect.height}px`;
  }
}
