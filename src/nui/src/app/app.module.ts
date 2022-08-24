import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NgxsModule } from '@ngxs/store';
import { NgxsLoggerPluginModule } from '@ngxs/logger-plugin';
import { NgxsReduxDevtoolsPluginModule } from '@ngxs/devtools-plugin';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { EventService } from '@services/event/event.service';
import { environment } from '../environments/environment';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { CustomHttpInterceptor } from './interceptors/custom-http-interceptor.interceptor';
import { MdtComponent } from './pages/mdt/mdt.component';
import { HeaderComponent } from './components/header/header.component';
import { AppState } from '@store/app.state';
import { UserState } from '@store/user.state';
import { BlankComponent } from './pages/blank/blank.component';
import { MenuComponent } from './pages/menu/menu.component';
import { DashboardComponent } from './pages/mdt/dashboard/dashboard.component';
import { PageHeaderComponent } from './components/page-header/page-header.component';
import { WindowService } from '@services/window/window.service';
import { ModalService } from '@services/modal/modal.service';
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
import { DepartmentService } from '@services/entity/department.service';
import { ModalComponent } from './components/modal/modal.component';
import { GridComponent } from './components/grid/grid.component';
import { DepartmentsViewComponent } from './pages/mdt/departments/departments-view/departments-view.component';
import { UserService } from '@services/user/user.service';
import { ColumnsComponent } from './components/columns/columns.component';

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
    ColumnsComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgxsModule.forRoot([
      AppState,
      UserState
    ], {
      developmentMode: !environment.production
    }),
    NgxsLoggerPluginModule.forRoot(),
    NgxsReduxDevtoolsPluginModule.forRoot(),
    ReactiveFormsModule,
    FormsModule,
  ],
  providers: [
    WindowService,
    EventService,
    ModalService,
    MenuService,
    LocalizationService,
    NotificationService,
    DepartmentService,
    UserService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CustomHttpInterceptor,
      multi: true,
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
