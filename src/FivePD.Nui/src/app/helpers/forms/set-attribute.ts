import {AttributeDataType, CheckboxField, SingleSelectField, FormInputBase, VariableDataAttribute, TextField} from '@models';
import {LocalizationService} from '@services/localization/localization.service';
import {CustomValidators} from "../validators";
import {coreAttributeNames, CoreAttributes} from "../core-attributes";

export function createAttributesFormFields(localizationService: LocalizationService, value?: VariableDataAttribute): FormInputBase<any>[] {
  const fields = [
    new TextField({
      key: 'Name',
      label: localizationService.translate('UI.LABEL.NAME.SINGULAR'),
      required: true,
      value: value?.Name,
      readonly: coreAttributeNames.includes(value?.Name ?? ''),
      validators: [
        CustomValidators.notCoreAttributeName(),
      ],
      show: form => form.value['AttributeType'] === -1,
    }),
    new SingleSelectField({
      key: 'DataType',
      label: localizationService.translate('UI.LABEL.TYPE.SINGULAR'),
      required: true,
      value: value?.DataType,
      readonly: coreAttributeNames.includes(value?.Name ?? ''),
      show: form => form.value['AttributeType'] === -1,
      size: 'normal',
      variant: 'secondary',
      defaultItemLabel: localizationService.translate('UI.LABEL.TYPE.SINGULAR'),
      items: () => [
        {
          key: AttributeDataType.ShortString,
          label: localizationService.translate('UI.LABEL.SHORT_TEXT.SINGULAR')
        },
        {key: AttributeDataType.LongString, label: localizationService.translate('UI.LABEL.LONG_TEXT.SINGULAR')},
        {key: AttributeDataType.Integer, label: localizationService.translate('UI.LABEL.WHOLE_NUMBER.SINGULAR')},
        {key: AttributeDataType.Bool, label: localizationService.translate('LABEL.CHECKBOX.SINGULAR')},
        {key: AttributeDataType.Date, label: localizationService.translate('LABEL.DATE.TITLE.SINGULAR')},
      ]
    }),
    new TextField({
      key: 'Label',
      label: localizationService.translate('UI.LABEL.LABEL.SINGULAR'),
      required: true,
      value: value?.Label,
    }),
    new TextField({
      key: 'HelpLabel',
      label: localizationService.translate('UI.LABEL.HELP_LABEL.SINGULAR'),
      required: false,
      value: value?.HelpLabel,
    }),
    new TextField({
      key: 'Placeholder',
      label: localizationService.translate('UI.LABEL.PLACEHOLDER'),
      required: false,
      value: value?.Placeholder,
    }),
    new CheckboxField({
      key: 'Hidden',
      label: localizationService.translate('UI.LABEL.HIDDEN.TITLE'),
      helpLabel: localizationService.translate('UI.LABEL.HIDDEN.HELP_LABEL'),
      value: value?.Hidden,
    }),
    new CheckboxField({
      key: 'Readonly',
      label: localizationService.translate('UI.LABEL.READONLY.TITLE'),
      helpLabel: localizationService.translate('UI.LABEL.READONLY.HELP_LABEL'),
      value: value?.Readonly,
    }),
    new CheckboxField({
      key: 'Required',
      label: localizationService.translate('UI.LABEL.REQUIRED'),
      value: value?.Required,
    }),
    new CheckboxField({
      key: 'ShowInGrid',
      label: localizationService.translate('UI.LABEL.SHOW_IN_GRID.TITLE'),
      value: value?.ShowInGrid
    }),
  ];

  if (!value) {
    fields.unshift(new SingleSelectField({
      key: 'AttributeType',
      label: localizationService.translate('LABEL.ATTRIBUTE.TYPE.TITLE'),
      helpLabel: localizationService.translate('LABEL.ATTRIBUTE.TYPE.HELP_LABEL'),
      size: 'normal',
      variant: 'secondary',
      defaultItemLabel: localizationService.translate('LABEL.CUSTOM.TITLE'),
      items: () => [
        {key: CoreAttributes.SubmitDate, label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.SUBMIT_DATE')},
        {key: CoreAttributes.UserId, label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.USER_ID.SINGULAR')},
        {key: CoreAttributes.UserIds, label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.USER_ID.PLURAL')},
        {
          key: CoreAttributes.DepartmentId,
          label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.DEPARTMENT_ID.SINGULAR')
        },
        {
          key: CoreAttributes.DepartmentIds,
          label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.DEPARTMENT_ID.PLURAL')
        },
        {
          key: CoreAttributes.PedHistoryId,
          label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.PED_HISTORY_ID.SINGULAR')
        },
        {
          key: CoreAttributes.PedHistoryIds,
          label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.PED_HISTORY_IDS.PLURAL')
        },
        {
          key: CoreAttributes.IncidentHistoryId,
          label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.INCIDENT_HISTORY_ID.SINGULAR')
        },
        {
          key: CoreAttributes.IncidentHistoryIds,
          label: localizationService.translate('LABEL.ATTRIBUTE.TYPES.INCIDENT_HISTORY_IDS.PLURAL')
        },
      ],
    }));
  }

  return fields;
}
