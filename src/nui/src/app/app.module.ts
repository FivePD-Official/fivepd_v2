import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NgxsModule } from '@ngxs/store';
import { NgxsLoggerPluginModule } from '@ngxs/logger-plugin';
import { NgxsReduxDevtoolsPluginModule } from '@ngxs/devtools-plugin';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { EventService } from './services/event.service';
import { environment } from '../environments/environment';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { CustomHttpInterceptor } from './interceptors/custom-http-interceptor.interceptor';
import { MdtComponent } from './pages/mdt/mdt.component';
import { HeaderComponent } from './components/header/header.component';
import { AppState } from './store/app.state';
import { UserState } from './store/user.state';
import { BlankComponent } from './pages/blank/blank.component';
import { MenuComponent } from './pages/menu/menu.component';
import { DashboardComponent } from './pages/mdt/dashboard/dashboard.component';
import { PageHeaderComponent } from './components/page-header/page-header.component';
import { WindowService } from './services/window.service';
import { ModalService } from './services/modal.service';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';
import { TranslatePipe } from './pipes/translate.pipe';
import { MenuService } from './services/menu.service';
import { LocalizationService } from './services/localization.service';

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
    TranslatePipe
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
  ],
  providers: [
    WindowService,
    EventService,
    ModalService,
    MenuService,
    LocalizationService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CustomHttpInterceptor,
      multi: true,
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
