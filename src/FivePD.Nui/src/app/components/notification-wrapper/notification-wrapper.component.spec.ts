import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotificationWrapperComponent } from './notification-wrapper.component';

describe('NotificationWrapperComponent', () => {
  let component: NotificationWrapperComponent;
  let fixture: ComponentFixture<NotificationWrapperComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NotificationWrapperComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotificationWrapperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
