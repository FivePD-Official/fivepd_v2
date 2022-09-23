import {
  AfterViewChecked,
  AfterViewInit,
  ApplicationRef, ChangeDetectorRef,
  Component,
  ComponentRef, ContentChildren,
  createComponent,
  ElementRef,
  EnvironmentInjector, Inject,
  OnInit, QueryList, Renderer2,
  Type,
  ViewChild, ViewChildren
} from '@angular/core';
import { ModalOptions, ModalRef } from '@models';
import { ModalInjector } from '@services/modal/modal-injector';
import { EXTRA_OBJECTS_TO_INJECT } from './extra-objects-token';

type RemoveFocusTrap = () => void;

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.sass']
})
export class ModalComponent implements OnInit, AfterViewInit, AfterViewChecked {

  componentRef!: ComponentRef<any>;
  childComponentType!: Type<any>;

  title: ModalOptions['title'] = '';
  description?: ModalOptions['description'];

  fields?: ModalOptions['fields'];
  buttons?: ModalOptions['buttons'];
  columns?: ModalOptions['columns'];

  @ViewChild('modalContainer') modalContainer!: ElementRef;
  @ViewChild('modalContent') modalContent!: ElementRef;
  removeFocusTrap?: RemoveFocusTrap;

  constructor(
    public modalRef: ModalRef,
    private _injector: EnvironmentInjector,
    private _appRef: ApplicationRef,
    private _changeDetector: ChangeDetectorRef,
    @Inject(EXTRA_OBJECTS_TO_INJECT) public extraObjectsToInject: Map<any, any>,
  ) { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.loadChildComponent(this.childComponentType);
  }

  ngAfterViewChecked(): void {
    if (!this.removeFocusTrap) {
      this.removeFocusTrap = this.trapFocus();
    }
  }

  ngOnDestroy(): void {
    if (this.removeFocusTrap) {
      this.removeFocusTrap();
    }
    if (this.componentRef) {
      this.componentRef.destroy();
      this._appRef.detachView(this.componentRef.hostView);
    }
  }

  close(): void {
    this.modalRef.close({
      success: false,
      value: {}
    });
  }

  private loadChildComponent(componentType: Type<any>): void {
    this.extraObjectsToInject ??= new Map();

    this.componentRef = createComponent(componentType, {
      environmentInjector: this._injector,
      elementInjector: new ModalInjector(this._injector, this.extraObjectsToInject),
    });
    this.modalContent.nativeElement.appendChild(this.componentRef.location.nativeElement);
    this._appRef.attachView(this.componentRef.hostView);
  }

  // TODO: Fix. The query results are empty after appending the component element.
  private trapFocus(): RemoveFocusTrap {
    const focusableEls: HTMLElement[] = [...this.modalContent.nativeElement.querySelectorAll('input, textarea, button:not([disabled])')];
    const firstFocusableEl = focusableEls[0];
    const lastFocusableEl = focusableEls[focusableEls.length - 1];
    let currentFocus: Element | null = null;

    if (firstFocusableEl?.nodeName !== 'BUTTON') {
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
          firstFocusableEl?.focus();
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
