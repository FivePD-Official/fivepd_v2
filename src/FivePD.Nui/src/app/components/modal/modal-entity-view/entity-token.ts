import { InjectionToken } from '@angular/core';
import { MappedLooseDataEntity, SetAttribute } from '@models';

export const ENTITY = new InjectionToken<MappedLooseDataEntity>('Entity');
export const ATTRIBUTES = new InjectionToken<SetAttribute[]>('Attributes');
