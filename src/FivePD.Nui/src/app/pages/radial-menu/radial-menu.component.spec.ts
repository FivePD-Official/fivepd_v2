import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RadialMenuComponent } from './radial-menu.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";
import { RouterTestingModule } from "@angular/router/testing";

describe('RadialMenuComponent', () => {
  let component: RadialMenuComponent;
  let fixture: ComponentFixture<RadialMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RadialMenuComponent,
        RouterTestingModule,
        TestHelperModule.builder()
          .withWindowEventListener()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RadialMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
