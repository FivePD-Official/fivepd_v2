import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateDepartmentModalComponent } from './create-department-modal.component';

describe('CreateDepartmentModelComponent', () => {
  let component: CreateDepartmentModalComponent;
  let fixture: ComponentFixture<CreateDepartmentModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateDepartmentModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateDepartmentModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
