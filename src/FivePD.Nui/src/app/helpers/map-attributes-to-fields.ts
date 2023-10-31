import {
  AttributeDataType,
  Checkbox,
  DateInput,
  Dropdown,
  DropdownOptions,
  FormInputBase,
  FormInputBaseOptions,
  LooseDataAttribute,
  MappedLooseDataEntity,
  NumberInput,
  Textarea,
  TextInput
} from '@models';
import { coreAttributeNames, CoreAttributes } from "./core-attributes";

export function mapAttributesToFields(attributes: LooseDataAttribute[], entity?: MappedLooseDataEntity, additionalFields: FormInputBase<any>[] = []): FormInputBase<any>[] {
  const fields: FormInputBase<any>[] = [];

  // Add predefined fields
  const singleUser = attributes.find(x => x.Name === CoreAttributes.UserId);
  if (singleUser && !singleUser.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.UserId,
      label: singleUser.Label,
      helpLabel: singleUser.HelpLabel,
      readonly: singleUser.Readonly,
      placeholder: singleUser.Placeholder,
      required: singleUser.Required,
      dropdownOptions: {
        type: 'single',
        label: singleUser.Label ?? '-',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[CoreAttributes.UserId] : undefined,
      }
    }));
  }

  const multiUser = attributes.find(x => x.Name === CoreAttributes.UserIds);
  if (multiUser && !multiUser.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.UserIds,
      label: multiUser.Label,
      helpLabel: multiUser.HelpLabel,
      readonly: multiUser.Readonly,
      placeholder: multiUser.Placeholder,
      required: multiUser.Required,
      dropdownOptions: {
        type: 'multi',
        label: multiUser.Label ?? '-',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemsResolver: entity ? selectedKey => (entity[CoreAttributes.UserIds] as any[]).includes(selectedKey) : undefined,
      }
    }));
  }

  const singleDepartment = attributes.find(x => x.Name === CoreAttributes.DepartmentId);
  if (singleDepartment && !singleDepartment.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.DepartmentId,
      label: singleDepartment.Label,
      helpLabel: singleDepartment.HelpLabel,
      readonly: singleDepartment.Readonly,
      placeholder: singleDepartment.Placeholder,
      required: singleDepartment.Required,
      dropdownOptions: {
        type: 'single',
        label: singleDepartment.Label ?? '-',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[CoreAttributes.DepartmentId] : undefined,
      }
    }));
  }

  const multiDepartment = attributes.find(x => x.Name === CoreAttributes.DepartmentIds);
  if (multiDepartment && !multiDepartment.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.DepartmentIds,
      label: multiDepartment.Label,
      helpLabel: multiDepartment.HelpLabel,
      readonly: multiDepartment.Readonly,
      placeholder: multiDepartment.Placeholder,
      required: multiDepartment.Required,
      dropdownOptions: {
        type: 'multi',
        label: multiDepartment.Label ?? '-',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemsResolver: entity ? selectedKey => (entity[CoreAttributes.DepartmentIds as any]).includes(selectedKey) : undefined,
      }
    }));
  }

  const singleIncidentHistory = attributes.find(x => x.Name === CoreAttributes.IncidentHistoryId);
  if (singleIncidentHistory && !singleIncidentHistory.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.IncidentHistoryId,
      label: singleIncidentHistory.Label,
      helpLabel: singleIncidentHistory.HelpLabel,
      readonly: singleIncidentHistory.Readonly,
      placeholder: singleIncidentHistory.Placeholder,
      required: singleIncidentHistory.Required,
      dropdownOptions: {
        type: 'single',
        label: singleIncidentHistory.Label ?? '-',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[CoreAttributes.IncidentHistoryId] : undefined,
      }
    }));
  }

  const multiIncidentHistory = attributes.find(x => x.Name === CoreAttributes.IncidentHistoryIds);
  if (multiIncidentHistory && !multiIncidentHistory.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.IncidentHistoryIds,
      label: multiIncidentHistory.Label,
      helpLabel: multiIncidentHistory.HelpLabel,
      readonly: multiIncidentHistory.Readonly,
      placeholder: multiIncidentHistory.Placeholder,
      required: multiIncidentHistory.Required,
      dropdownOptions: {
        type: 'multi',
        label: multiIncidentHistory.Label ?? '-',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemsResolver: entity ? selectedKey => (entity[CoreAttributes.IncidentHistoryIds as any]).includes(selectedKey) : undefined,
      }
    }));
  }

  const singlePedHistory = attributes.find(x => x.Name === CoreAttributes.PedHistoryId);
  if (singlePedHistory && !singlePedHistory.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.PedHistoryId,
      label: singlePedHistory.Label,
      helpLabel: singlePedHistory.HelpLabel,
      readonly: singlePedHistory.Readonly,
      placeholder: singlePedHistory.Placeholder,
      required: singlePedHistory.Required,
      dropdownOptions: {
        type: 'single',
        label: singlePedHistory.Label ?? '-',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[CoreAttributes.PedHistoryId] : undefined,
      }
    }));
  }

  const multiPedHistory = attributes.find(x => x.Name === CoreAttributes.PedHistoryIds);
  if (multiPedHistory && !multiPedHistory.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.PedHistoryIds,
      label: multiPedHistory.Label,
      helpLabel: multiPedHistory.HelpLabel,
      readonly: multiPedHistory.Readonly,
      placeholder: multiPedHistory.Placeholder,
      required: multiPedHistory.Required,
      dropdownOptions: {
        type: 'multi',
        label: multiPedHistory.Label ?? '-',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemsResolver: entity ? selectedKey => (entity[CoreAttributes.PedHistoryIds as any]).includes(selectedKey) : undefined,
      }
    }));
  }

  const filteredAttributes = attributes
    .filter(x => !x.Hidden)
    .filter(x => !coreAttributeNames.includes(x.Name as CoreAttributes));

  // Add other non readonly fields
  for (const attr of filteredAttributes) {
    const options: FormInputBaseOptions<any> = {
      key: attr.Name,
      label: attr.Label,
      helpLabel: attr.HelpLabel,
      placeholder: attr.Placeholder,
      required: attr.Required,
      readonly: attr.Readonly,
    };

    if (entity) {
      options.value = entity[attr.Name];
    }

    let inputField: FormInputBase<any> | undefined;
    if (attr.DataType === AttributeDataType.ShortString) {
      inputField = new TextInput(options);
    } else if (attr.DataType === AttributeDataType.LongString) {
      inputField = new Textarea(options);
    } else if (attr.DataType === AttributeDataType.Integer) {
      if (attr.Options?.Items) {
        const dropdownOptions: DropdownOptions = {
          items: Object.entries(attr.Options.Items).map(([key, value]) => {
            return {
              key: parseInt(key),
              label: value
            };
          }),
          label: attr.Label ?? '?',
          size: 'normal',
          variant: 'secondary',
        };

        if (entity) {
          dropdownOptions.selectedItemResolver = selectedKey => selectedKey === options.value;
        }

        inputField = new Dropdown({
          ...options,
          dropdownOptions
        });
      } else {
        inputField = new NumberInput(options);
      }
    } else if (attr.DataType === AttributeDataType.Bool) {
      inputField = new Checkbox(options);
    } else if (attr.DataType === AttributeDataType.Date) {
      inputField = new DateInput(options);
    }

    if (!inputField) {
      continue;
    }

    fields.push(inputField);
  }

  fields.push(...additionalFields);

  return fields;
}
