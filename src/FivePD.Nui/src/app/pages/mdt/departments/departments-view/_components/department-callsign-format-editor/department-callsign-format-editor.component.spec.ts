import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DepartmentCallsignFormatEditorComponent } from './department-callsign-format-editor.component';

describe('DepartmentCallsignFormatModalComponent', () => {
  let component: DepartmentCallsignFormatEditorComponent;
  let fixture: ComponentFixture<DepartmentCallsignFormatEditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DepartmentCallsignFormatEditorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DepartmentCallsignFormatEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
