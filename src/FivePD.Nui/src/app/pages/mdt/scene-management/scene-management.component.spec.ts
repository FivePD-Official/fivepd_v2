import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SceneManagementComponent } from './scene-management.component';

describe('SceneManagementComponent', () => {
  let component: SceneManagementComponent;
  let fixture: ComponentFixture<SceneManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SceneManagementComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(SceneManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
