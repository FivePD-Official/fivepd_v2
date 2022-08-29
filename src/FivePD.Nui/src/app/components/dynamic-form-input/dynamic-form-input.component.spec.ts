import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicFormInputComponent } from './dynamic-form-input.component';

describe('DynamicFormInputComponent', () => {
  let component: DynamicFormInputComponent;
  let fixture: ComponentFixture<DynamicFormInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DynamicFormInputComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DynamicFormInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
