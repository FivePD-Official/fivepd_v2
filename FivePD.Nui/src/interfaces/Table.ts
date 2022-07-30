import { ActionResponse } from "@interfaces";

export interface Column<T> {
  header: string;
  accessorKey?: Extract<keyof T, string>;
  filter?: boolean;
  linkResolver?: (item: T) => string;
  valueResolver?: (item: T) => string;
  actions?: TableCellActions<T>[];
  show?: () => boolean;
}

export type ColumnDef<T> = Column<T>[];

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
}

export interface TableCellActionButton<T> extends TableCellAction<T> {
  onClick: (item: T, onResponse: (response: ActionResponse) => void) => void;
}

export interface TableCellActionSelect<T> extends TableCellAction<T> {
  selectedItemResolver: (item: T, itemId: number) => boolean;
  items: () => TableCellActionSelectItem[];
  onSelect: (item: T, selectedId: number, onResponse: (response: ActionResponse) => void) => void;
}

interface TableCellActionSelectItem {
  id: number;
  label: string;
  selected?: boolean;
}