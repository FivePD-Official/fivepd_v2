import {
  AttributeDataType,
  CheckboxField,
  DateField,
  SingleSelectField,
  FormInputBase,
  FormInputBaseOptions,
  VariableDataAttribute,
  MappedVariableDataEntity,
  NumberField,
  TextareaField,
  TextField, MultiSelectField, SingleSelectFieldOptions
} from '@models';
import { coreAttributeNames, CoreAttributes } from "./core-attributes";
import { MultiSelectFieldComponent } from "../components/form-inputs/multi-select-field/multi-select-field.component";

export function mapAttributesToFields(attributes: VariableDataAttribute[], entity?: MappedVariableDataEntity, additionalFields: FormInputBase<any>[] = []): FormInputBase<any>[] {
  const fields: FormInputBase<any>[] = [];

  // Add predefined fields
  const singleUser = attributes.find(x => x.Name === CoreAttributes.UserId);
  if (singleUser && !singleUser.Hidden) {
    fields.push(new SingleSelectField({
      key: CoreAttributes.UserId,
      label: singleUser.Label,
      helpLabel: singleUser.HelpLabel,
      readonly: singleUser.Readonly,
      placeholder: singleUser.Placeholder,
      required: singleUser.Required,
      defaultItemLabel: singleUser.Label ?? '-',
      size: 'normal',
      variant: 'secondary',
      items: () => [],
      value: entity ? entity[CoreAttributes.UserId] : undefined,
    }));
  }

  const multiUser = attributes.find(x => x.Name === CoreAttributes.UserIds);
  if (multiUser && !multiUser.Hidden) {
    fields.push(new MultiSelectField({
      key: CoreAttributes.UserIds,
      label: multiUser.Label,
      helpLabel: multiUser.HelpLabel,
      readonly: multiUser.Readonly,
      placeholder: multiUser.Placeholder,
      required: multiUser.Required,
      defaultItemLabel: multiUser.Label ?? '-',
      size: 'normal',
      variant: 'secondary',
      items: () => [],
      value: entity ? (entity[CoreAttributes.UserIds] as any[]) : undefined,
    }));
  }

  const singleDepartment = attributes.find(x => x.Name === CoreAttributes.DepartmentId);
  if (singleDepartment && !singleDepartment.Hidden) {
    fields.push(new SingleSelectField({
      key: CoreAttributes.DepartmentId,
      label: singleDepartment.Label,
      helpLabel: singleDepartment.HelpLabel,
      readonly: singleDepartment.Readonly,
      placeholder: singleDepartment.Placeholder,
      required: singleDepartment.Required,
      defaultItemLabel: singleDepartment.Label ?? '-',
      size: 'normal',
      variant: 'secondary',
      items: () => [],
      value: entity ? entity[CoreAttributes.DepartmentId] : undefined,
    }));
  }

  const multiDepartment = attributes.find(x => x.Name === CoreAttributes.DepartmentIds);
  if (multiDepartment && !multiDepartment.Hidden) {
    fields.push(new MultiSelectField({
      key: CoreAttributes.DepartmentIds,
      label: multiDepartment.Label,
      helpLabel: multiDepartment.HelpLabel,
      readonly: multiDepartment.Readonly,
      placeholder: multiDepartment.Placeholder,
      required: multiDepartment.Required,
      defaultItemLabel: multiDepartment.Label ?? '-',
      size: 'normal',
      variant: 'secondary',
      items: () => [],
      value: entity ? (entity[CoreAttributes.DepartmentIds] as any[]) : undefined,
    }));
  }

  const singleIncidentHistory = attributes.find(x => x.Name === CoreAttributes.IncidentHistoryId);
  if (singleIncidentHistory && !singleIncidentHistory.Hidden) {
    fields.push(new SingleSelectField({
      key: CoreAttributes.IncidentHistoryId,
      label: singleIncidentHistory.Label,
      helpLabel: singleIncidentHistory.HelpLabel,
      readonly: singleIncidentHistory.Readonly,
      placeholder: singleIncidentHistory.Placeholder,
      required: singleIncidentHistory.Required,
      defaultItemLabel: singleIncidentHistory.Label ?? '-',
      size: 'normal',
      variant: 'secondary',
      items: () => [],
      value: entity ? entity[CoreAttributes.IncidentHistoryId] : undefined,
    }));
  }

  const multiIncidentHistory = attributes.find(x => x.Name === CoreAttributes.IncidentHistoryIds);
  if (multiIncidentHistory && !multiIncidentHistory.Hidden) {
    fields.push(new MultiSelectField({
      key: CoreAttributes.IncidentHistoryIds,
      label: multiIncidentHistory.Label,
      helpLabel: multiIncidentHistory.HelpLabel,
      readonly: multiIncidentHistory.Readonly,
      placeholder: multiIncidentHistory.Placeholder,
      required: multiIncidentHistory.Required,
      defaultItemLabel: multiIncidentHistory.Label ?? '-',
      size: 'normal',
      variant: 'secondary',
      items: () => [],
      value: entity ? (entity[CoreAttributes.IncidentHistoryIds] as any[]) : undefined,
    }));
  }

  const singlePedHistory = attributes.find(x => x.Name === CoreAttributes.PedHistoryId);
  if (singlePedHistory && !singlePedHistory.Hidden) {
    fields.push(new SingleSelectField({
      key: CoreAttributes.PedHistoryId,
      label: singlePedHistory.Label,
      helpLabel: singlePedHistory.HelpLabel,
      readonly: singlePedHistory.Readonly,
      placeholder: singlePedHistory.Placeholder,
      required: singlePedHistory.Required,
      defaultItemLabel: singlePedHistory.Label ?? '-',
      size: 'normal',
      variant: 'secondary',
      items: () => [],
      value: entity ? entity[CoreAttributes.PedHistoryId] : undefined,
    }));
  }

  const multiPedHistory = attributes.find(x => x.Name === CoreAttributes.PedHistoryIds);
  if (multiPedHistory && !multiPedHistory.Hidden) {
    fields.push(new MultiSelectField({
      key: CoreAttributes.PedHistoryIds,
      label: multiPedHistory.Label,
      helpLabel: multiPedHistory.HelpLabel,
      readonly: multiPedHistory.Readonly,
      placeholder: multiPedHistory.Placeholder,
      required: multiPedHistory.Required,
      defaultItemLabel: multiPedHistory.Label ?? '-',
      size: 'normal',
      variant: 'secondary',
      items: () => [],
      value: entity ? (entity[CoreAttributes.PedHistoryIds] as any[]) : undefined,
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
      inputField = new TextField(options);
    } else if (attr.DataType === AttributeDataType.LongString) {
      inputField = new TextareaField(options);
    } else if (attr.DataType === AttributeDataType.Integer) {
      if (attr.Options?.Items) {
        const dropdownOptions: FormInputBase<number> & SingleSelectFieldOptions = {
          items: Object.entries(attr.Options.Items).map(([key, value]) => {
            return {
              key: parseInt(key),
              label: value
            };
          }),
          defaultItemLabel: attr.Label ?? '?',
          size: 'normal',
          variant: 'secondary',
          value: entity ? options.value : -1,
          ...options,
        } as any;

        inputField = new SingleSelectField({
          ...options,
          ...dropdownOptions,
        });
      } else {
        inputField = new NumberField(options);
      }
    } else if (attr.DataType === AttributeDataType.Bool) {
      inputField = new CheckboxField(options);
    } else if (attr.DataType === AttributeDataType.Date) {
      inputField = new DateField(options);
    }

    if (!inputField) {
      continue;
    }

    fields.push(inputField);
  }

  fields.push(...additionalFields);

  return fields;
}
