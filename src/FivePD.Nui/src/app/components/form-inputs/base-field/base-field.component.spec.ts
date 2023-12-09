import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BaseFieldComponent } from './base-field.component';

describe('BaseFieldComponent', () => {
  let component: BaseFieldComponent;
  let fixture: ComponentFixture<BaseFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BaseFieldComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BaseFieldComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
