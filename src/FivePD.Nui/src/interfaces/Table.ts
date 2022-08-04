export interface Column<T> {
  header: string;
  accessorKey: Extract<keyof T, string>;
  filter?: boolean;
  linkResolver?: (item: T) => string;
  valueResolver?: (item: T) => string;
}

export type ColumnDef<T> = Column<T>[];

export interface FormattedRowItem {
  value: string;
  link?: string;
}

export type FormattedRow = FormattedRowItem[];