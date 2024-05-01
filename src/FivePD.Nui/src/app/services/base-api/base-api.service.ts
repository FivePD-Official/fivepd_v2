import { inject, Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { ActionResponse, Collation, Id, Page } from "@models";
import { parseId } from "../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export abstract class BaseApiService<T> {

  private readonly _http = inject(HttpClient);

  protected baseUrl = '';

  public abstract get(id: Id): Observable<T>;

  public abstract getPage(collation?: Collation): Observable<Page<T>>;

  public abstract create(request: object): Observable<ActionResponse>;

  public abstract edit(request: object): Observable<ActionResponse>;

  public abstract delete(id: Id): Observable<ActionResponse>;

  protected _get(eventId: string, id: Id) {
    return this._http.post<T>(this.createUrl(eventId), {
      Id: parseId(id),
    });
  }

  protected _getPage(eventId: string, collation?: Collation) {
    return this._http.post<Page<T>>(this.createUrl(eventId), collation);
  }

  protected _create(eventId: string, request: object) {
    return this._http.post<ActionResponse>(this.createUrl(eventId), request);
  }

  protected _edit(eventId: string, request: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>(this.createUrl(eventId), request);
  }

  protected _delete(eventId: string, id: Id) {
    return this._http.post<ActionResponse>(this.createUrl(eventId), {
      Id: parseId(id)
    });
  }

  private createUrl(eventId: string) {
    return `${this.baseUrl}:${eventId}`;
  }
}
