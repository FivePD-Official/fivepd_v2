import { Injectable } from '@angular/core';
import { ModalConfirmOptions, ModalOptions, ModalRef } from '@models';
import { ModalFormComponent } from '../../../components/modal/modal-form/modal-form.component';
import { ModalService } from '@services/modals/modal/modal.service';

@Injectable({
  providedIn: 'root'
})
export class ModalConfirmService {

  constructor(
    private readonly modalService: ModalService,
  ) { }

  open<T>(options: ModalConfirmOptions<T>, extraObjectsToInject: Map<any, any> = new Map<any, any>()): ModalRef<T> {
    return this.modalService.open<T>(ModalFormComponent, {...options} as ModalOptions, extraObjectsToInject);
  }
}
