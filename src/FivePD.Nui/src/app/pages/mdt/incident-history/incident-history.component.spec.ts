import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IncidentHistoryComponent } from './incident-history.component';

describe('IncidentHistoryComponent', () => {
  let component: IncidentHistoryComponent;
  let fixture: ComponentFixture<IncidentHistoryComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IncidentHistoryComponent]
    });
    fixture = TestBed.createComponent(IncidentHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
