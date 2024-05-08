import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DispatchComponent } from './dispatch.component';
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";

describe('DispatchComponent', () => {
  let component: DispatchComponent;
  let fixture: ComponentFixture<DispatchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        DispatchComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DispatchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
