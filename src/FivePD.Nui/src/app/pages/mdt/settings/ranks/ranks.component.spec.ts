import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RanksComponent } from './ranks.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";

describe('RanksComponent', () => {
  let component: RanksComponent;
  let fixture: ComponentFixture<RanksComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RanksComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .withLocalizationService()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RanksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
