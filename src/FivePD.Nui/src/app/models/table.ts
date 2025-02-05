import {ActionResponse, DropdownItem, IconType} from '@models';
import { BadgeVariants } from '../components/badge/badge.component';

export interface ColumnOptions<T> {
  header: string;
  accessorKey?: Extract<keyof T, string>;
  filter?: {
    label: string;
    dropdown?: DropdownItem[];
  };
  linkResolver?: (item: T) => string;
  valueResolver?: (item: T) => string;
  badgeResolver?: (item: T) => BadgeVariants;
  actions?: TableCellActions<T>[];
  show?: () => boolean;
  disabled?: () => boolean;
  cellAction?: {
    icon: IconType;
    onClick: (item: T) => void;
  };
}

export class Column<T> {

  header: string = '';
  accessorKey?: Extract<keyof T, string>;
  filter?: {
    label: string;
    dropdown?: DropdownItem[];
  };
  linkResolver?: (item: T) => string;
  valueResolver?: (item: T) => string;
  badgeResolver?: (item: T) => BadgeVariants;
  cellAction?: {
    icon: IconType;
    onClick: (item: T) => void;
  };
  actions?: TableCellActions<T>[];
  show: () => boolean;

  constructor(options: ColumnOptions<T>) {
    this.header = options.header;
    this.accessorKey = options.accessorKey;
    this.filter = options.filter;
    this.linkResolver = options.linkResolver;
    this.valueResolver = options.valueResolver;
    this.badgeResolver = options.badgeResolver;
    this.actions = options.actions;
    this.show = options.show ? options.show : () => true;
    this.cellAction = options.cellAction;
  }

}

export interface FormattedRowItem<T> {
  value: string;
  badge?: BadgeVariants;
  link?: string;
  originalItem: T;
  actions?: Column<T>['actions'];
  cellAction?: Column<T>['cellAction'];
}

export type FormattedRow<T> = FormattedRowItem<T>[];

export type TableCellActions<T> =
  | TableCellActionButton<T>
  | TableCellActionSelect<T>
  | TableCellActionSelectMulti<T>
  | TableCellActionCheckbox<T>;

export interface TableCellAction<T> {
  type: 'button' | 'singleSelect' | 'multiSelect' | 'checkbox';
  label: string;
  show?: (item: T) => boolean;
  disabled?: (item: T) => boolean;
}

export interface TableCellActionButton<T> extends TableCellAction<T> {
  onClick: (item: T) => void;
}

export interface TableCellActionSelect<T> extends TableCellAction<T> {
  value: (item: T) => number;
  items: () => TableCellActionSelectItem[];
  onSelect: (item: T, selectedId: number, onResponse: (response: ActionResponse) => void) => void;
}

export interface TableCellActionSelectMulti<T> extends TableCellAction<T> {
  value: (item: T) => number[];
  items: () => TableCellActionSelectItem[];
  onSelectMulti: (item: T, selectedIds: number[], onResponse: (response: ActionResponse) => void) => void;
  resetWhenSelectedId?: number;
}

export interface TableCellActionCheckbox<T> extends TableCellAction<T> {
  checkedResolver: (item: T) => boolean;
  onCheck: (item: T, checked: boolean) => void;
  selectedItemResolver?: never;
  items?: never;
  onSelectMulti?: never;
  resetWhenSelectedId?: never;
  onSelect?: never;
}

interface TableCellActionSelectItem {
  key: number;
  label: string;
  selected?: boolean;
}
