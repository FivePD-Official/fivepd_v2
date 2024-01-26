import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MySettingsComponent } from './my-settings.component';

describe('UserSettingsComponent', () => {
  let component: MySettingsComponent;
  let fixture: ComponentFixture<MySettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MySettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MySettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
