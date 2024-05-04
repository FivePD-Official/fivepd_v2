import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Tab, TabsComponent } from './tabs.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('TabsComponent', () => {
  let component: TabsComponent;
  let fixture: ComponentFixture<TabsComponent>;

  const tabs: Tab[] = []

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TabsComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TabsComponent);
    component = fixture.componentInstance;

    component.tabs = tabs;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
