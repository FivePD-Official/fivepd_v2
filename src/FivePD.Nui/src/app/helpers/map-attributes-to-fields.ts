import {
  AttributeDataType,
  Checkbox,
  DateInput,
  Dropdown,
  DropdownOptions,
  FormInputBase,
  FormInputBaseOptions,
  MappedLooseDataEntity,
  NumberInput,
  LooseDataAttribute,
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
      label: 'UI.LABEL.USER.SINGULAR',
      required: singleUser.Required,
      dropdownOptions: {
        type: 'single',
        label: 'UI.LABEL.USER.SINGULAR',
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
      label: 'UI.LABEL.USER.PLURAL',
      required: multiUser.Required,
      dropdownOptions: {
        type: 'multi',
        label: 'UI.LABEL.USER.PLURAL',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[CoreAttributes.UserIds] : undefined,
      }
    }));
  }

  const singleDepartment = attributes.find(x => x.Name === CoreAttributes.DepartmentId);
  if (singleDepartment && !singleDepartment.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.DepartmentId,
      label: 'UI.LABEL.DEPARTMENT.SINGULAR',
      required: singleDepartment.Required,
      dropdownOptions: {
        type: 'single',
        label: 'UI.LABEL.DEPARTMENT.SINGULAR',
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
      label: 'UI.LABEL.DEPARTMENT.PLURAL',
      required: multiDepartment.Required,
      dropdownOptions: {
        type: 'multi',
        label: 'UI.LABEL.DEPARTMENT.PLURAL',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[CoreAttributes.DepartmentIds] : undefined,
      }
    }));
  }

  const singleIncidentHistory = attributes.find(x => x.Name === CoreAttributes.IncidentHistoryId);
  if (singleIncidentHistory && !singleIncidentHistory.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.IncidentHistoryId,
      label: 'UI.LABEL.INCIDENT.SINGULAR',
      required: singleIncidentHistory.Required,
      dropdownOptions: {
        type: 'single',
        label: 'UI.LABEL.INCIDENT.SINGULAR',
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
      label: 'UI.LABEL.INCIDENT.PLURAL',
      required: multiIncidentHistory.Required,
      dropdownOptions: {
        type: 'multi',
        label: 'UI.LABEL.INCIDENT.PLURAL',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[CoreAttributes.IncidentHistoryIds] : undefined,
      }
    }));
  }

  const singlePedHistory = attributes.find(x => x.Name === CoreAttributes.PedHistoryId);
  if (singlePedHistory && !singlePedHistory.Hidden) {
    fields.push(new Dropdown({
      key: CoreAttributes.PedHistoryId,
      label: 'UI.LABEL.CITIZEN.SINGULAR',
      required: singlePedHistory.Required,
      dropdownOptions: {
        type: 'single',
        label: 'UI.LABEL.CITIZEN.SINGULAR',
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
      label: 'UI.LABEL.CITIZEN.PLURAL',
      required: multiPedHistory.Required,
      dropdownOptions: {
        type: 'multi',
        label: 'UI.LABEL.CITIZEN.PLURAL',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[CoreAttributes.PedHistoryIds] : undefined,
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
