import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '@models';
import { WindowService } from '@services/window/window.service';
import { GlobalState } from '@store';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.sass']
})
export class HeaderComponent implements OnInit {

  user$: Observable<User>;
  currentStatus: string = '10-0';
  isStatusSelectOpened: boolean = false;
  statusList = ['10-1', '10-2', '10-3', '10-4'];

  constructor(
    private readonly store: Store<GlobalState>,
    public window: WindowService,
  ) {
    this.user$ = this.store.select('user', 'details');
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
