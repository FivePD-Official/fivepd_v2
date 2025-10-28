import { Component, OnInit } from '@angular/core';
import { LocalizationService } from "@services/localization/localization.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { IncidentHistoryService } from "@services/incident-history/incident-history.service";
import { Collation, Column, IncidentHistory, Page } from "@models";
import { FIncidentType } from "@enums";
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { GridComponent } from '../../../components/grid/grid.component';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';

@Component({
    selector: 'app-incident-history',
    templateUrl: './incident-history.component.html',
    styleUrls: ['./incident-history.component.sass'],
    imports: [PageHeaderComponent, GridComponent, TranslatePipe]
})
export class IncidentHistoryComponent implements OnInit {

  incidentHistoryPage?: Page<IncidentHistory>;
  incidentHistoryPageCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  incidentHistoryPageColumnDef: Column<IncidentHistory>[] = [
    new Column({
      header: 'UI.LABEL.ID.SHORT.SINGULAR',
      accessorKey: 'Id',
      filter: {
        label: 'UI.LABEL.SEARCH_BY.ID',
      }
    }),
    new Column({
      header: 'UI.LABEL.TITLE.SINGULAR',
      accessorKey: 'Title',
      filter: {
        label: 'UI.LABEL.SEARCH_BY.TITLE',
      },
    }),
    new Column({
      header: 'UI.LABEL.TYPE.SINGULAR',
      accessorKey: 'Type',
      valueResolver: item => this._localizationService.translate(item.Type === FIncidentType.Callout ? 'LABEL.CALLOUT.SINGULAR' : 'UI.LABEL.TRAFFIC_STOP.SINGULAR'),
      filter: {
        label: this._localizationService.translate('UI.LABEL.SEARCH_BY.TYPE'),
        dropdown: [
          {key: FIncidentType.Callout, label: this._localizationService.translate('LABEL.CALLOUT.SINGULAR')},
          {key: FIncidentType.TrafficStop, label: this._localizationService.translate('UI.LABEL.TRAFFIC_STOP.SINGULAR')},
        ]
      }
    }),
    // new Column({
    //   header: 'Actions',
    //   show: () => true,
    //   actions: [
    //     {
    //       type: 'button',
    //       label: this._localizationService.translate('ACTION.VIEW.TITLE'),
    //       show: () => true,
    //       onClick: entity => {
    //         this._modalService.open(ModalEntityViewComponent, {
    //           title: 'LABEL.ARREST_REPORT.SINGULAR',
    //           columns: 2,
    //         }, new Map().set(ENTITY, entity).set(ATTRIBUTES, wrapper.attributes));
    //       }
    //     },
    //   ],
    // })
  ];

  constructor(
    private readonly _localizationService: LocalizationService,
    private readonly _incidentHistoryService: IncidentHistoryService,
    private readonly _modalService: ModalService,
  ) {
  }

  ngOnInit(): void {
    this.getIncidentHistoryPage();
  }

  getIncidentHistoryPage(): void {
    this._incidentHistoryService.getPage(this.incidentHistoryPageCollation)
      .subscribe(page => this.incidentHistoryPage = page);
  }

  onIncidentHistoryPageCollationChange(collation: Collation): void {
    this.incidentHistoryPageCollation = collation;
    this.getIncidentHistoryPage();
  }

}
