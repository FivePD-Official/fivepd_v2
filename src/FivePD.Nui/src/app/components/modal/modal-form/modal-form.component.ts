import { AfterViewInit, Component, OnInit } from '@angular/core';
import { FormSubmitEvent, ModalRef } from '@models';

@Component({
  selector: 'app-modal-form',
  templateUrl: './modal-form.component.html',
  styleUrls: ['./modal-form.component.sass']
})
export class ModalFormComponent implements OnInit {

  constructor(
    public modalRef: ModalRef,
  ) { }

  ngOnInit(): void {
  }

  submit(event?: FormSubmitEvent): void {
    this.modalRef.close({
      success: true,
      value: event?.value ?? {},
    });
  }

  close(): void {
    this.modalRef.close({
      success: false,
      value: {}
    });
  }

}
