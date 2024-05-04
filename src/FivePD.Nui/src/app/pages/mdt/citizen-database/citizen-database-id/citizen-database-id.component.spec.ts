import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CitizenDatabaseIdComponent } from './citizen-database-id.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";
import { RouterTestingModule } from "@angular/router/testing";

describe('CitizenDatabaseIdComponent', () => {
  let component: CitizenDatabaseIdComponent;
  let fixture: ComponentFixture<CitizenDatabaseIdComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CitizenDatabaseIdComponent,
        RouterTestingModule,
        TestHelperModule.builder()
          .withStore()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CitizenDatabaseIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
