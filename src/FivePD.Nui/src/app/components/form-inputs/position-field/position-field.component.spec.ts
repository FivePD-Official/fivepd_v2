import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PositionFieldComponent } from './position-field.component';
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";
import { FormGroup } from "@angular/forms";
import { PositionField, TextField } from "@models";

describe('PositionFieldComponent', () => {
  let component: PositionFieldComponent;
  let fixture: ComponentFixture<PositionFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PositionFieldComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PositionFieldComponent);
    component = fixture.componentInstance;

    component.form = new FormGroup([]);
    component.field = new PositionField({});

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
