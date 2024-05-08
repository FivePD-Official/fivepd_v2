import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MdtComponent } from './mdt.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('MdtComponent', () => {
  let component: MdtComponent;
  let fixture: ComponentFixture<MdtComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MdtComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MdtComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
