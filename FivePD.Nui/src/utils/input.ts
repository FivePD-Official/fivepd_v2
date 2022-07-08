import type { IDateInput } from '@interfaces';
import { DateFormat } from '@enums';

export const createBirthdateInputList = (dateFormat: DateFormat): IDateInput[] => {
  let inputs: IDateInput[] = [];
  switch (dateFormat as DateFormat) {
    case DateFormat.DMY: {
      inputs = [
        { name: 'Day', placeholder: 'DD' },
        { name: 'Month', placeholder: 'MM' },
        { name: 'Year', placeholder: 'YYYY' }
      ];
      break;
    }
    case DateFormat.MDY: {
      inputs = [
        { name: 'Month', placeholder: 'MM' },
        { name: 'Day', placeholder: 'DD' },
        { name: 'Year', placeholder: 'YYYY' }
      ];
      break;
    }
    case DateFormat.YMD: {
      inputs = [
        { name: 'Year', placeholder: 'YYYY' },
        { name: 'Month', placeholder: 'MM' },
        { name: 'Day', placeholder: 'DD' }
      ];
      break;
    }
  }
  return inputs;
}

export type FormFields = null | { [key: string]: any };

export const formatFormEvent = (event: any): FormFields => {

  if (!event?.target?.elements) {
    return null;
  }

  const fields: any = {};

  for (const e of event.target.elements) {
    if (!e.name || !e.value) {
      continue;
    }

    if (['Day', 'Month', 'Year'].includes(e.name)) {
      if (!fields.hasOwnProperty('Birthdate')) {
        fields['Birthdate'] = {};
      }
      fields['Birthdate'][e.name] = e.value;
    } else {
      fields[e.name] = e.type === 'number' ? e.valueAsNumber : e.value;
    }
  }

  return fields;
}