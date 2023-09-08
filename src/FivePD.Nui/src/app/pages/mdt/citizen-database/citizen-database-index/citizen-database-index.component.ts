import { Component, OnInit } from '@angular/core';
import {
  Checkbox,
  Collation,
  Column,
  Dropdown,
  FormInputBase,
  FormSubmitEvent,
  Page,
  PedHistory,
  TextInput
} from '@models';
import { formatDate } from "@helpers";
import { FGender } from "@enums";
import { PedHistoryService } from "@services/ped-history/ped-history.service";

@Component({
  selector: 'app-citizen-database-index',
  templateUrl: './citizen-database-index.component.html',
  styleUrls: ['./citizen-database-index.component.sass']
})
export class CitizenDatabaseIndexComponent implements OnInit {

  citizens?: Page<PedHistory>;
  citizensCollation: Collation = {
    Page: 1,
    Filters: {},
  };
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
    new Dropdown({
      key: 'Gender',
      label: 'Gender',
      dropdownOptions: {
        label: 'All',
        items: [
          {key: FGender.Male, label: 'Male'},
          {key: FGender.Female, label: 'Female'},
        ],
        size: 'normal',
        variant: 'secondary',
      }
    }),
  ];
  citizensColumns: Column<PedHistory>[] = [
    new Column({
      header: 'Name',
      valueResolver: item => `${item.Firstname} ${item.Lastname}`,
      show: () => true,
    }),
    new Column({
      header: 'Birthdate',
      valueResolver: item => formatDate(new Date(item.Birthdate), 'none'),
      show: () => true,
    }),
    new Column({
      header: 'Gender',
      valueResolver: item => item.Gender === FGender.Male ? 'Male' : 'Female',
      show: () => true,
    }),
  ];

  constructor(private readonly _pedHistoryService: PedHistoryService) {
  }

  ngOnInit(): void {
    this.getAll();
  }

  onSearchSubmit(event: FormSubmitEvent): void {
    this.citizensCollation.Filters = event.value as Record<string, string>;
    this.getAll();
  }

  getAll(): void {
    this._pedHistoryService.getAll(this.citizensCollation).subscribe(res => {
      this.citizens = res;
    });
  }

}
