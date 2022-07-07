export interface Column<T = any> {
  valueResolver?: (rowItem: any, columnKey: string, columnValue: Column, substituteList: any[]) => string,
  linkResolver?: (rowItem: any, columnKey: string, columnValue: Column) => string,
  label: string,
};

export interface FormattedColumn {
  value: string,
  isLink: boolean,
}

export interface FormattedRowItem {
  value: string,
  link?: string,
}

export type FormattedRow = FormattedRowItem[];