import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevtoolsViewComponent } from './devtools-view.component';

describe('DevtoolsViewComponent', () => {
  let component: DevtoolsViewComponent;
  let fixture: ComponentFixture<DevtoolsViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevtoolsViewComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DevtoolsViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
