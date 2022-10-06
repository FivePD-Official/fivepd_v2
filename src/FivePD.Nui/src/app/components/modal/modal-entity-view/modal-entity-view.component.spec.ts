import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalEntityViewComponent } from './modal-entity-view.component';

describe('ModalEntityViewComponent', () => {
  let component: ModalEntityViewComponent;
  let fixture: ComponentFixture<ModalEntityViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ModalEntityViewComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModalEntityViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
