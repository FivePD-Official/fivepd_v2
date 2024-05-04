import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InformationComponent } from './information.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('InformationComponent', () => {
  let component: InformationComponent;
  let fixture: ComponentFixture<InformationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        InformationComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InformationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
