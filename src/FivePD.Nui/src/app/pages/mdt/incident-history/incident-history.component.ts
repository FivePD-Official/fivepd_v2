import { Component } from '@angular/core';
import { LocalizationService } from "@services/localization/localization.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { IncidentHistoryService } from "@services/incident-history/incident-history.service";
import { Collation, Column, IncidentHistory, Page } from "@models";
import { FIncidentType } from "@enums";

@Component({
  selector: 'app-incident-history',
  templateUrl: './incident-history.component.html',
  styleUrls: ['./incident-history.component.sass']
})
export class IncidentHistoryComponent {

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
      valueResolver: item => this._localizationService.translate(item.Type === FIncidentType.Callout ? 'UI.LABEL.CALLOUT.SINGULAR' : 'UI.LABEL.TRAFFIC_STOP.PLURAL'),
      // TODO: filter dropdown
    }),
    // new Column({
    //   header: 'Actions',
    //   show: () => true,
    //   actions: [
    //     {
    //       type: 'button',
    //       label: this._localizationService.translate('UI.ACTION.VIEW'),
    //       show: () => true,
    //       onClick: entity => {
    //         this._modalService.open(ModalEntityViewComponent, {
    //           title: 'UI.LABEL.ARREST_REPORT.SINGULAR',
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
    this._incidentHistoryService.getAll(this.incidentHistoryPageCollation)
      .subscribe(page => this.incidentHistoryPage = page);
  }

  onIncidentHistoryPageCollationChange(collation: Collation): void {
    this.incidentHistoryPageCollation = collation;
    this.getIncidentHistoryPage();
  }

}
