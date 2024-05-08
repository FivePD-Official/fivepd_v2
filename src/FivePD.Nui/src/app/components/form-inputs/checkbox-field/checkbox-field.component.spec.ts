import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CheckboxFieldComponent } from './checkbox-field.component';
import { FormGroup } from "@angular/forms";
import { CheckboxField } from "@models";
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";

describe('CheckboxFieldComponent', () => {
  let component: CheckboxFieldComponent;
  let fixture: ComponentFixture<CheckboxFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CheckboxFieldComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CheckboxFieldComponent);
    component = fixture.componentInstance;

    component.form = new FormGroup([]);
    component.field = new CheckboxField({});

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
