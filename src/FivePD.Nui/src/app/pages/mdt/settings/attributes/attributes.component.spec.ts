import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AttributesComponent } from './attributes.component';

describe('AdminSettingsComponent', () => {
  let component: AttributesComponent;
  let fixture: ComponentFixture<AttributesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AttributesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AttributesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
