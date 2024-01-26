import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { NotificationWrapperComponent } from '../../components/notification-wrapper/notification-wrapper.component';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../../components/sidebar/sidebar.component';
import { HeaderComponent } from '../../components/header/header.component';
import { NgClass } from "@angular/common";

@Component({
  selector: 'app-mdt',
  templateUrl: './mdt.component.html',
  styleUrls: ['./mdt.component.sass'],
  standalone: true,
  imports: [NgClass, HeaderComponent, SidebarComponent, RouterOutlet, NotificationWrapperComponent]
})
export class MdtComponent implements OnInit {

  isSidebarCollapsed: boolean = false;
  dispatchMode: boolean = false;
  windowMaximized: boolean = false;

  constructor(
    private readonly _store: Store<GlobalState>,
  ) {
    _store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.dispatchMode = state.app.dispatchMode;
      this.isSidebarCollapsed = state.sidebar.collapsed;
      this.windowMaximized = state.app.windowMaximized;
    });
  }

  ngOnInit(): void {
  }

}
