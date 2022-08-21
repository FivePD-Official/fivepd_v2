import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, switchMap } from 'rxjs';
import { ActionResponse, Department, Division, NameTakenResponse, Page, Rank, User } from '@models';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {

  constructor(
    private _http: HttpClient,
  ) { }

  getDepartment(id: number): Observable<Department> {
    return this._http.post<Department>('fivepd:department:getById', {
      DepartmentId: id,
    });
  }

  getDepartments(): Observable<Page<Department>> {
    return this._http.post<Page<Department>>('fivepd:department:list', {});
  }

  createDepartment(department: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:create', department);
  }

  editDepartment(department: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:edit', department);
  }

  deleteDepartment(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:delete', {
      DepartmentId: id,
    });
  }

  checkIfDepartmentNameTaken(name: string): Observable<NameTakenResponse> {
    return this._http.post<NameTakenResponse>('fivepd:department:checkIfDepartmentNameTaken', {
      Name: name,
    });
  }

  joinDepartment(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:join', {
      DepartmentId: id,
    });
  }

  leaveDepartment(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:leave', {
      DepartmentId: id,
    });
  }

  createRank(departmentId: number, name: Rank['Name'], permission: Rank['Permission']): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:createRank', {
      DepartmentId: departmentId,
      Name: name,
      Permission: permission,
    });
  }

  editRank(departmentId: number, rankId: number, name: Rank['Name'], permission: Rank['Permission']): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:editRank', {
      DepartmentId: departmentId,
      RankId: rankId,
      Name: name,
      Permission: permission,
    });
  }

  deleteRank(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:deleteRank', {
      RankId: id,
    });
  }

  createDivision(division: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:createDivision', division);
  }

  editDivision(division: object): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:renameDivision', division);
  }

  deleteDivision(id: number): Observable<ActionResponse> {
    return this._http.post<ActionResponse>('fivepd:department:createDivision', {
      DivisionId: id,
    });
  }

  getDepartmentUsers(id: number): Observable<Page<User>> {
    return this._http.post<Page<User>>('fivepd:department:getUsers', {
      DepartmentId: id,
      // TODO: Filters
    });
  }

  getRanks(id: number): Observable<Page<Rank>> {
    return this._http.post<Page<Rank>>('fivepd:department:getRanks', {
      DepartmentId: id,
      // TODO: Filters
    });
  }

  getDivisions(id: number): Observable<Page<Division>> {
    return this._http.post<Page<Division>>('fivepd:department:getDivisions', {
      DepartmentId: id,
      // TODO: Filters
    });
  }
}
