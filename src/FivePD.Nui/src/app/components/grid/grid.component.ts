import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import {
  Button,
  CheckboxField,
  Collation,
  Column,
  FormattedRow,
  FormattedRowItem,
  FormInputBase,
  MultiSelectField,
  Page,
  SingleSelectField,
  TableCellAction,
  TableCellActionButton,
  TextField
} from '@models';
import { debounceTime, Subject } from 'rxjs';
import { formatDate } from '@helpers';
import { Router, RouterLink } from "@angular/router";
import { TranslatePipe } from '../../pipes/translate.pipe';
import { PaginationComponent } from '../pagination/pagination.component';
import { IconComponent } from '../icon/icon.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TippyModule } from '@ngneat/helipopper';
import { ButtonComponent } from '../button/button.component';
import { NgClass } from '@angular/common';
import { TextFieldComponent } from "../form-inputs/text-field/text-field.component";
import { SingleSelectFieldComponent } from "../form-inputs/single-select-field/single-select-field.component";
import { MultiSelectFieldComponent } from "../form-inputs/multi-select-field/multi-select-field.component";
import { BadgeComponent } from "../badge/badge.component";

@Component({
    selector: 'app-grid',
    templateUrl: './grid.component.html',
    styleUrls: ['./grid.component.sass'],
    imports: [ButtonComponent, TippyModule, ReactiveFormsModule, FormsModule, NgClass, RouterLink, IconComponent, PaginationComponent, TranslatePipe, TextFieldComponent, SingleSelectFieldComponent, MultiSelectFieldComponent, BadgeComponent]
})
export class GridComponent<T> implements OnInit, OnDestroy, OnChanges {

  @Input() title: string = '';
  @Input() buttons: Button[] = [];
  @Input() columns: Column<T>[] = [];
  @Input() data: Page<T> | undefined | null;
  @Input() showPagination: boolean = true;
  @Input() rowLinkResolver?: (item: T) => string;

  @Input() rowsClickable?: boolean = false;
  @Output() rowClick = new EventEmitter<T>();

  formattedColumns: Column<T>[] = [];
  formattedRows: FormattedRow<T>[] = [];

  filter$ = new Subject();

  readonly defaultCollation: Collation = {
    Page: 1,
    Filters: {},
    Sorting: {},
  };
  _collation: Collation = this.defaultCollation;
  @Input() set collation(value: Collation<any> | null) {
    this._collation = value ?? this.defaultCollation;
  }

  @Output() onCellAction = new EventEmitter();
  @Output() onCollationChange = new EventEmitter<Collation>();

  constructor(private readonly _router: Router) {
  }

  ngOnInit(): void {
    this.filter$.pipe(
      debounceTime(500)
    ).subscribe(() => this.onCollationChange.emit(this._collation));
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

  getFilterInput(column: Column<any>) {
    return new TextField({
      key: column.accessorKey,
      placeholder: column.filter?.label ?? '',
      required: false,
    });
  }

  getFilterSingleSelectInput(column: Column<any>) {
    return new SingleSelectField({
      key: column.accessorKey,
      label: '',
      defaultItemLabel: column.filter?.label ?? '',
      items: () => column.filter?.dropdown ?? [],
      size: 'small',
      height: 'sm',
      variant: 'secondary',
      required: false,
    });
  }

  getSingleSelectField(action: any, item: FormattedRowItem<T>) {
    return new SingleSelectField({
      key: '#',
      value: action.value ? action.value(item.originalItem) : -1,
      label: '',
      defaultItemLabel: action.label,
      items: () => action.items(),
      size: 'small',
      height: 'sm',
      variant: 'tertiary-bg',
      required: false,
    });
  }

  getMultiSelectField(action: any, item: FormattedRowItem<T>) {
    return new MultiSelectField({
      key: '#',
      value: action.value ? action.value(item.originalItem) : [],
      label: '',
      defaultItemLabel: action.label,
      items: () => action.items(),
      size: 'small',
      height: 'sm',
      variant: 'tertiary-bg',
      required: false,
    });
  }

  getCheckboxField(action: any, item: FormattedRowItem<T>): FormInputBase<boolean> {
    return new CheckboxField({
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

        if (value.cellAction) {
          rowItem.cellAction = value.cellAction;
        }

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

    const filterCopy = {...this._collation.Filters};
    if (filterCopy.hasOwnProperty(accessorKey)) {
      if (typeof value === 'string' || typeof value === 'number') {
        if (value) {
          filterCopy[accessorKey] = value;
        } else {
          delete filterCopy[accessorKey];
        }
      }
    } else {
      if (typeof value === 'string' || typeof value === 'number') {
        filterCopy[accessorKey] = value;
      }
    }

    this._collation.Filters = filterCopy;
    this.filter$.next(undefined);
  }

  onPageChange(page: number): void {
    this._collation.Page = page;
    this.onCollationChange.emit(this._collation);
  }

  tableCellActionButtonClick(action: TableCellAction<T>, item: FormattedRowItem<T>): void {
    (action as TableCellActionButton<any>).onClick(item.originalItem);
  }

  onTableCellActionSelect(action: any, item: FormattedRowItem<T>, selectedItemKey: any): void {
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

    if (this.rowsClickable) {
      this.rowClick.emit(row[0].originalItem);
    }
  }
}
