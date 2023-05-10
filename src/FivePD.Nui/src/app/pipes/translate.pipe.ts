import { Pipe, PipeTransform } from '@angular/core';
import { LocalizationService } from '@services/localization/localization.service';

@Pipe({
  name: 'translate',
  pure: false,
})
export class TranslatePipe implements PipeTransform {

  constructor(
    private _localization: LocalizationService,
  ) { }

  transform(path: string, params: Record<string, string> = {}): string {
    return this._localization.translate(path, params, false);
  }

}
