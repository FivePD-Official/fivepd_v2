export interface IDateInput {
  name: string;
  placeholder: string;
}

export interface InputField {
  type: 'text' | 'number' | 'date' | 'checkboxGroup' | 'select';
  name: string;
  label?: string;
  required: boolean;
  helpLabel?: string;
  placeholder?: string;
  defaultValue?: () => string | number | undefined;
  validator?: (value: any) => boolean;
  checkboxes?: {
    id: string;
    label: string;
    defaultValue?: boolean;
  }[];
  items?: () => SelectItem[];
  defaultItem?: Omit<SelectItem, 'selected'>;
  selectedItemResolver?: (item: SelectItem) => boolean;
  onSelect?: (item: SelectItem) => void;
}

export interface SelectItem {
  id: number;
  label: string;
  selected?: boolean;
}

export type FormFilter = Record<string, string>;