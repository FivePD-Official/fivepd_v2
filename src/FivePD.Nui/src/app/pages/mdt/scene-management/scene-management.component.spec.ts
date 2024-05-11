import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SceneManagementComponent } from './scene-management.component';
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";

describe('SceneManagementComponent', () => {
  let component: SceneManagementComponent;
  let fixture: ComponentFixture<SceneManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        SceneManagementComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SceneManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
