import { ApplicationRef, ComponentRef, createComponent, EnvironmentInjector, Injectable, Type } from '@angular/core';
import { ActionResponse, ModalFormOptions, ModalOptions } from '@models';
import { filter, MonoTypeOperatorFunction, tap } from 'rxjs';
import { EXTRA_OBJECTS_TO_INJECT } from '../../../components/modal/extra-objects-token';
import { ModalInjector } from '@services/modals/modal/modal-injector';
import { ModalComponent } from '../../../components/modal/modal.component';
import { ModalRef } from '@services/modals/modal/modal-ref';

@Injectable({
  providedIn: 'root'
})
export class ModalService {

  modalRef?: ModalRef;

  constructor(
    private _appRef: ApplicationRef,
    private _injector: EnvironmentInjector
  ) {
  }

  public open<T>(componentType: Type<any>, options: ModalOptions<T>, extraObjectsToInject: Map<any, any> = new Map<any, any>()): ModalRef<T> {
    const modifiedOptions = {...options};
    if (options.buttons?.submit) {
      modifiedOptions.buttons!.cancel = {variant: 'secondary', label: 'UI.ACTION.CANCEL'};
    }
    const modalRef = this.appendModalComponentToMainView<T>({
      ...modifiedOptions,
    }, extraObjectsToInject);

    const modalComponent = modalRef.modalComponentRef.instance;
    modalComponent.childComponentType = componentType;
    this.modalRef = modalRef;

    return modalRef;
  }

  public close<T>(): MonoTypeOperatorFunction<T> {
    return source => {
      return source.pipe(
        tap(() => this.modalRef?.stopLoading()),
        tap(() => {
          if (this.modalRef) {
            this.removeModalComponentFromMdtView(this.modalRef.modalComponentRef)
          }
        })
      )
    }
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

    const valueResolver = (modalRef.options as ModalFormOptions<T>).valueResolver;
    if (valueResolver) {
      valueResolver.updating
        .subscribe(updating => {
          if (updating) {
            modalRef.startLoading('Updating...');
          } else {
            modalRef.stopLoading();
          }
        });
    }

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
