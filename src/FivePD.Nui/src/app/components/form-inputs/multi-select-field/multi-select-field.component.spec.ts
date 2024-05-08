import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MultiSelectFieldComponent } from './multi-select-field.component';
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";
import { FormGroup } from "@angular/forms";
import { MultiSelectField } from "@models";

describe('MultiSelectFieldComponent', () => {
  let component: MultiSelectFieldComponent;
  let fixture: ComponentFixture<MultiSelectFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MultiSelectFieldComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MultiSelectFieldComponent);
    component = fixture.componentInstance;

    component.form = new FormGroup([]);
    component.field = new MultiSelectField({
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
