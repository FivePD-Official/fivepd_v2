import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { EventService } from '@services/event/event.service';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { CustomHttpInterceptor } from './interceptors/custom-http-interceptor.interceptor';
import { MdtComponent } from './pages/mdt/mdt.component';
import { HeaderComponent } from './components/header/header.component';
import { BlankComponent } from './pages/blank/blank.component';
import { MenuComponent } from './pages/menu/menu.component';
import { DashboardComponent } from './pages/mdt/dashboard/dashboard.component';
import { PageHeaderComponent } from './components/page-header/page-header.component';
import { WindowService } from '@services/window/window.service';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';
import { TranslatePipe } from './pipes/translate.pipe';
import { MenuService } from '@services/menu/menu.service';
import { LocalizationService } from '@services/localization/localization.service';
import { NotificationService } from '@services/notification/notification.service';
import { NotificationWrapperComponent } from './components/notification-wrapper/notification-wrapper.component';
import { NotificationComponent } from './components/notification/notification.component';
import { ButtonComponent } from './components/button/button.component';
import { DynamicFormComponent } from './components/dynamic-form/dynamic-form.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DynamicFormInputComponent } from './components/dynamic-form-input/dynamic-form-input.component';
import { DepartmentService } from '@services/department/department.service';
import { ModalComponent } from './components/modal/modal.component';
import { GridComponent } from './components/grid/grid.component';
import { DepartmentsViewComponent } from './pages/mdt/departments/departments-view/departments-view.component';
import { UserService } from '@services/user/user.service';
import { ContainerComponent } from './components/container/container.component';
import { IconComponent } from './components/icon/icon.component';
import {
  CitizenDatabaseIndexComponent
} from './pages/mdt/citizen-database/citizen-database-index/citizen-database-index.component';
import { RadialMenuComponent } from './pages/radial-menu/radial-menu.component';
import { TimerBarComponent } from './pages/timer-bar/timer-bar.component';
import { ClickOutsideDirective } from './directives/click-outside.directive';
import { DateMaskDirective } from './directives/date-mask.directive';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { ReportsComponent } from './pages/mdt/reports/reports.component';
import { ModalFormComponent } from './components/modal/modal-form/modal-form.component';
import { ModalEntityViewComponent } from './components/modal/modal-entity-view/modal-entity-view.component';
import { TippyModule, tooltipVariation } from '@ngneat/helipopper';
import { DevtoolsGeneralComponent } from './pages/mdt/devtools/devtools-general/devtools-general.component';
import { DevtoolsCalloutsComponent } from './pages/mdt/devtools/devtools-callouts/devtools-callouts.component';
import { DevtoolsViewComponent } from './pages/mdt/devtools/devtools-view/devtools-view.component';
import { TabsComponent } from './components/tabs/tabs.component';
import { StoreModule } from '@ngrx/store';
import { userReducer } from '@store/user.reducer';
import { appReducer } from '@store/app.reducer';
import { DynamicFormDirective } from './components/dynamic-form/dynamic-form.directive';
import { ModalService } from '@services/modals/modal/modal.service';
import { UnitService } from "@services/unit/unit.service";
import { ConfigService } from "@services/config/config.service";
import { InformationComponent } from './pages/information/information.component';
import { NotMdtComponent } from './pages/not-mdt/not-mdt.component';
import { DispatchComponent } from './pages/mdt/dispatch/dispatch.component';
import { AttributeGridOptions } from "./pages/mdt/settings/_service/attribute-grid-options";
import { IncidentHistoryComponent } from './pages/mdt/incident-history/incident-history.component';
import { CitizenDatabaseIdComponent } from './pages/mdt/citizen-database/citizen-database-id/citizen-database-id.component';
import { MessagesComponent } from './pages/mdt/messages/messages.component';
import { PaginationComponent } from './components/pagination/pagination.component';
import { AsPipe } from './pipes/as.pipe';
import { WelcomeModalComponent } from "./pages/welcome-modal/welcome-modal.component";
import { CalloutDetailsComponent } from './pages/mdt/settings/callout-details/callout-details.component';
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { IncidentService } from "@services/incident/incident.service";
import { IncidentHistoryService } from "@services/incident-history/incident-history.service";
import { CalloutService } from "@services/callout/callout.service";
import { DevtoolsService } from "@services/devtools/devtools.service";
import { DispatchService } from "@services/dispatch/dispatch.service";
import { MessageService } from "@services/message/message.service";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { UserSettingsComponent } from "./pages/mdt/settings/_components/user-settings/user-settings.component";
import { AdminSettingsComponent } from "./pages/mdt/settings/_components/admin-settings/admin-settings.component";

@NgModule({
  declarations: [
    AppComponent,
    MdtComponent,
    HeaderComponent,
    BlankComponent,
    MenuComponent,
    DashboardComponent,
    PageHeaderComponent,
    SidebarComponent,
    SafeHtmlPipe,
    TranslatePipe,
    NotificationWrapperComponent,
    NotificationComponent,
    ButtonComponent,
    DynamicFormComponent,
    DynamicFormInputComponent,
    ModalComponent,
    GridComponent,
    DepartmentsViewComponent,
    ContainerComponent,
    IconComponent,
    CitizenDatabaseIndexComponent,
    RadialMenuComponent,
    TimerBarComponent,
    ClickOutsideDirective,
    DateMaskDirective,
    ReportsComponent,
    ModalFormComponent,
    ModalEntityViewComponent,
    DevtoolsGeneralComponent,
    DevtoolsCalloutsComponent,
    DevtoolsViewComponent,
    TabsComponent,
    DynamicFormDirective,
    InformationComponent,
    NotMdtComponent,
    DispatchComponent,
    IncidentHistoryComponent,
    WelcomeModalComponent,
    CitizenDatabaseIdComponent,
    MessagesComponent,
    PaginationComponent,
    AsPipe,
    CalloutDetailsComponent,
    AdminSettingsComponent,
    UserSettingsComponent,
  ],
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
    })
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
    LooseDataService,
    MenuService,
    MessageService,
    ModalService,
    NotificationService,
    PedHistoryService,
    UnitService,
    UserService,
    WindowService,
    AttributeGridOptions,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CustomHttpInterceptor,
      multi: true,
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
