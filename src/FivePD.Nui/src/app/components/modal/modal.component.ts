import {
  AfterViewChecked,
  AfterViewInit,
  ApplicationRef,
  ChangeDetectorRef,
  Component,
  ComponentRef,
  createComponent,
  ElementRef,
  EnvironmentInjector,
  Inject,
  OnInit,
  Type,
  ViewChild
} from '@angular/core';
import { FormSubmitEvent, ModalOptions, ModalRef } from '@models';
import { ModalInjector } from '@services/modal/modal-injector';
import { EXTRA_OBJECTS_TO_INJECT } from './extra-objects-token';
import { ModalFormComponent } from './modal-form/modal-form.component';
import { DynamicFormDirective } from '../dynamic-form/dynamic-form.directive';
import { DynamicFormComponent } from '../dynamic-form/dynamic-form.component';

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
  dynamicFormComponent?: DynamicFormComponent;
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

  submit(): void {
    if (this.childComponentType === ModalFormComponent) {
      this.dynamicFormComponent?.internalOnSubmit();
    } else {
      this.modalRef.close({
        success: true,
        value: {},
      });
    }
  }

  private loadChildComponent(componentType: Type<any>): void {
    this.extraObjectsToInject ??= new Map();

    this.componentRef = createComponent(componentType, {
      environmentInjector: this._injector,
      elementInjector: new ModalInjector(this._injector, this.extraObjectsToInject),
    });
    this.modalContent.nativeElement.appendChild(this.componentRef.location.nativeElement);
    this._appRef.attachView(this.componentRef.hostView);
    if (componentType === ModalFormComponent) {
      (this.componentRef.instance as unknown as ModalFormComponent).form.subscribe(form => {
        this.dynamicFormComponent = form;
      })
    }
    this.trapFocus();
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
          lastFocusableEl?.focus();
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
