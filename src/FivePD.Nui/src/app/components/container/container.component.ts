import { Component, Input, OnInit } from '@angular/core';
import { NgStyle } from '@angular/common';

@Component({
    selector: 'app-container',
    templateUrl: './container.component.html',
    styleUrls: ['./container.component.sass'],
    imports: [NgStyle]
})
export class ContainerComponent implements OnInit {

  @Input() rows: number = 1;
  @Input() columns: number = 1;

  gridTemplateColumns: string = '';
  gridTemplateRows: string = '';

  constructor() {
  }

  ngOnInit(): void {
    this.gridTemplateColumns = `repeat(${this.columns}, 1fr)`;
    this.gridTemplateRows = `repeat(${this.rows}, auto)`;
  }

}
