import { FormControl, FormGroup, Validators } from "@angular/forms";
import { FormInputBase } from "@models";

export function createDefaultFormGroup<T>(field: FormInputBase<any>, defaultValue: T) {
  const validatorsToAdd = [...field.validators];
  if (field.required) {
    validatorsToAdd.push(Validators.required);
  }

  const asyncValidatorsToAdd = [...field.asyncValidators];
  const control = new FormControl({
    value: field.value ?? defaultValue,
    disabled: field.readonly
  }, validatorsToAdd, asyncValidatorsToAdd);

  return new FormGroup({
    [field.key]: control,
  });
}
