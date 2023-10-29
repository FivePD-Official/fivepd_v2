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
  SetAttribute,
  Textarea,
  TextInput
} from '@models';
import { PredefinedAttributes } from "./predefined-attributes";

const predefinedAttributes = [
  PredefinedAttributes.UserId,
  PredefinedAttributes.UserIds,
  PredefinedAttributes.DepartmentId,
  PredefinedAttributes.DepartmentIds,
  PredefinedAttributes.IncidentHistoryId,
  PredefinedAttributes.IncidentHistoryIds,
  PredefinedAttributes.PedHistoryId,
  PredefinedAttributes.PedHistoryIds,
];

export function mapAttributesToFields(attributes: SetAttribute[], entity?: MappedLooseDataEntity, additionalFields: FormInputBase<any>[] = []): FormInputBase<any>[] {
  const fields: FormInputBase<any>[] = [];

  // Add predefined fields
  const singleUser = attributes.find(x => x.Name === PredefinedAttributes.UserId);
  if (singleUser && !singleUser.Hidden) {
    fields.push(new Dropdown({
      key: PredefinedAttributes.UserId,
      label: 'UI.LABEL.USER.SINGULAR',
      required: true,
      dropdownOptions: {
        type: 'single',
        label: 'UI.LABEL.USER.SINGULAR',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[PredefinedAttributes.UserId] : undefined,
      }
    }));
  }

  const multiUser = attributes.find(x => x.Name === PredefinedAttributes.UserIds);
  if (multiUser && !multiUser.Hidden) {
    fields.push(new Dropdown({
      key: PredefinedAttributes.UserIds,
      label: 'UI.LABEL.USER.PLURAL',
      required: true,
      dropdownOptions: {
        type: 'multi',
        label: 'UI.LABEL.USER.PLURAL',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[PredefinedAttributes.UserIds] : undefined,
      }
    }));
  }

  const singleDepartment = attributes.find(x => x.Name === PredefinedAttributes.DepartmentId);
  if (singleDepartment && !singleDepartment.Hidden) {
    fields.push(new Dropdown({
      key: PredefinedAttributes.DepartmentId,
      label: 'UI.LABEL.DEPARTMENT.SINGULAR',
      required: true,
      dropdownOptions: {
        type: 'single',
        label: 'UI.LABEL.DEPARTMENT.SINGULAR',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[PredefinedAttributes.DepartmentId] : undefined,
      }
    }));
  }

  const multiDepartment = attributes.find(x => x.Name === PredefinedAttributes.DepartmentIds);
  if (multiDepartment && !multiDepartment.Hidden) {
    fields.push(new Dropdown({
      key: PredefinedAttributes.DepartmentIds,
      label: 'UI.LABEL.DEPARTMENT.PLURAL',
      required: true,
      dropdownOptions: {
        type: 'multi',
        label: 'UI.LABEL.DEPARTMENT.PLURAL',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[PredefinedAttributes.DepartmentIds] : undefined,
      }
    }));
  }

  const singleIncidentHistory = attributes.find(x => x.Name === PredefinedAttributes.IncidentHistoryId);
  if (singleIncidentHistory && !singleIncidentHistory.Hidden) {
    fields.push(new Dropdown({
      key: PredefinedAttributes.IncidentHistoryId,
      label: 'UI.LABEL.INCIDENT.SINGULAR',
      required: true,
      dropdownOptions: {
        type: 'single',
        label: 'UI.LABEL.INCIDENT.SINGULAR',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[PredefinedAttributes.IncidentHistoryId] : undefined,
      }
    }));
  }

  const multiIncidentHistory = attributes.find(x => x.Name === PredefinedAttributes.IncidentHistoryIds);
  if (multiIncidentHistory && !multiIncidentHistory.Hidden) {
    fields.push(new Dropdown({
      key: PredefinedAttributes.IncidentHistoryIds,
      label: 'UI.LABEL.INCIDENT.PLURAL',
      required: true,
      dropdownOptions: {
        type: 'multi',
        label: 'UI.LABEL.INCIDENT.PLURAL',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[PredefinedAttributes.IncidentHistoryIds] : undefined,
      }
    }));
  }

  const singlePedHistory = attributes.find(x => x.Name === PredefinedAttributes.PedHistoryId);
  if (singlePedHistory && !singlePedHistory.Hidden) {
    fields.push(new Dropdown({
      key: PredefinedAttributes.PedHistoryId,
      label: 'UI.LABEL.CITIZEN.SINGULAR',
      required: true,
      dropdownOptions: {
        type: 'single',
        label: 'UI.LABEL.CITIZEN.SINGULAR',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[PredefinedAttributes.PedHistoryId] : undefined,
      }
    }));
  }

  const multiPedHistory = attributes.find(x => x.Name === PredefinedAttributes.PedHistoryIds);
  if (multiPedHistory && !multiPedHistory.Hidden) {
    fields.push(new Dropdown({
      key: PredefinedAttributes.PedHistoryIds,
      label: 'UI.LABEL.CITIZEN.PLURAL',
      required: true,
      dropdownOptions: {
        type: 'multi',
        label: 'UI.LABEL.CITIZEN.PLURAL',
        size: 'normal',
        variant: 'secondary',
        items: [],
        selectedItemResolver: entity ? selectedKey => selectedKey === entity[PredefinedAttributes.PedHistoryIds] : undefined,
      }
    }));
  }

  const filteredAttributes = attributes
    .filter(x => !x.Hidden)
    .filter(x => !predefinedAttributes.includes(x.Name as PredefinedAttributes));

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
