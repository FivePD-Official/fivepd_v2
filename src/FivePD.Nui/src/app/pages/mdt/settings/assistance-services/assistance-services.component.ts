import { Component, inject } from '@angular/core';
import { AsyncPipe } from "@angular/common";
import { ContainerComponent } from "../../../../components/container/container.component";
import { GridComponent } from "../../../../components/grid/grid.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { BehaviorSubject, filter, switchMap } from "rxjs";
import {
  AssistanceServiceStation,
  AssistanceServiceTypeConfig,
  Button,
  Collation,
  Column,
  PositionField,
  TextField
} from "@models";
import { LocalizationService } from "@services/localization/localization.service";
import {
  AssistanceServiceTypeConfigService
} from "@services/assistance-service-type-config/assistance-service-type-config.service";
import {
  AssistanceServiceStationService
} from "@services/assistance-service-station/assistance-service-station.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { CustomValidators } from "../../../../helpers/validators";
import { ModalComponent } from "../../../../components/modal/modal.component";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { Router } from "@angular/router";

@Component({
  selector: 'app-assistance-services',
  imports: [
    AsyncPipe,
    ContainerComponent,
    GridComponent,
    TranslatePipe
  ],
  templateUrl: './assistance-services.component.html',
})
export class AssistanceServicesComponent {

  private readonly _assistanceServiceTypeConfigService = inject(AssistanceServiceTypeConfigService);
  private readonly _assistanceServiceStationService = inject(AssistanceServiceStationService);
  private readonly _localizationService = inject(LocalizationService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _router = inject(Router);

  readonly typeConfigsCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly typeConfigsPage$ = this.typeConfigsCollation$.pipe(
    switchMap(collation => this._assistanceServiceTypeConfigService.getPage(collation))
  );
  readonly typeConfigsColumn: Column<AssistanceServiceTypeConfig>[] = [
    new Column({
      accessorKey: 'TypeName',
      header: 'UI.LABEL.TYPE_NAME.SINGULAR',
      show: () => true,
      filter: {
        label: 'UI.LABEL.SEARCH_BY.TYPE'
      }
    }),
    new Column({
      accessorKey: 'AssemblyName',
      header: 'LABEL.ASSEMBLY_NAME.SINGULAR',
      show: () => true,
      filter: {
        label: 'UI.LABEL.SEARCH_BY.ASSEMBLY'
      }
    }),
    new Column({
      header: 'LABEL.ACTIVE.TITLE',
      valueResolver: item => this._localizationService.translate(item.Active ? 'UI.LABEL.YES' : 'UI.LABEL.NO'),
      badgeResolver: item => item.Active ? 'green' : 'gray'
    }),
    new Column({
      header: 'LABEL.ENABLED',
      valueResolver: item => this._localizationService.translate(item.Enabled ? 'UI.LABEL.YES' : 'UI.LABEL.NO'),
      badgeResolver: item => item.Enabled ? 'green' : 'gray'
    }),
  ];

  readonly stationsCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly stationsPage$ = this.stationsCollation$.pipe(
    switchMap(collation => this._assistanceServiceStationService.getPage(collation))
  );
  readonly stationsColumn: Column<AssistanceServiceStation>[] = [
    new Column({
      accessorKey: 'Name',
      header: 'UI.LABEL.NAME.SINGULAR',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.ACTION.PLURAL',
      show: () => true,
      actions: [
        {
          type: 'button',
          label: this._localizationService.translate('ACTION.EDIT.TITLE'),
          onClick: station => this._router.navigate(['mdt', 'settings', 'assistance-services', 'stations', station.Id]),
        },
        {
          type: 'button',
          label: this._localizationService.translate('ACTION.DELETE.TITLE'),
          onClick: station => {
            this._modalConfirmService.open({
              title: 'LABEL.ASSISTANCE_SERVICE.STATION.DELETE.TITLE',
              description: this._localizationService.translate('LABEL.ASSISTANCE_SERVICE.STATION.DELETE.DESCRIPTION', {station: `<b>${station.Name}</b>`}),
              buttons: {
                submit: {variant: 'danger', label: 'ACTION.DELETE.TITLE'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(() => this._assistanceServiceStationService.delete(station.Id)),
                this._modalService.handleResponse()
              ).subscribe(() => this.stationsCollation$.next(this.stationsCollation$.value))
          }
        },
      ]
    })
  ];
  readonly stationsButtons: Button[] = [
    {
      type: 'button',
      label: 'LABEL.ASSISTANCE_SERVICE.STATION.CREATE.TITLE',
      show: () => true,
      variant: 'secondary',
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'LABEL.ASSISTANCE_SERVICE.STATION.CREATE.TITLE',
        buttons: {
          submit: {variant: 'primary', label: 'ACTION.CREATE.TITLE'},
        },
        fields: () => [
          new TextField({
            key: 'name',
            label: 'UI.LABEL.NAME.SINGULAR',
            required: true,
          }),
          new PositionField({
            key: 'position',
            label: 'UI.LABEL.POSITION.SINGULAR',
            required: true,
            validators: [CustomValidators.positionComponentsRequired()],
            options: {
              showAddCurrentPositionButton: true,
            }
          })
        ]
      }).result
        .pipe(
          filter(res => res.success),
          switchMap(res => this._assistanceServiceStationService.create(res.value)),
          this._modalService.handleResponse(),
        )
        .subscribe(() => this.stationsCollation$.next(this.stationsCollation$.value))
    }
  ];

  typeConfigsRowLinkResolver(item: AssistanceServiceTypeConfig): string {
    return `/mdt/settings/assistance-services/type-configs/${item.Id}`;
  }
}
