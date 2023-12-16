import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CallsignEditorComponent } from './callsign-editor.component';

describe('UserCallsignEditorComponent', () => {
  let component: CallsignEditorComponent;
  let fixture: ComponentFixture<CallsignEditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CallsignEditorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CallsignEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
