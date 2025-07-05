import { Injectable, SecurityContext } from '@angular/core';
import { LocalizationChangeEvent } from '@models';
import { HttpClient } from '@angular/common/http';
import { EventService } from '@services/event/event.service';
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
  ) {
    this._event.onEvent<LocalizationChangeEvent>('fivepd:player:localizationChange').subscribe(localization => {
      this.set(JSON.parse(localization.Data))
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

  getAvailableLocalizations() {
    return this._http.post<Record<string, string>>('fivepd:localization:getAvailableLocalizations', {});
  }

  translate(path: string, params: Record<string, string> = {}, boldParams: boolean = true): string {
    const translation = getKeyFromObjectPath(this._localization, path, path);
    for (const [key, value] of Object.entries(params ?? {})) {
      params[key] = this.translate(value, {}, false);
    }

    return replaceParams(translation, params, boldParams);
  }

  private async set(localization: object): Promise<void> {
    if (localization) {
      this._localization = localization;
    }
  }
}
