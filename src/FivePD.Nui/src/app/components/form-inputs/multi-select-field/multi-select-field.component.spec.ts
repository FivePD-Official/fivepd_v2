import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MultiSelectFieldComponent } from './multi-select-field.component';

describe('MultiSelectFieldComponent', () => {
  let component: MultiSelectFieldComponent;
  let fixture: ComponentFixture<MultiSelectFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MultiSelectFieldComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MultiSelectFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
