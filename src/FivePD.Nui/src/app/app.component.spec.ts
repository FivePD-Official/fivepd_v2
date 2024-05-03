import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AppComponent } from './app.component';
import { MockedWithProperties, TestHelperModule } from "../test-helpers/test-helper.module";
import { HttpClientTestingModule, HttpTestingController, TestRequest } from "@angular/common/http/testing";
import { WindowService } from "@services/window/window.service";
import { EventService } from "@services/event/event.service";
import { LocalizationService } from "@services/localization/localization.service";
import { MockWindowEventListener } from "../test-helpers/mock-window-event-listener";
import { NavigationEnd, NavigationStart, Router } from "@angular/router";
import SpyInstance = jest.SpyInstance;
import { TestHelper } from "../test-helpers/test-helper";
import { FixtureHelper } from "../test-helpers/fixture-helper";

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let fixtureHelper: FixtureHelper;
  let httpTestingController: HttpTestingController;
  let router: MockedWithProperties<Router>;
  let windowService: WindowService;
  let eventService: EventService;
  let windowEventListener: MockWindowEventListener;
  let localizationService: LocalizationService;

  let onLoadSpy: SpyInstance;
  let localizationLoadSpy: SpyInstance;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        RouterTestingModule,
        HttpClientTestingModule,
        TestHelperModule.builder()
          .withWindowEventListener()
          .withRouter()
          .withStore()
          .build()
      ],
      declarations: [AppComponent],
      providers: [
        WindowService,
        EventService,
        LocalizationService,
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    fixtureHelper = new FixtureHelper(fixture);
    component = fixture.componentInstance;

    httpTestingController = TestBed.inject(HttpTestingController);
    router = TestHelper.injectMock(Router);
    windowService = TestBed.inject(WindowService);
    eventService = TestBed.inject(EventService);
    windowEventListener = TestBed.inject(MockWindowEventListener);
    localizationService = TestBed.inject(LocalizationService);

    onLoadSpy = jest.spyOn(component, 'onLoad');
    localizationLoadSpy = jest.spyOn(localizationService, 'load');

    fixture.detectChanges();
  });

  it('can be created', () => {
    expect(component).toBeTruthy();
  });


  describe('on init', () => {
    it('should render the "main-container" with a "router-outlet" in it', () => {
      expect(fixtureHelper.findElement(".main-container")).toBeTruthy();
      expect(fixtureHelper.findElement(".main-container router-outlet")).toBeTruthy();
    });

    it('component.onLoad should be called', () => {
      expect(onLoadSpy).toHaveBeenCalled();
    });

    it('should call localizationService.load', () => {
      expect(localizationLoadSpy).toHaveBeenCalled();
    });

    describe('on window message event', () => {
      let eventNextSpy: SpyInstance;
      beforeEach(() => {
        eventNextSpy = jest.spyOn(eventService, 'next');
        windowEventListener.message({ data: 'asd' })
      });

      it('should call eventService.next with event.data', () => {
        expect(eventNextSpy).toHaveBeenCalledWith('asd');
      });
    });

    // describe('should send NuiEventType.IsNuiReady request', () => {
    //   let req: TestRequest;
    //   beforeEach(() => {
    //     req = httpTestingController.expectOne("nuiready");
    //   });
    //
    //   it('req method = POST', () => {
    //     expect(req.request.method).toEqual('POST');
    //   });
    // });

    // describe('on router navigation event', () => {
    //
    //   describe('if the event is NOT an instance of NavigationEnd', () => {
    //     let setMdtPathSpy: SpyInstance;
    //     beforeEach(() => {
    //       setMdtPathSpy = jest.spyOn(component, 'setMdtPath');
    //       router.events.next(new NavigationStart(1, '/mdt'));
    //     });
    //
    //     it('should NOT call component.setMdtPath', () => {
    //       expect(setMdtPathSpy).not.toHaveBeenCalled();
    //     });
    //   });
    //
    //   describe('if the event url starts with "/mdt"', () => {
    //     let setMdtPathSpy: SpyInstance;
    //     beforeEach(() => {
    //       setMdtPathSpy = jest.spyOn(component, 'setMdtPath');
    //       router.events.next(new NavigationEnd(1, '/mdt', '/'));
    //     });
    //
    //     it('should call component.setMdtPath', () => {
    //       expect(setMdtPathSpy).toHaveBeenCalled();
    //     });
    //   });
    //
    //   describe('if the event url does NOT start with "/mdt"', () => {
    //     let setMdtPathSpy: SpyInstance;
    //     beforeEach(() => {
    //       setMdtPathSpy = jest.spyOn(component, 'setMdtPath');
    //       router.events.next(new NavigationEnd(1, '/test', '/'));
    //     });
    //
    //     it('should NOT call component.setMdtPath', () => {
    //       expect(setMdtPathSpy).not.toHaveBeenCalled();
    //     });
    //   });
    // });

    // describe('on window keydown event', () => {
    //   describe('when event.code equals to "Escape"', () => {
    //     let windowHideSpy: SpyInstance;
    //     beforeEach(() => {
    //       windowHideSpy = jest.spyOn(windowService, 'hide');
    //       windowEventListener.keydown({ code: 'Escape' })
    //     });
    //
    //     it('should call windowService.hide', () => {
    //       expect(windowHideSpy).toHaveBeenCalled();
    //     });
    //   });
    //
    //   describe('when event.code does NOT equal to "Escape"', () => {
    //     let windowHideSpy: SpyInstance;
    //     beforeEach(() => {
    //       windowHideSpy = jest.spyOn(windowService, 'hide');
    //       windowEventListener.keydown({ code: 'X' })
    //     });
    //
    //     it('should NOT call windowService.hide', () => {
    //       expect(windowHideSpy).not.toHaveBeenCalled();
    //     });
    //   });
    // });

  });
});
