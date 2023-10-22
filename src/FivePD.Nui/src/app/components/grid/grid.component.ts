import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import {
  Button,
  Checkbox,
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
import { Router } from "@angular/router";

@Component({
  selector: 'app-grid',
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.sass']
})
export class GridComponent<T> implements OnInit, OnDestroy, OnChanges {

  @Input() title: string = '';
  @Input() buttons: Button[] = [];
  @Input() columns: Column<T>[] = [];
  @Input() data: Page<T> | undefined | null;
  @Input() showPagination: boolean = true;
  @Input() rowLinkResolver?: (item: T) => string;

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

  constructor(private readonly _router: Router) { }

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

  private previousColumnShowValues: boolean[] = [];

  get hackToDetectColumnShowChange(): null {
    const values = this.columns.map(column => column.show());
    if (values.some((value, index) => value !== this.previousColumnShowValues[index])) {
      this.setFormattedColumns();
    }
    this.previousColumnShowValues = values;

    return null;
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
    const dropdown = new Dropdown({
      key: '#',
      dropdownOptions: {
        label: action.label,
        items: action.items(),
        size: 'small',
        height: 'sm',
        variant: 'tertiary-bg',
      },
      required: false,
    });

    if ('selectedItemResolver' in action) {
      (dropdown.dropdownOptions as any).type = 'single';
      (dropdown.dropdownOptions as any).selectedItemResolver = (dropdownItemKey: any) => action.selectedItemResolver(item.originalItem, dropdownItemKey);
    }
    if ('selectedItemsResolver' in action) {
      (dropdown.dropdownOptions as any).type = 'multi';
      (dropdown.dropdownOptions as any).selectedItemsResolver = (dropdownItemKey: any) => action.selectedItemsResolver(item.originalItem, dropdownItemKey);
    }

    return dropdown;
  }

  getCheckboxField(action: any, item: FormattedRowItem<T>): FormInputBase<boolean> {
    return new Checkbox({
      key: '#',
      required: false,
      value: action.checkedResolver(item.originalItem),
    });
  }

  setFormattedColumns(): void {
    this.formattedColumns = this.columns.filter(column => column.show());
    this.setFormattedRows();
  }

  setFormattedRows(): void {
    this.formattedRows = this.data?.Items?.map((item: any) => {
      const row: FormattedRow<any> = [];

      const originalItem = {...item};
      for (const value of Object.values(this.formattedColumns)) {
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
          badge: value.badgeResolver ? value.badgeResolver(mappedItem) : undefined,
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

  onPageChange(page: number): void {
    this.collation.Page = page;
    this.onCollationChange.emit(this.collation);
  }

  tableCellActionButtonClick(action: TableCellAction<T>, item: FormattedRowItem<T>): void {
    (action as TableCellActionButton<any>).onClick(item.originalItem);
  }

  onTableCellActionSelect(action: any, item: FormattedRowItem<T>, selectedItemKey: number): void {
    if ('onSelect' in action) {
      action.onSelect(item.originalItem, selectedItemKey);
    } else {
      action.onSelectMulti(item.originalItem, selectedItemKey);
    }
  }

  onTableCellActionCheckbox(action: any, item: FormattedRowItem<T>, checked: boolean): void {
    action.onCheck(item.originalItem, checked);
  }

  headerButtonOnClick(button: Button): void {
    if (button.onClick) {
      button.onClick();
    }
  }

  onRowClick(row: FormattedRowItem<T>[]): void {
    if (this.rowLinkResolver) {
      this._router.navigateByUrl(this.rowLinkResolver(row[0].originalItem));
    }
  }
}
