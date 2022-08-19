import { Pipe, PipeTransform } from '@angular/core';
import { LocalizationService } from '../services/localization.service';
import getKeyFromObjectPath from 'lodash-es/get';

@Pipe({
  name: 'translate',
  pure: false,
})
export class TranslatePipe implements PipeTransform {

  constructor(
    private _localization: LocalizationService,
  ) { }

  transform(path: string, ...args: string[]): string {
    return getKeyFromObjectPath(this._localization.get(), path);
  }

}
