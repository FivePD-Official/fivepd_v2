import { DateFormat, IDateInput } from '../interfaces';

export { themes } from './colors';
export { hideNui, showNui } from './nui';

export const createBirthdateInputList = (dateFormat: DateFormat): IDateInput[] => {
  let inputs: IDateInput[] = [];
  switch (dateFormat as DateFormat) {
    case DateFormat.DMY: {
      inputs = [
        { name: 'day', placeholder: 'DD' },
        { name: 'month', placeholder: 'MM' },
        { name: 'year', placeholder: 'YYYY' }
      ];
      break;
    }
    case DateFormat.MDY: {
      inputs = [
        { name: 'month', placeholder: 'MM' },
        { name: 'day', placeholder: 'DD' },
        { name: 'year', placeholder: 'YYYY' }
      ];
      break;
    }
    case DateFormat.YMD: {
      inputs = [
        { name: 'year', placeholder: 'YYYY' },
        { name: 'month', placeholder: 'MM' },
        { name: 'day', placeholder: 'DD' }
      ];
      break;
    }
  }
  return inputs;
}