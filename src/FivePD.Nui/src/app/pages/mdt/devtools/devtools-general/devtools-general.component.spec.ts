import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevtoolsGeneralComponent } from './devtools-general.component';

describe('DevtoolsViewComponent', () => {
  let component: DevtoolsGeneralComponent;
  let fixture: ComponentFixture<DevtoolsGeneralComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DevtoolsGeneralComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DevtoolsGeneralComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
