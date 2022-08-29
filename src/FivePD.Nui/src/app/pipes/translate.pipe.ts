import { Pipe, PipeTransform } from '@angular/core';
import { LocalizationService } from '@services/localization/localization.service';
import getKeyFromObjectPath from 'lodash-es/get';
import { Localization, Paths } from '@models';

@Pipe({
  name: 'translate',
  pure: false,
})
export class TranslatePipe implements PipeTransform {

  constructor(
    private _localization: LocalizationService,
  ) { }

  transform(path: Paths<Localization> | string, params: Record<string, string> = {}): string {
    return this._localization.translate(path, params, false);
  }

}
