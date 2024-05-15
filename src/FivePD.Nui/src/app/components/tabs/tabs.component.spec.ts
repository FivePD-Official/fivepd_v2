import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Tab, TabsComponent } from './tabs.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";
import { FixtureHelper } from "../../../test-helpers/fixture-helper";
import { ActivatedRoute, Router } from "@angular/router";
import { TestHelper } from "../../../test-helpers/test-helper";
import SpyInstance = jest.SpyInstance;
import { RouterTestingModule } from "@angular/router/testing";
import { DummyComponent } from "../../../test-helpers/dummy-component";

describe('TabsComponent', () => {
  let component: TabsComponent;
  let fixture: ComponentFixture<TabsComponent>;
  let fixtureHelper: FixtureHelper;
  let router: Router;

  const tabs: Tab[] = []

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TabsComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build(),
        RouterTestingModule.withRoutes([
          {
            path: 'a',
            component: DummyComponent
          },
          {
            path: 'b',
            component: DummyComponent
          }
        ])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TabsComponent);
    component = fixture.componentInstance;
    fixtureHelper = new FixtureHelper(fixture);
    router = TestBed.inject(Router);

    component.tabs = tabs;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('when component.tabs is empty', () => {
    beforeEach(() => {
      component.tabs = [];
      fixture.detectChanges();
    });

    it('should NOT display any tab elements', () => {
      expect(fixtureHelper.findElements('[data-testid^="tab"]').length).toBe(0);
    });
  });

  describe('when component.tabs is NOT empty', () => {
    beforeEach(() => {
      component.tabs = [
        {
          label: 'Tab 1',
          routerLink: '/route-1'
        },
        {
          label: 'Tab 2',
          routerLink: '/route-2'
        },
      ];
      fixture.detectChanges();
    });

    it('should display the correct number of tab elements', () => {
      expect(fixtureHelper.findElements('[data-testid^="tab"]').length).toBe(2);
    });

    it('tab 1 should contain the correct label', () => {
      expect(fixtureHelper.findElement('[data-testid="tab - Tab 1"]').element.textContent).toBe('translated: Tab 1');
    });

    it('tab 2 should contain the correct label', () => {
      expect(fixtureHelper.findElement('[data-testid="tab - Tab 2"]').element.textContent).toBe('translated: Tab 2');
    });

    describe.skip('when a tab is clicked', () => {
      let navigateSpy: SpyInstance
      beforeEach(() => {
        navigateSpy = jest.spyOn(router, 'navigate');
        fixtureHelper.clickOn('[data-testid="tab - Tab 1"]');
        fixture.detectChanges();
      });

      it('should navigate to the correct url', () => {
        expect(router.url).toBe('asd');
      });
    });

  });

});
