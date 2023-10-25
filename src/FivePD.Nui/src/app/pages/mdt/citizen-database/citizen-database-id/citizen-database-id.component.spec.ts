import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CitizenDatabaseIdComponent } from './citizen-database-id.component';

describe('CitizenDatabaseIdComponent', () => {
  let component: CitizenDatabaseIdComponent;
  let fixture: ComponentFixture<CitizenDatabaseIdComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [CitizenDatabaseIdComponent]
});
    fixture = TestBed.createComponent(CitizenDatabaseIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
