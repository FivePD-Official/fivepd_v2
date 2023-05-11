import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevtoolsCalloutsComponent } from './devtools-callouts.component';

describe('DevtoolsCalloutsComponent', () => {
  let component: DevtoolsCalloutsComponent;
  let fixture: ComponentFixture<DevtoolsCalloutsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevtoolsCalloutsComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DevtoolsCalloutsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
