import { Injectable, SecurityContext } from '@angular/core';
import { defaultLocalization, Localization, LocalizationChangeEvent, Paths } from '@models';
import { NuiEventType } from '@enums';
import { HttpClient } from '@angular/common/http';
import { EventService } from '@services/event/event.service';
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
    this._http.post<Localization>('fivepd:localization:getLocalizationFromNui', {}).subscribe(localization => this.set(localization));
  }

  get(): Localization {
    return this._localization;
  }

  translate(path: Paths<Localization> | string, params: Record<string, string> = {}): string {
    const translation = getKeyFromObjectPath(this._localization, path, path);
    return this.replaceParams(translation, params);
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

  private replaceParams(text: string, params: Record<string, string>): string {
    for (const [key, value] of Object.entries(params)) {
      text = text.replace(`{{${key}}}`, `<b>${value}</b>`);
    }
    return text;
  }
}
