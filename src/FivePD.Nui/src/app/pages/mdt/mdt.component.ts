import { Component, OnInit } from '@angular/core';
import { Modal, ModalService } from '@services/modal/modal.service';
import { Select } from '@ngxs/store';
import { AppState } from '@store/app.state';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-mdt',
  templateUrl: './mdt.component.html',
  styleUrls: ['./mdt.component.sass']
})
export class MdtComponent implements OnInit {

  @Select(AppState.isSidebarCollapsed) isSidebarCollapsed$!: Observable<boolean>;
  currentModal: Modal | null = null;

  constructor(
    private modalService: ModalService,
  ) { }

  ngOnInit(): void {
    this.modalService.onOpen().subscribe(modal => {
      this.currentModal = modal;
    });
  }

}
