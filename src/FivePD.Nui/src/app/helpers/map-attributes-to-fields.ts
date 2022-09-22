import {
  AttributeDataType,
  Checkbox,
  DateInput,
  Dropdown,
  DropdownOptions,
  FormInputBase,
  FormInputBaseOptions,
  LooseDataEntity, MappedLooseDataEntity,
  NumberInput,
  SetAttribute,
  TextInput
} from '@models';

export function mapAttributesToFields(attributes: SetAttribute[], entity?: MappedLooseDataEntity): FormInputBase<any>[] {
  const fields: FormInputBase<any>[] = [];
  for (const attr of attributes.filter(a => !a.Readonly)) {
    const options: FormInputBaseOptions<any> = {
      key: attr.Name,
      label: attr.Label,
      helpLabel: attr.HelpLabel,
      placeholder: attr.Placeholder,
      required: attr.Required,
    };

    if (entity) {
      options.value = entity[attr.Name];
    }

    let inputField: FormInputBase<any> | undefined;
    if (attr.DataType === AttributeDataType.String) {
      inputField = new TextInput(options);
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
    } else if (attr.DataType === AttributeDataType.DateTime) {
      inputField = new DateInput(options);
    }

    if (!inputField) {
      continue;
    }

    fields.push(inputField);
  }

  return fields;
}
