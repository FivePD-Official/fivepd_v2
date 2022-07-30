export interface IDateInput {
  name: string
  placeholder: string
}

export interface InputField {
  type: 'text' | 'number' | 'date' | 'checkboxGroup',
  name: string,
  label?: string,
  required: boolean,
  helpLabel?: string,
  placeholder?: string,
  defaultValue?: string | number,
  validator?: (value: any) => boolean,
  checkboxes?: {
    id: string,
    label: string,
    defaultValue?: boolean,
  }[],
}

export type FormFilter = Record<string, string>;