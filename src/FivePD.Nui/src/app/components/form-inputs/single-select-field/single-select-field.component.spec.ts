import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SingleSelectFieldComponent } from './single-select-field.component';
import { FormGroup } from "@angular/forms";
import { SingleSelectField } from "@models";
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";

describe('SingleSelectFieldComponent', () => {
  let component: SingleSelectFieldComponent;
  let fixture: ComponentFixture<SingleSelectFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        SingleSelectFieldComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SingleSelectFieldComponent);
    component = fixture.componentInstance;

    component.form = new FormGroup([]);
    component.field = new SingleSelectField({
      required: false,
      defaultItemLabel: "",
      items: [],
      size: 'small',
      variant: 'secondary'
    });

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
