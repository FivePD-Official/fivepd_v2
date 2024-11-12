import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GameNotificationWrapperComponent } from './game-notification-wrapper.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('GameNotificationWrapperComponent', () => {
  let component: GameNotificationWrapperComponent;
  let fixture: ComponentFixture<GameNotificationWrapperComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        GameNotificationWrapperComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withStore()
          .withLocalizationService()
          .build()
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GameNotificationWrapperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
