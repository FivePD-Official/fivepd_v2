import { Component, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { Observable } from 'rxjs';
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";

@Component({
  selector: 'app-mdt',
  templateUrl: './mdt.component.html',
  styleUrls: ['./mdt.component.sass']
})
export class MdtComponent implements OnInit {

  isSidebarCollapsed: boolean = false;
  dispatchMode: boolean = false;

  constructor(
    private readonly _store: Store<GlobalState>,
  ) {
    _store.pipe(takeUntilDestroyed()).subscribe(state => {
      this.dispatchMode = state.app.dispatchMode;
      this.isSidebarCollapsed = state.app.isSidebarCollapsed;
    });
  }

  ngOnInit(): void {
  }

}
