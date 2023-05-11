import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import {
  Button,
  Collation,
  Column,
  Dropdown,
  FormattedRow,
  FormattedRowItem,
  FormInputBase,
  Page,
  TableCellAction,
  TableCellActionButton,
  TextInput
} from '@models';
import { debounceTime, Subject } from 'rxjs';
import { formatDate } from '@helpers';

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
      this.setFormattedColumns();
    }
    if (changes['data']) {
      this.setFormattedRows();
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

  getSelectField(action: any, item: FormattedRowItem<T>): FormInputBase<number> {
    return new Dropdown({
      key: '#',
      dropdownOptions: {
        label: action.label,
        items: action.items(),
        size: 'small',
        selectedItemResolver: (dropdownItemKey) => action.selectedItemResolver(item.originalItem, dropdownItemKey),
      },
      required: false,
    });
  }

  setFormattedColumns(): void {
    this.formattedColumns = this.columns.filter(column => column.show());
  }

  setFormattedRows(): void {
    this.formattedRows = this.data?.Items?.map((item: any) => {
      const row: FormattedRow<any> = [];

      for (const value of Object.values(this.columns)) {
        if (!value.show()) {
          continue;
        }

        const originalItem = {...item};
        const mappedItem = this.mapEntity({...item});

        let newValue = '';
        if (value.valueResolver) {
          newValue = value.valueResolver(mappedItem);
        } else if (value.accessorKey) {
          newValue = mappedItem[value.accessorKey];
        }

        const rowItem: FormattedRowItem<any> = {
          value: newValue,
          originalItem,
        };


        if (value.actions) {
          rowItem.actions = value.actions;
        }
        if (value.linkResolver) {
          rowItem.link = value.linkResolver(mappedItem);
        }
        row.push(rowItem);
      }

      return row;
    }) ?? [];
  };

  mapEntity<T extends any>(entity: T): T {
    // @ts-ignore
    for (const [key, value] of Object.entries(entity)) {
      if (value instanceof Date) {
        // @ts-ignore
        entity[key] = formatDate(new Date(value));
      }
    }
    return entity;
  }


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

  tableCellActionButtonClick(action: TableCellAction<T>, item: FormattedRowItem<T>): void {
    (action as TableCellActionButton<any>).onClick(item.originalItem);
  }

  onTableCellActionSelect(action: any, item: FormattedRowItem<T>, selectedItemKey: number): void {
    console.log('change', selectedItemKey)
    action.onSelect(item.originalItem, selectedItemKey);
  }

  headerButtonOnClick(button: Button): void {
    if (button.onClick) {
      button.onClick();
    }
  }
}
