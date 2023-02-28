import { ApplicationRef, ComponentRef, createComponent, EnvironmentInjector, Injectable, Type } from '@angular/core';
import { ActionResponse, ModalOptions, ModalRef } from '@models';
import { ModalComponent } from '../../components/modal/modal.component';
import { ModalInjector } from '@services/modal/modal-injector';
import { EXTRA_OBJECTS_TO_INJECT } from '../../components/modal/extra-objects-token';
import { filter, MonoTypeOperatorFunction, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ModalService {

  modalRef?: ModalRef<any>;

  constructor(
    private _appRef: ApplicationRef,
    private _injector: EnvironmentInjector
  ) {
  }

  public open<T>(componentType: Type<any>, options: ModalOptions<T>, extraObjectsToInject: Map<any, any> = new Map<any, any>()): ModalRef<T> {
    const modalRef = this.appendModalComponentToMainView<T>(options, extraObjectsToInject);

    const modalComponent = modalRef.modalComponentRef.instance;
    modalComponent.childComponentType = componentType;
    this.modalRef = modalRef;

    return modalRef;
  }

  public handleResponse(): MonoTypeOperatorFunction<ActionResponse> {
    return source => {
      return source.pipe(
        tap(() => this.modalRef?.stopLoading()),
        filter(res => res.Status),
        tap(() => {
          if (this.modalRef) {
            this.removeModalComponentFromMdtView(this.modalRef.modalComponentRef)
          }
        })
      )
    }
  }

  private appendModalComponentToMainView<T>(options: ModalOptions, extraObjectsToInject: Map<any, any>): ModalRef<T> {
    const modalRef = new ModalRef<T>(options);

    extraObjectsToInject.set(ModalRef, modalRef);
    extraObjectsToInject.set(EXTRA_OBJECTS_TO_INJECT, extraObjectsToInject);

    const componentRef = createComponent(ModalComponent, {
      environmentInjector: this._injector,
      elementInjector: new ModalInjector(this._injector, extraObjectsToInject),
    });

    const mdtView = document.querySelector('#modal-container');
    if (!mdtView) {
      // @ts-ignore
      return null;
    }

    mdtView.appendChild(componentRef.location.nativeElement);
    this._appRef.attachView(componentRef.hostView);

    modalRef.modalComponentRef = componentRef;

    modalRef.options.valueResolver?.updating
      .subscribe(updating => {
        if (updating) {
          modalRef.startLoading('Updating...');
        } else {
          modalRef.stopLoading();
        }
      });

    modalRef.result.subscribe(res => {
      if (!res.success) {
        this.removeModalComponentFromMdtView(modalRef.modalComponentRef);
      }
    });

    return modalRef;
  }

  private removeModalComponentFromMdtView(componentRef: ComponentRef<ModalComponent>) {
    this._appRef.detachView(componentRef.hostView);
    componentRef.destroy();
  }
}
