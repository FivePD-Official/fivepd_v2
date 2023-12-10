import { Component, OnInit } from '@angular/core';
import { Collation, Column, SingleSelectField, FormInputBase, FormSubmitEvent, Page, PedHistory, TextField } from '@models';
import { formatDate } from "@helpers";
import { CoreSetId, FGender } from "@enums";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { Router, RouterLink } from "@angular/router";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { GridComponent } from '../../../../components/grid/grid.component';
import { DatePipe } from '@angular/common';
import { DynamicFormComponent } from '../../../../components/dynamic-form/dynamic-form.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { LooseDataService } from "@services/loose-data/loose-data.service";
import { zip } from "rxjs";

@Component({
  selector: 'app-citizen-database-index',
  templateUrl: './citizen-database-index.component.html',
  styleUrls: ['./citizen-database-index.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, DynamicFormComponent, RouterLink, GridComponent, DatePipe, TranslatePipe]
})
export class CitizenDatabaseIndexComponent implements OnInit {

  citizens?: Page<PedHistory>;
  citizensCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  formFields: FormInputBase<any>[] = [
    new TextField({
      key: 'Firstname',
      label: 'Firstname',
      placeholder: 'John',
    }),
    new TextField({
      key: 'Lastname',
      label: 'Lastname',
      placeholder: 'Doe',
    }),
    new SingleSelectField({
      key: 'Gender',
      label: 'Gender',
      defaultItemLabel: 'All',
      items: [
        {key: FGender.Male, label: 'Male'},
        {key: FGender.Female, label: 'Female'},
      ],
      size: 'normal',
      variant: 'secondary',
    }),
  ];
  citizensColumns: Column<PedHistory>[] = [
    new Column({
      header: 'Firstname',
      accessorKey: 'Firstname',
      show: () => true,
    }),
    new Column({
      header: 'Lastname',
      accessorKey: 'Lastname',
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

  recentlyViewedPeds: PedHistory[] = [];

  constructor(private readonly _pedHistoryService: PedHistoryService,
              private readonly _router: Router,
              public readonly store: Store<GlobalState>,
              private readonly _looseDataService: LooseDataService) {
    store.select('app', 'recentlyViewedPeds').subscribe(peds => this.recentlyViewedPeds = peds);
  }

  ngOnInit(): void {
    this.getAll();
    zip(
      this._looseDataService.updateSetAttributes(CoreSetId.PedTicket),
      this._looseDataService.updateSetAttributes(CoreSetId.PedWarning),
      this._looseDataService.updateSetAttributes(CoreSetId.ArrestReport),
      this._looseDataService.updateSetAttributes(CoreSetId.IncidentReport),
    ).subscribe(() => {});
  }

  citizensRowLinkResolver(item: PedHistory): string {
    return `/mdt/citizen-database/${item.Id}`;
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

  onCitizensCollationChange(collation: Collation): void {
    this.citizensCollation = collation;
    this.getAll();
  }

  protected readonly FGender = FGender;
}
