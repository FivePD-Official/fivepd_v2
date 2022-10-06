import { ApplicationRef, ComponentRef, createComponent, EnvironmentInjector, Injectable, Type } from '@angular/core';
import { ModalOptions, ModalRef } from '@models';
import { ModalComponent } from '../../components/modal/modal.component';
import { ModalInjector } from '@services/modal/modal-injector';
import { EXTRA_OBJECTS_TO_INJECT } from '../../components/modal/extra-objects-token';

@Injectable({
  providedIn: 'root'
})
export class ModalService {

  constructor(
    private _appRef: ApplicationRef,
    private _injector: EnvironmentInjector
  ) { }

  public open(componentType: Type<any>, options: ModalOptions, extraObjectsToInject: Map<any, any> = new Map<any, any>()): ModalRef {
    const modalRef = this.appendModalComponentToMainView(options, extraObjectsToInject);

    const modalComponent = modalRef.modalComponentRef.instance;
    modalComponent.childComponentType = componentType;
    modalComponent.title = options.title;
    modalComponent.description = options.description || '';
    modalComponent.fields = options.fields || [];
    modalComponent.buttons = options.buttons || {
      cancel: { theme: 'secondary', label: '' },
      submit: { theme: 'secondary', label: '' },
    };
    modalComponent.columns = options.columns || 1;

    return modalRef;
  }

  private appendModalComponentToMainView(options: ModalOptions, extraObjectsToInject: Map<any, any>): ModalRef {
    const modalRef = new ModalRef(options);

    extraObjectsToInject.set(ModalRef, modalRef);
    extraObjectsToInject.set(EXTRA_OBJECTS_TO_INJECT, extraObjectsToInject);

    const componentRef = createComponent(ModalComponent, {
      environmentInjector: this._injector,
      elementInjector: new ModalInjector(this._injector, extraObjectsToInject),
    });

    const mdtView = document.querySelector('.mdt-view');
    if (!mdtView) {
      // @ts-ignore
      // There might be some future plans to use modals for menus as well,
      // but for now we'll only use this in the MDT view.
      return null;
    }

    mdtView.appendChild(componentRef.location.nativeElement);
    this._appRef.attachView(componentRef.hostView);

    modalRef.modalComponentRef = componentRef;

    modalRef.result.subscribe(() => {
      this.removeModalComponentFromMdtView(modalRef.modalComponentRef);
    });

    return modalRef;
  }

  private removeModalComponentFromMdtView(componentRef: ComponentRef<ModalComponent>) {
    this._appRef.detachView(componentRef.hostView);
    componentRef.destroy();
  }
}
