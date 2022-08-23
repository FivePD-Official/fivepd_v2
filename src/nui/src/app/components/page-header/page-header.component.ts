import { Component, Input, OnInit } from '@angular/core';
import type { Localization, Paths } from '@models';

@Component({
  selector: 'app-page-header',
  templateUrl: './page-header.component.html',
  styleUrls: ['./page-header.component.sass']
})
export class PageHeaderComponent implements OnInit {

  @Input() title: string = '';
  @Input() centerTitle?: boolean;

  constructor() { }

  ngOnInit(): void {
  }

}
