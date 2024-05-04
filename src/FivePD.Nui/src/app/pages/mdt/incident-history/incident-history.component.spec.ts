import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IncidentHistoryComponent } from './incident-history.component';
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";

describe('IncidentHistoryComponent', () => {
  let component: IncidentHistoryComponent;
  let fixture: ComponentFixture<IncidentHistoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        IncidentHistoryComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(IncidentHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
