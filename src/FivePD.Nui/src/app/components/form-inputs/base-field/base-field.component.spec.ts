import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BaseFieldComponent } from './base-field.component';
import { FormGroup } from "@angular/forms";
import { TextField } from "@models";

describe('BaseFieldComponent', () => {
  let component: BaseFieldComponent;
  let fixture: ComponentFixture<BaseFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BaseFieldComponent,
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BaseFieldComponent);
    component = fixture.componentInstance;

    component.form = new FormGroup([]);
    component.field = new TextField({});

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
