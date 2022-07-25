export interface ColumnDef<T> {
  header: string;
  accessorKey: string;
  filter?: boolean;
  linkResolver?: (item: T) => string;
}

export interface FormattedColumn {
  value: string,
  isLink: boolean,
}

export interface FormattedRowItem {
  value: string,
  link?: string,
}

export type FormattedRow = FormattedRowItem[];