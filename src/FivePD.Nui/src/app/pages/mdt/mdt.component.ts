import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-mdt',
  templateUrl: './mdt.component.html',
  styleUrls: ['./mdt.component.sass']
})
export class MdtComponent implements OnInit {

  isSidebarCollapsed$: Observable<boolean>;

  constructor(
    private readonly store: Store<GlobalState>,
  ) {
    this.isSidebarCollapsed$ = this.store.select('app', 'isSidebarCollapsed');
  }

  ngOnInit(): void {
  }

}
