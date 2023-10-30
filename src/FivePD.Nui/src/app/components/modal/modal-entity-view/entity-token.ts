import { InjectionToken } from '@angular/core';
import { MappedLooseDataEntity, LooseDataAttribute } from '@models';

export const ENTITY = new InjectionToken<MappedLooseDataEntity>('Entity');
export const ATTRIBUTES = new InjectionToken<LooseDataAttribute[]>('Attributes');
