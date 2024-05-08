import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalEntityViewComponent } from './modal-entity-view.component';
import { ModalRef } from "@services/modals/modal/modal-ref";
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";
import { ATTRIBUTES, ENTITY } from "./entity-token";

describe('ModalEntityViewComponent', () => {
  let component: ModalEntityViewComponent;
  let fixture: ComponentFixture<ModalEntityViewComponent>;

  const modalRef = new ModalRef({
    title: 'Title',
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ModalEntityViewComponent,
        TestHelperModule.builder()
          .withWindowEventListener()
          .withStore()
          .build()
      ],
      providers: [
        {provide: ModalRef, useValue: modalRef},
        {provide: ENTITY, useValue: {}},
        {provide: ATTRIBUTES, useValue: []},
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ModalEntityViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
