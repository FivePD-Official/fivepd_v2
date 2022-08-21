import { Component, Input, OnInit } from '@angular/core';
import { Modal, ModalService } from '@services/modal/modal.service';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.sass']
})
export class ModalComponent implements OnInit {

  @Input() modal!: Modal;

  constructor(
    public modalService: ModalService
  ) { }

  ngOnInit(): void {
  }

  onSubmit(value?: object): void {
    if (value) {
      this.modalService.submit(value);
    } else {
      this.modalService.submit();
    }
  }

}
