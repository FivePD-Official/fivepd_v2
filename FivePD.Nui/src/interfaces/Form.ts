export interface IDateInput {
  name: string
  placeholder: string
}

export interface InputField {
  type: 'text' | 'number' | 'date',
  name: string,
  label: string,
  helpLabel?: string,
  placeholder?: string,
}