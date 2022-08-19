import { Injectable, SecurityContext } from '@angular/core';
import { defaultLocalization, Localization, LocalizationChangeEvent, Paths } from '@interfaces';
import { ClientEvents, NuiEventType } from '@enums';
import { HttpClient } from '@angular/common/http';
import { EventService } from './event.service';
import { DomSanitizer } from '@angular/platform-browser';
import getKeyFromObjectPath from 'lodash-es/get';

@Injectable({
  providedIn: 'root'
})
export class LocalizationService {

  _localization: Localization = defaultLocalization;

  constructor(
    private _http: HttpClient,
    private _event: EventService,
    private sanitizer: DomSanitizer,
  ) {
    this._event.onEvent<LocalizationChangeEvent>(NuiEventType.LocalizationChange).subscribe(localization => this.set(localization.data));
  }

  load(): void {
    this._http.post<Localization>(ClientEvents.GetLocalizationFromNui, {}).subscribe(localization => this.set(localization));
  }

  get(): Localization {
    return this._localization;
  }

  translate(path: Paths<Localization>): string {
    return getKeyFromObjectPath(this._localization, path);
  }

  private async set(localization: Localization): Promise<void> {
    await this.sanitizeObject(localization);
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
