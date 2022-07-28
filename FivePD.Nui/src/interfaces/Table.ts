export interface Column<T> {
  header: string;
  accessorKey: keyof T;
  filter?: boolean;
  linkResolver?: (item: T) => string;
  valueResolver?: (item: T) => string;
}

export type ColumnDef<T> = Column<T>[];

export interface FormattedRowItem {
  value: string,
  link?: string,
}

export type FormattedRow = FormattedRowItem[];