import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-columns',
  templateUrl: './columns.component.html',
  styleUrls: ['./columns.component.sass']
})
export class ColumnsComponent implements OnInit {

  @Input() rows: number = 1; // TODO?
  @Input() columns: number = 1;

  gridTemplateColumns: string = '';

  constructor() { }

  ngOnInit(): void {
    this.gridTemplateColumns = `repeat(${this.columns}, 1fr)`;
  }

}
