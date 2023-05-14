import { Injectable, SecurityContext } from '@angular/core';
import { LocalizationChangeEvent } from '@models';
import { NuiEventType } from '@enums';
import { HttpClient } from '@angular/common/http';
import { EventService } from '@services/event/event.service';
import { DomSanitizer } from '@angular/platform-browser';
import getKeyFromObjectPath from 'lodash-es/get';
import { replaceParams } from '@helpers';

@Injectable({
  providedIn: 'root'
})
export class LocalizationService {

  _localization: object = {};

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private sanitizer: DomSanitizer,
  ) {
    this._event.onEvent<LocalizationChangeEvent>(NuiEventType.LocalizationChange).subscribe(localization => {
      this.set(JSON.parse(localization.data))
    });
  }

  get(): object {
    return this._localization;
  }

  load() {
    this._http.post('fivepd:localization:getLocalizationFromNui', {}).subscribe(res => {
      this.set(res)
    });
  }

  translate(path: string, params: Record<string, string> = {}, boldParams: boolean = true): string {
    const translation = getKeyFromObjectPath(this._localization, path, path);
    return replaceParams(translation, params, boldParams);
  }

  private async set(localization: object): Promise<void> {
    // TODO FIX await this.sanitizeObject(localization);
    this._localization = localization;
  }

  private sanitizeObject(object: any): Promise<void> {
    return new Promise(resolve => {
      Object.keys(object).forEach(key => {
        if (object[key] && typeof object[key] === 'object') {
          this.sanitizeObject(object[key]);
          return;
        }
        object[key] = this.sanitizer.sanitize(SecurityContext.HTML, object[key]);
        resolve();
      });
    });
  }
}
