export interface IDateInput {
  name: string;
  placeholder: string;
}

export interface InputField {
  type: 'text' | 'number' | 'date';
  name: string;
  label?: string;
  required: boolean;
  helpLabel?: string;
  placeholder?: string;
  defaultValue?: string | number;
  validator?: (value: any) => boolean;
}

export type FormFilter = Record<string, string>;