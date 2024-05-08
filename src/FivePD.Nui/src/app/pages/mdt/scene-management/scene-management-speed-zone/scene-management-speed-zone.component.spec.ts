import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SceneManagementSpeedZoneComponent } from './scene-management-speed-zone.component';

describe('SceneManagementSpeedZoneComponent', () => {
  let component: SceneManagementSpeedZoneComponent;
  let fixture: ComponentFixture<SceneManagementSpeedZoneComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SceneManagementSpeedZoneComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(SceneManagementSpeedZoneComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
