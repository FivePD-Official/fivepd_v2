import { TestBed } from '@angular/core/testing';

import { ModalFormService } from './modal-form.service';

describe('ModalFormService', () => {
  let service: ModalFormService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ModalFormService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
