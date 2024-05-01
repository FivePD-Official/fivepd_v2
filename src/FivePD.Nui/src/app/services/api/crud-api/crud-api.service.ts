import { Injectable } from '@angular/core';
import { Observable } from "rxjs";
import { ActionResponse, Id } from "@models";
import { BaseApiService } from "@services/api/base-api/base-api.service";
import { parseId } from "../../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export abstract class CrudApiService<T> extends BaseApiService<T> {

  public abstract create(request: object): Observable<ActionResponse>;

  public abstract edit(request: object): Observable<ActionResponse>;

  public abstract delete(id: Id): Observable<ActionResponse>;

  protected _create(eventId: string, request: object) {
    return this.httpClient.post<ActionResponse>(this.createUrl(eventId), request);
  }

  protected _edit(eventId: string, request: object): Observable<ActionResponse> {
    return this.httpClient.post<ActionResponse>(this.createUrl(eventId), request);
  }

  protected _delete(eventId: string, id: Id) {
    return this.httpClient.post<ActionResponse>(this.createUrl(eventId), {
      Id: parseId(id)
    });
  }
}
