import { InjectionToken } from '@angular/core';
import { MappedVariableDataEntity, VariableDataAttribute } from '@models';

export const ENTITY = new InjectionToken<MappedVariableDataEntity>('Entity');
export const ATTRIBUTES = new InjectionToken<VariableDataAttribute[]>('Attributes');
