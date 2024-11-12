import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { EventService } from '@services/event/event.service';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { CustomHttpInterceptor } from './interceptors/custom-http-interceptor.interceptor';
import { WindowService } from '@services/window/window.service';
import { MenuService } from '@services/menu/menu.service';
import { LocalizationService } from '@services/localization/localization.service';
import { NotificationService } from '@services/notification/notification.service';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DepartmentService } from '@services/department/department.service';
import { UserService } from '@services/user/user.service';
import { TippyModule, tooltipVariation } from '@ngneat/helipopper';
import { StoreModule } from '@ngrx/store';
import { userReducer } from '@store/user.reducer';
import { appReducer } from '@store/app.reducer';
import { ModalService } from '@services/modals/modal/modal.service';
import { UnitService } from "@services/unit/unit.service";
import { ConfigService } from "@services/config/config.service";
import { AttributeGridOptions } from "./pages/mdt/settings/_service/attribute-grid-options";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { IncidentService } from "@services/incident/incident.service";
import { IncidentHistoryService } from "@services/incident-history/incident-history.service";
import { CalloutService } from "@services/callout/callout.service";
import { DevtoolsService } from "@services/devtools/devtools.service";
import { DispatchService } from "@services/dispatch/dispatch.service";
import { MessageService } from "@services/message/message.service";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { VehicleService } from "@services/vehicle/vehicle.service";
import { sidebarReducer } from "@store/sidebar.reducer";
import { VariableDataService } from "@services/variable-data/variable-data.service";
import {
    GameNotificationWrapperComponent
} from "./components/game-notification-wrapper/game-notification-wrapper.component";

@NgModule({
  declarations: [AppComponent],
    imports: [
        BrowserModule,
        AppRoutingModule,
        HttpClientModule,
        ReactiveFormsModule,
        FormsModule,
        TippyModule.forRoot({
            defaultVariation: 'custom',
            variations: {
                custom: {
                    ...tooltipVariation,
                    theme: 'custom',
                    arrow: false,
                    animation: false,
                }
            }
        }),
        StoreModule.forRoot({
            app: appReducer,
            user: userReducer,
            sidebar: sidebarReducer,
        }),
        GameNotificationWrapperComponent,
    ],
  providers: [
    CalloutService,
    CalloutDetailsService,
    ConfigService,
    DepartmentService,
    DevtoolsService,
    DispatchService,
    EventService,
    IncidentService,
    IncidentHistoryService,
    LocalizationService,
    VariableDataService,
    MenuService,
    MessageService,
    ModalService,
    NotificationService,
    PedHistoryService,
    UnitService,
    UserService,
    WindowService,
    AttributeGridOptions,
    VehicleService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CustomHttpInterceptor,
      multi: true,
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
