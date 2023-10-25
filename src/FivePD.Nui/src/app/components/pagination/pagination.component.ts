import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Collation, Page } from "@models";
import { TranslatePipe } from '../../pipes/translate.pipe';
import { IconComponent } from '../icon/icon.component';
import { NgClass, NgIf } from '@angular/common';

@Component({
  selector: 'app-pagination',
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.sass'],
  standalone: true,
  imports: [NgIf, NgClass, IconComponent, TranslatePipe]
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
