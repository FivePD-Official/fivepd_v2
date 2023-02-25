import { ActionResponse } from '@models';

export interface ColumnOptions<T> {
  header: string;
  accessorKey?: Extract<keyof T, string>;
  filter?: {
    label: string;
  };
  linkResolver?: (item: T) => string;
  valueResolver?: (item: T) => string;
  actions?: TableCellActions<T>[];
  show?: () => boolean;
  disabled?: () => boolean;
}

export class Column<T> {

  header: string = '';
  accessorKey?: Extract<keyof T, string>;
  filter?: {
    label: string;
  };
  linkResolver?: (item: T) => string;
  valueResolver?: (item: T) => string;
  actions?: TableCellActions<T>[];
  show: () => boolean;

  constructor(options: ColumnOptions<T>) {
    this.header = options.header;
    this.accessorKey = options.accessorKey;
    this.filter = options.filter;
    this.linkResolver = options.linkResolver;
    this.valueResolver = options.valueResolver;
    this.actions = options.actions;
    this.show = options.show ? options.show : () => true;
  }

}

export interface FormattedRowItem<T> {
  value: string;
  link?: string;
  originalItem: T;
  actions?: Column<T>['actions'];
}

export type FormattedRow<T> = FormattedRowItem<T>[];

export type TableCellActions<T> = TableCellActionButton<T> | TableCellActionSelect<T>;

export interface TableCellAction<T> {
  type: 'button' | 'select';
  label: string;
  show?: (item: T) => boolean;
  disabled?: (item: T) => boolean;
}

export interface TableCellActionButton<T> extends TableCellAction<T> {
  onClick: (item: T) => void;
}

export interface TableCellActionSelect<T> extends TableCellAction<T> {
  selectedItemResolver: (item: T, dropdownItemKey: number) => boolean;
  items: () => TableCellActionSelectItem[];
  onSelect: (item: T, selectedId: number, onResponse: (response: ActionResponse) => void) => void;
}

interface TableCellActionSelectItem {
  key: number;
  label: string;
  selected?: boolean;
}
