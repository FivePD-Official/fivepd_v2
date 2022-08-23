import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import {
  Button,
  Collation, Column,
  ColumnDef, FormattedRow,
  FormattedRowItem, FormInputBase,
  Page,
  TableCellAction,
  TableCellActionButton,
  TableCellActionSelect, TextInput
} from '@models';
import { debounceTime, Observable, Subject } from 'rxjs';

@Component({
  selector: 'app-grid',
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.sass']
})
export class GridComponent<T> implements OnInit, OnDestroy, OnChanges {

  @Input() title: string = '';
  @Input() buttons: Button[] = [];
  @Input() columns: Column<T>[] = [];
  @Input() data: Page<T> | undefined;

  formattedColumns: Column<T>[] = [];
  formattedRows: FormattedRow<T>[] = [];

  filter$ = new Subject();
  collation: Collation = {
    Page: 1,
    Filters: {},
    Sorting: {},
  };

  @Output() onCellAction = new EventEmitter();
  @Output() onCollationChange = new EventEmitter<Collation>();

  constructor() { }

  ngOnInit(): void {
    this.filter$.pipe(
      debounceTime(500)
    ).subscribe(() => this.onCollationChange.emit(this.collation));
  }

  ngOnDestroy(): void {
    this.filter$.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['columns']) {
      this.setFormattedColumns(changes['columns'].currentValue);
    }
    if (changes['data']) {
      this.setFormattedRows(changes['data'].currentValue);
    }
  }

  get isAnyColumnFilterable(): boolean {
    return this.columns.some(column => column.filter);
  }

  getFilterInput(column: Column<any>): FormInputBase<string> {
    return new TextInput({
      key: column.accessorKey,
      placeholder: column.filter?.label ?? '',
      required: false,
    });
  }

  setFormattedColumns(columns: Column<T>): void {
    this.formattedColumns = this.columns.filter(column => column.show);
  }

  setFormattedRows(data: FormattedRow<T>[]): void {
    this.formattedRows = this.data?.Items?.map((item: any) => {
      const row: FormattedRow<any> = [];

      for (const value of Object.values(this.columns)) {
        if (!value.show) {
          continue;
        }

        let newValue = '';

        if (value.valueResolver) {
          newValue = value.valueResolver(item);
        } else if (value.accessorKey) {
          newValue = item[value.accessorKey];
        }

        const rowItem: FormattedRowItem<any> = {
          value: newValue,
          originalItem: item,
        };

        if (value.actions) {
          rowItem.actions = value.actions;
        }
        if (value.linkResolver) {
          rowItem.link = value.linkResolver(item);
        }
        row.push(rowItem);
      }

      return row;
    }) ?? []
  };

  onFilterFormChange(accessorKey: string | undefined, value: string | number): void {
    if (!accessorKey) {
      return;
    }

    const filterCopy = { ...this.collation.Filters };
    if (filterCopy.hasOwnProperty(accessorKey)) {
      if (typeof value === 'string') {
        if (value) {
          filterCopy[accessorKey] = value;
        } else {
          delete filterCopy[accessorKey];
        }
      }
    } else {
      if (typeof value === 'string') {
        filterCopy[accessorKey] = value;
      }
    }

    this.collation.Filters = filterCopy;
    this.filter$.next(undefined);
  }

  toPreviousPage() {
    if (this.collation.Page === 1) {
      return;
    }

    this.collation.Page--;
    this.onCollationChange.emit(this.collation);
  }

  toNextPage() {
    if (this.collation.Page === this.data?.PageCount) {
      return;
    }

    this.collation.Page++;
    this.onCollationChange.emit(this.collation);
  }

  toPage(pageNumber: number) {
    if (!this.data) {
      return;
    }

    if (this.collation.Page === pageNumber || this.data.PageCount < pageNumber) {
      return;
    }

    this.collation.Page = pageNumber;
    this.onCollationChange.emit(this.collation);
  }

  tableCellActionButtonClick(action: TableCellAction, item: FormattedRowItem<any>) {
    (action as TableCellActionButton<any>).onClick(item.originalItem);
  }

  tableCellActionSelect(action: TableCellAction, item: FormattedRowItem<any>, selectedId: number) {
    (action as TableCellActionSelect<any>).onSelect(item.originalItem, selectedId, res => {
      if (res.Status) {
        // TODO this.requestData(this.filter);
        this.onCellAction.emit();
      }
    });
  }

  headerButtonOnClick(button: Button): void {
    if (button.onClick) {
      button.onClick();
    }
  }
}
