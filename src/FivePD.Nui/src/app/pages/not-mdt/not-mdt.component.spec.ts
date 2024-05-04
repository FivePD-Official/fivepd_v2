import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotMdtComponent } from './not-mdt.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('NotMdtComponent', () => {
  let component: NotMdtComponent;
  let fixture: ComponentFixture<NotMdtComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        NotMdtComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .withWindowEventListener()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NotMdtComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
