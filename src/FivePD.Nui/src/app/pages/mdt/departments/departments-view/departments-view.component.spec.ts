import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DepartmentsViewComponent } from './departments-view.component';

describe('DepartmentsViewComponent', () => {
  let component: DepartmentsViewComponent;
  let fixture: ComponentFixture<DepartmentsViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [DepartmentsViewComponent]
})
    .compileComponents();

    fixture = TestBed.createComponent(DepartmentsViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
