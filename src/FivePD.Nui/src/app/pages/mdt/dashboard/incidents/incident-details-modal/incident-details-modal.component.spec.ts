import { ComponentFixture, TestBed } from '@angular/core/testing';

import { INCIDENT, IncidentDetailsModalComponent } from './incident-details-modal.component';
import { TestHelperModule } from "../../../../../../test-helpers/test-helper.module";
import { IncidentDTO } from "@models";
import { FIncidentType, FPriority } from "@enums";
import { ModalRef } from "@services/modals/modal/modal-ref";

const incident: IncidentDTO = {
  Id: '12345',
  Type: FIncidentType.Callout,
  Priority: FPriority.Code1,
  Title: 'Sample Incident',
  Description: 'Description of the sample incident',
  Address: '123 Sample Street',
  Units: ['Unit1', 'Unit2'],
};

describe('IncidentDetailsModalComponent', () => {
  let component: IncidentDetailsModalComponent;
  let fixture: ComponentFixture<IncidentDetailsModalComponent>;

  const modalRef = new ModalRef({
    title: 'Title',
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        IncidentDetailsModalComponent,
        TestHelperModule.builder()
          .withStore()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ],
      providers: [
        {provide: INCIDENT, useValue: incident},
        {provide: ModalRef, useValue: modalRef}
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(IncidentDetailsModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
