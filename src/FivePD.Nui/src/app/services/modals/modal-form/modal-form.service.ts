import { Injectable } from '@angular/core';
import { ModalService } from '@services/modals/modal/modal.service';
import { ModalFormOptions, ModalRef } from '@models';
import { ModalFormComponent } from '../../../components/modal/modal-form/modal-form.component';

@Injectable({
  providedIn: 'root'
})
export class ModalFormService {

  constructor(
    private readonly modalService: ModalService,
  ) { }

  open<T>(options: ModalFormOptions<T>, extraObjectsToInject: Map<any, any> = new Map<any, any>()): ModalRef<T> {
    return this.modalService.open<T>(ModalFormComponent, {...options}, extraObjectsToInject);
  }
}
