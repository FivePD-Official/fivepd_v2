import { Injectable } from '@angular/core';
import { Observable } from "rxjs";
import { ActionResponse, Collation, Id, Message } from "@models";
import { BaseApiService } from "@services/api/base-api/base-api.service";

@Injectable({
  providedIn: 'root'
})
export class MessageService extends BaseApiService<Message> {

  override baseUrl = 'fivepd:message';

  get(id: Id) {
    return this._get('get', id);
  }

  getPage(collation: Collation) {
    return this._getPage('getAllPageable', collation);
  }

  read(id: number): Observable<void> {
    return this.httpClient.post<void>(this.createUrl('read'), {
      Id: id,
    });
  }

  send(request: object): Observable<ActionResponse> {
    return this.httpClient.post<ActionResponse>(this.createUrl('send'), request);
  }
}
