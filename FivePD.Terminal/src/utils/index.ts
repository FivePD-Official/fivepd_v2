import { DateFormat, IDateInput } from '../interfaces';

export { hideNui, showNui } from './nui';
export * from './request';
export * from './MenuItem';

export const NuiEventType = Object.freeze({
  Close: 'close',
  Mdt: 'view:mdt',
  DutyMenu: 'view:duty_menu',
  RadialMenu: 'view:radial_menu',
  MenuUp: 'menu:control_up',
  MenuDown: 'menu:control_down',
  MenuLeft: 'menu:control_left',
  MenuRight: 'menu:control_right',
  MenuSelect: 'menu:control_select',
});

export const NuiEvents = Object.freeze({
  GetLocalization: 'fivepd:nui:getLocalization',
  GetLoadout: 'fivepd:nui:getLoadout',
  GetAvailableLocalesFromNui: 'fivepd:nui:getAvailableLocalesFromNui',
  ChangeLocalization: 'fivepd:nui:changeLocalization',
  GetRadialMenus: 'fivepd:nui:getRadialMenus',
});

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