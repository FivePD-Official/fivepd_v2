import { AttributeDataType, Checkbox, Dropdown, FormInputBase, SetAttribute, TextInput } from '@models';
import { LocalizationService } from '@services/localization/localization.service';

export function createAttributesFormFields(localizationService: LocalizationService, value?: SetAttribute): FormInputBase<any>[] {
  return [
    new TextInput({
      key: 'Name',
      label: localizationService.translate('UI.LABEL.NAME.SINGULAR'),
      required: true,
      value: value?.Name
    }),
    new Dropdown({
      key: 'DataType',
      label: localizationService.translate('UI.LABEL.TYPE.SINGULAR'),
      required: true,
      readonly: true,
      dropdownOptions: {
        size: 'normal',
        label: localizationService.translate('UI.LABEL.TYPE.SINGULAR'),
        selectedItemResolver: itemKey => value?.DataType === itemKey,
        items: [
          {
            key: AttributeDataType.ShortString,
            label: localizationService.translate('UI.LABEL.SHORT_TEXT.SINGULAR')
          },
          {key: AttributeDataType.LongString, label: localizationService.translate('UI.LABEL.LONG_TEXT.SINGULAR')},
          {key: AttributeDataType.Integer, label: localizationService.translate('UI.LABEL.WHOLE_NUMBER.SINGULAR')},
          {key: AttributeDataType.Bool, label: localizationService.translate('UI.LABEL.CHECKBOX.SINGULAR')},
          {key: AttributeDataType.Date, label: localizationService.translate('UI.LABEL.DATE.SINGULAR')},
        ]
      }
    }),
    new TextInput({
      key: 'Label',
      label: localizationService.translate('UI.LABEL.LABEL.SINGULAR'),
      required: true,
      value: value?.Label
    }),
    new TextInput({
      key: 'HelpLabel',
      label: localizationService.translate('UI.LABEL.HELP_LABEL.SINGULAR'),
      required: false,
      value: value?.HelpLabel
    }),
    new TextInput({
      key: 'Placeholder',
      label: localizationService.translate('UI.LABEL.PLACEHOLDER'),
      required: false,
      value: value?.Placeholder
    }),
    new Checkbox({
      key: 'Required',
      label: localizationService.translate('UI.LABEL.REQUIRED'),
      value: value?.Required
    }),
    new Checkbox({
      key: 'ShowInGrid',
      label: localizationService.translate('UI.LABEL.SHOW_IN_GRID.TITLE'),
      value: value?.ShowInGrid
    }),
  ];
}
