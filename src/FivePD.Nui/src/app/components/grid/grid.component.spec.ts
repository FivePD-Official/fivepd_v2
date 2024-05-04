import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GridComponent } from './grid.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('GridComponent<T>', () => {
  let component: GridComponent<any>;
  let fixture: ComponentFixture<GridComponent<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        GridComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(GridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
