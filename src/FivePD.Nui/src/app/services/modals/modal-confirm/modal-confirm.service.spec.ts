import { TestBed } from '@angular/core/testing';

import { ModalConfirmService } from './modal-confirm.service';

describe('ModalConfirmService', () => {
  let service: ModalConfirmService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ModalConfirmService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
