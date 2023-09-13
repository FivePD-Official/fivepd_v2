import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Collation, Page } from "@models";

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.sass']
})
export class PaginationComponent {

  @Input() data?: Page<any> | null;
  @Input() collation!: Collation;

  @Output() onPageChange = new EventEmitter<number>();

  toPreviousPage() {
    if (this.collation?.Page === 1) {
      return;
    }

    this.onPageChange.emit(--this.collation.Page);
  }

  toNextPage() {
    if (this.collation.Page === this.data?.PageCount) {
      return;
    }

    this.onPageChange.emit(++this.collation.Page);
  }

  toPage(pageNumber: number) {
    if (!this.data) {
      return;
    }

    if (this.collation.Page === pageNumber || this.data.PageCount < pageNumber) {
      return;
    }

    this.collation.Page = pageNumber;
    this.onPageChange.emit(pageNumber);
  }

}
