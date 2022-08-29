import { Component, OnInit } from '@angular/core';
import { Column, FormInputBase, FormSubmitEvent, Page, TextInput } from '@models';
import { HttpClient } from '@angular/common/http';

interface Ped {
  NetworkId: number;
  Firstname: string;
  Lastname: string;
  Gender: number;
  Birthdate: {
    Year: number;
    Month: number;
    Day: number;
  }
}

@Component({
  selector: 'app-citizen-database-index',
  templateUrl: './citizen-database-index.component.html',
  styleUrls: ['./citizen-database-index.component.sass']
})
export class CitizenDatabaseIndexComponent implements OnInit {

  citizens?: Page<Ped>;
  formFields: FormInputBase<any>[] = [
    new TextInput({
      key: 'Firstname',
      label: 'Firstname',
      placeholder: 'John',
    }),
    new TextInput({
      key: 'Lastname',
      label: 'Lastname',
      placeholder: 'Doe',
    }),
    // TODO: Date input
  ];
  citizensColumns: Column<Ped>[] = [
    new Column<Ped>({
      header: 'Name',
      valueResolver: item => `${item.Firstname} ${item.Lastname}`,
      show: () => true,
    }),
    new Column<Ped>({
      header: 'Birthdate',
      valueResolver: item => `${item.Birthdate.Month}/${item.Birthdate.Day}/${item.Birthdate.Year}`,
      show: () => true,
    }),
  ];

  constructor(private _http: HttpClient) { }

  ngOnInit(): void {
  }

  onSearchSubmit(event: FormSubmitEvent): void {
    // TODO: Entity service
    this._http.post<Page<Ped>>('fivepd:ped:search', event.value).subscribe(res => {
      this.citizens = res;
    });
  }

}
