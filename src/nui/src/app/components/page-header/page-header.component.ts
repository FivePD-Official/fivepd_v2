import { Component, Input, OnInit } from '@angular/core';
import type { Localization, Paths, PropButton } from '@interfaces';

@Component({
  selector: 'app-page-header',
  templateUrl: './page-header.component.html',
  styleUrls: ['./page-header.component.sass']
})
export class PageHeaderComponent implements OnInit {

  @Input() title: Paths<Localization> = '';
  @Input() centerTitle?: boolean;
  @Input() buttons?: PropButton[];

  constructor() { }

  ngOnInit(): void {
  }

}
