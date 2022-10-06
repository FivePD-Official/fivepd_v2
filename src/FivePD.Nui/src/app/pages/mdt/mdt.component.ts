import { Component, OnInit } from '@angular/core';
import { Select } from '@ngxs/store';
import { AppState } from '@store/app.state';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-mdt',
  templateUrl: './mdt.component.html',
  styleUrls: ['./mdt.component.sass']
})
export class MdtComponent implements OnInit {

  @Select(AppState.isSidebarCollapsed) isSidebarCollapsed$!: Observable<boolean>;

  constructor() { }

  ngOnInit(): void {
  }

}
