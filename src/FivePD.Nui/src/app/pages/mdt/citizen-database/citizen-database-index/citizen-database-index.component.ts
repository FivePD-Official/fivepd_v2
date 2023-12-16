import { Component, inject, OnInit } from '@angular/core';
import { Collation, Column, FormInputBase, PedHistory, SingleSelectField, TextInput } from '@models';
import { formatDate } from "@helpers";
import { CoreSetId, FGender } from "@enums";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { RouterLink } from "@angular/router";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { GridComponent } from '../../../../components/grid/grid.component';
import { AsyncPipe, DatePipe } from '@angular/common';
import { DynamicFormComponent } from '../../../../components/dynamic-form/dynamic-form.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { LooseDataService } from "@services/loose-data/loose-data.service";
import { BehaviorSubject, map, switchMap, zip } from "rxjs";

@Component({
  selector: 'app-citizen-database-index',
  templateUrl: './citizen-database-index.component.html',
  styleUrls: ['./citizen-database-index.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, DynamicFormComponent, RouterLink, GridComponent, DatePipe, TranslatePipe, AsyncPipe]
})
export class CitizenDatabaseIndexComponent implements OnInit {

  private readonly _pedHistoryService = inject(PedHistoryService);
  private readonly _store = inject<Store<GlobalState>>(Store<GlobalState>);
  private readonly _looseDataService = inject(LooseDataService);

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly citizens$ = this.collation$.pipe(
    switchMap(collation => this._pedHistoryService.getAll(collation)),
  );

  readonly recentlyViewedPeds = this._store.select('app', 'recentlyViewedPeds');
  readonly recentlyViewedPedsCount = this.recentlyViewedPeds.pipe(map(x => x.length));

  readonly formFields: FormInputBase<any>[] = [
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
  readonly citizensColumns: Column<PedHistory>[] = [
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

  ngOnInit(): void {
    zip(
      this._looseDataService.updateSetAttributes(CoreSetId.PedTicket),
      this._looseDataService.updateSetAttributes(CoreSetId.PedWarning),
      this._looseDataService.updateSetAttributes(CoreSetId.ArrestReport),
      this._looseDataService.updateSetAttributes(CoreSetId.IncidentReport),
    ).subscribe();
  }

  rowLinkResolver(item: PedHistory): string {
    return `/mdt/citizen-database/${item.Id}`;
  }

  protected readonly FGender = FGender;
}
