import { Component, OnInit } from '@angular/core';
import { User } from '@models';
import { WindowService } from '@services/window/window.service';
import { GlobalState } from '@store';
import { Store } from '@ngrx/store';
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.sass']
})
export class HeaderComponent implements OnInit {

  user?: User;
  currentStatus: string = '10-0';
  isStatusSelectOpened: boolean = false;
  statusList = ['10-1', '10-2', '10-3', '10-4'];

  constructor(
    private readonly store: Store<GlobalState>,
    public window: WindowService,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(res => {
      this.user = res.user.details.User;
    });
  }

  ngOnInit(): void {
  }


  toggleStatusSelect(): void {
    this.isStatusSelectOpened = !this.isStatusSelectOpened;
  }

  closeStatusSelect(): void {
    this.isStatusSelectOpened = false;
  }

  changeStatus(status: string): void {
    this.toggleStatusSelect();
    this.currentStatus = status;
  }
}
