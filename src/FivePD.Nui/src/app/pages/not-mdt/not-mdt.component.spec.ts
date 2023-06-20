import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotMdtComponent } from './not-mdt.component';

describe('NotMdtComponent', () => {
  let component: NotMdtComponent;
  let fixture: ComponentFixture<NotMdtComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [NotMdtComponent]
    });
    fixture = TestBed.createComponent(NotMdtComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
