import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CitizenDatabaseIndexComponent } from './citizen-database-index.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";

describe('CitizenDatabaseIndexComponent', () => {
  let component: CitizenDatabaseIndexComponent;
  let fixture: ComponentFixture<CitizenDatabaseIndexComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CitizenDatabaseIndexComponent,
        TestHelperModule.builder()
          .withStore()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CitizenDatabaseIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
