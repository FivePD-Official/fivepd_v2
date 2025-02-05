import { inject, Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Collation, Id, Page } from "@models";
import { parseId } from "../../../helpers/parse-id";

@Injectable({
  providedIn: 'root'
})
export abstract class BaseApiService<T> {

  protected readonly httpClient = inject(HttpClient);
  protected baseUrl = '';

  public abstract get(id: Id): Observable<T>;

  public abstract getPage(collation?: Collation): Observable<Page<T>>;

  protected _get<K = T>(eventId: string, id: Id, parseToNumber = true) {
    return this.httpClient.post<K>(this.createUrl(eventId), {
      Id: parseToNumber ? parseId(id) : id,
    });
  }

  protected _getPage<K = Page<T>>(eventId: string, collation?: Collation) {
    return this.httpClient.post<K>(this.createUrl(eventId), collation ?? {});
  }

  protected createUrl(eventId: string) {
    return `${this.baseUrl}:${eventId}`;
  }
}
