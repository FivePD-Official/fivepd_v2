import { inject, Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Id } from "@models";
import { parseId } from "../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export class AccessControlService {

  private readonly _httpClient = inject(HttpClient);

  allow(id: Id) {
    return this._httpClient.post('fivepd:accessControl:giveUI', {
      id: parseId(id),
    });
  }

  disallow(id: Id) {
    return this._httpClient.post('fivepd:accessControl:removeUI', {
      id: parseId(id),
    });
  }
}
