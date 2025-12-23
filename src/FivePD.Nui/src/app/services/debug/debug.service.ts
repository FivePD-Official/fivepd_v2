import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {BaseNuiEvent} from '@models';

@Injectable({
  providedIn: 'root'
})
export class DebugService {

  constructor(
    private readonly http: HttpClient,
  ) {
  }

  debugInfo(): Observable<BaseNuiEvent> {
    return this.http.post<BaseNuiEvent>('fivepd:debug:information', {});
  }
}
