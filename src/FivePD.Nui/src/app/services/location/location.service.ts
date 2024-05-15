import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActionResponse, Collation, Id, Location, LocationType, Page } from '@models';
import { parseId } from "../../helpers/parse-id";
import { CrudApiService } from "@services/api/crud-api/crud-api.service";

@Injectable({
  providedIn: 'root'
})
export class LocationService extends CrudApiService<Location> {

  override baseUrl = 'fivepd:ui:location';

  get(id: Id){
    return this._get('NOT_IMPLEMENTED', id); // TODO
  }

  getPage(collation: Collation<{ Type: LocationType }>) {
    return this._getPage('getAllPageable', collation);
  }

  create(request: object) {
    return this._create('create', request);
  }

  edit(request: object) {
    return this._edit('NOT_IMPLEMENTED', request); // TODO
  }

  delete(id: Id) {
    return this._delete('delete', id);
  }
}
