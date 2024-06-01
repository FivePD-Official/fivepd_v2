import { Injectable } from '@angular/core';
import { Collation, Id, Rank } from "@models";
import { CrudApiService } from "@services/api/crud-api/crud-api.service";

export interface RankPageCollationFilters {
  Global: boolean;
  DepartmentId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class RankService extends CrudApiService<Rank> {

  override baseUrl = 'fivepd:rank';

  public override get(id: Id) {
    return this._get('get', id);
  }

  public getAllGlobal() {
    return this.httpClient.post<Rank[]>(this.createUrl('getAllGlobal'), {});
  }

  public override getPage(collation?: Collation<RankPageCollationFilters> | undefined) {
    return this._getPage('getAllPageable', collation);
  }

  public override create(request: object) {
    return this._create('create', request);
  }

  public createGlobal(request: object) {
    return this._create('createGlobal', request);
  }

  public override edit(request: object) {
    return this._edit('edit', request);
  }

  public editGlobal(request: object) {
    return this._edit('editGlobal', request);
  }

  public override delete(id: Id) {
    return this._delete('delete', id);
  }
}
