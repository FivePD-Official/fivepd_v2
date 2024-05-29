import { Injectable } from '@angular/core';
import { ModalService } from '@services/modals/modal/modal.service';
import { ModalFormOptions, ModalOptions } from '@models';
import { ModalFormComponent } from '../../../components/modal/modal-form/modal-form.component';
import { ModalRef } from '@services/modals/modal/modal-ref';

@Injectable({
  providedIn: 'root'
})
export class ModalFormService {

  constructor(
    private readonly modalService: ModalService,
  ) { }

  open<T1, T2 = {}>(options: ModalFormOptions<T1, T2>, extraObjectsToInject: Map<any, any> = new Map<any, any>()): ModalRef<T1> {
    return this.modalService.open<T1>(ModalFormComponent, {...options} as ModalOptions, extraObjectsToInject);
  }
}
