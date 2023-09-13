import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ActionResponse, Collation, Message, Page } from "@models";

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor(private readonly _http: HttpClient) {
  }

  get(id: number): Observable<Message> {
    return this._http.post<Message>('fivepd:message:get', {
      Id: id,
    });
  }

  getAllPageable(collation?: Collation): Observable<Page<Message>> {
    return this._http.post<Page<Message>>('fivepd:message:getAllPageable', {
      ...collation
    });
  }

  read(id: number): Observable<void> {
    return this._http.post<void>('fivepd:message:read', {
      Id: id,
    });
  }

  send(request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:message:send', request);
  }
}
