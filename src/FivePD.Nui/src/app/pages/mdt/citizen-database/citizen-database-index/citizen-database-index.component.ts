import { Component, inject, OnInit } from '@angular/core';
import { Collation, Column, FormInputBase, PedData, SingleSelectField, TextField } from '@models';
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
import { BehaviorSubject, map, Observable, switchMap, zip } from "rxjs";
import { LocalizationService } from "@services/localization/localization.service";
import { VariableDataService } from "@services/variable-data/variable-data.service";

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
  private readonly _variableDataService = inject(VariableDataService);
  private readonly _localizationService = inject(LocalizationService);

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly citizens$ = this.collation$.pipe(
    switchMap(collation => this._pedHistoryService.getPage(collation)),
  );

  readonly recentlyViewedPeds: Observable<PedData[]> = this._store.select('app', 'recentlyViewedPeds');
  readonly recentlyViewedPedsCount = this.recentlyViewedPeds.pipe(map(x => x.length));

  readonly formFields: FormInputBase<any>[] = [
    new TextField({
      key: 'Firstname',
      label: 'LABEL.FIRSTNAME',
      placeholder: 'John',
    }),
    new TextField({
      key: 'Lastname',
      label: 'LABEL.LASTNAME',
      placeholder: 'Doe',
    }),
    new SingleSelectField({
      key: 'Gender',
      label: 'LABEL.GENDER.TITLE',
      defaultItemLabel: this._localizationService.translate('UI.LABEL.ALL'),
      items: [
        {key: FGender.Male, label: this._localizationService.translate('LABEL.GENDER.MALE')},
        {key: FGender.Female, label: this._localizationService.translate('LABEL.GENDER.FEMALE')},
      ],
      size: 'normal',
      variant: 'secondary',
    }),
  ];
  readonly citizensColumns: Column<PedData>[] = [
    new Column({
      header: 'LABEL.FIRSTNAME',
      accessorKey: 'Firstname',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.LASTNAME',
      accessorKey: 'Lastname',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.BIRTHDATE',
      valueResolver: item => formatDate(new Date(item.Birthdate), 'none'),
      show: () => true,
    }),
    new Column({
      header: 'LABEL.GENDER.TITLE',
      valueResolver: item => this._localizationService.translate(item.Gender === FGender.Male ? 'LABEL.GENDER.MALE' : 'LABEL.GENDER.FEMALE'),
      show: () => true,
    }),
  ];

  ngOnInit(): void {
    zip(
      this._variableDataService.updateSetAttributes(CoreSetId.PedTicket),
      this._variableDataService.updateSetAttributes(CoreSetId.PedWarning),
      this._variableDataService.updateSetAttributes(CoreSetId.ArrestReport),
      this._variableDataService.updateSetAttributes(CoreSetId.IncidentReport),
    ).subscribe();
  }

  rowLinkResolver(item: PedData): string {
    return `/mdt/citizen-database/${item.Id}`;
  }

  protected readonly FGender = FGender;
}
