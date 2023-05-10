import { AttributeDataType, Checkbox, Dropdown, FormInputBase, SetAttribute, TextInput } from '@models';
import { LocalizationService } from '@services/localization/localization.service';

export function createAttributesFormFields(localizationService: LocalizationService, value?: SetAttribute): FormInputBase<any>[] {
  return [
    new TextInput({
      key: 'Name',
      label: 'Name',
      required: true,
      value: value?.Name
    }),
    new Dropdown({
      key: 'DataType',
      label: 'Type',
      required: true,
      readonly: true,
      dropdownOptions: {
        size: 'normal',
        label: localizationService.translate('Nui.Labels.Type'),
        selectedItemResolver: itemKey => value?.DataType === itemKey,
        items: [
          {
            key: AttributeDataType.ShortString,
            label: localizationService.translate('Nui.Labels.ShortString')
          },
          {key: AttributeDataType.LongString, label: localizationService.translate('Nui.Labels.LongString')},
          {key: AttributeDataType.Integer, label: localizationService.translate('Nui.Labels.WholeNumber')},
          {key: AttributeDataType.Bool, label: localizationService.translate('Nui.Labels.Checkbox')},
          {key: AttributeDataType.Date, label: localizationService.translate('Nui.Labels.Date')},
        ]
      }
    }),
    new TextInput({
      key: 'Label',
      label: localizationService.translate('Nui.Labels.Label'),
      required: true,
      value: value?.Label
    }),
    new TextInput({
      key: 'HelpLabel',
      label: localizationService.translate('Nui.Labels.HelpLabel'),
      required: false,
      value: value?.HelpLabel
    }),
    new TextInput({
      key: 'Placeholder',
      label: localizationService.translate('Nui.Labels.Placeholder'),
      required: false,
      value: value?.Placeholder
    }),
    new Checkbox({
      key: 'Required',
      label: localizationService.translate('Nui.Labels.Required'),
      value: value?.Required
    }),
    new Checkbox({
      key: 'ShowInGrid',
      label: localizationService.translate('Nui.Labels.ShowInGrid'),
      value: value?.ShowInGrid
    }),
  ];
}
