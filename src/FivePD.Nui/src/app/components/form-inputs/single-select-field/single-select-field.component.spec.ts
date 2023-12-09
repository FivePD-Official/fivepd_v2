import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SingleSelectFieldComponent } from './single-select-field.component';

describe('SingleSelectFieldComponent', () => {
  let component: SingleSelectFieldComponent;
  let fixture: ComponentFixture<SingleSelectFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SingleSelectFieldComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(SingleSelectFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
