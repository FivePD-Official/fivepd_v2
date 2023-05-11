import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-page-header',
  templateUrl: './page-header.component.html',
  styleUrls: ['./page-header.component.sass']
})
export class PageHeaderComponent implements OnInit {

  @Input() title: string = '';
  @Input() centerTitle?: boolean;
  @Input() underline?: boolean = true;

  constructor() { }

  ngOnInit(): void {
  }

}
