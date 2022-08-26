import { Component, Input, OnInit } from '@angular/core';
import { ModalOptions, ModalService } from '@services/modal/modal.service';
import { FormSubmitEvent } from '@models';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.sass']
})
export class ModalComponent implements OnInit {

  @Input() modal!: ModalOptions;

  constructor(
    public modalService: ModalService
  ) { }

  ngOnInit(): void {
  }

  onSubmit(submitEvent?: FormSubmitEvent): void {
    if (submitEvent?.value) {
      this.modalService.submit(submitEvent.value);
    } else {
      this.modalService.submit();
    }
  }

}
