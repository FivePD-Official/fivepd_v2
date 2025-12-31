import { Component, inject } from '@angular/core';
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { BehaviorSubject, filter, map, switchMap, tap } from "rxjs";
import { parseId } from "../../../../../helpers/parse-id";
import {
  AssistanceServiceStationService
} from "@services/assistance-service-station/assistance-service-station.service";
import { ActivatedRoute } from "@angular/router";
import { PageHeaderComponent } from "../../../../../components/page-header/page-header.component";
import { ContainerComponent } from "../../../../../components/container/container.component";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { AsyncPipe } from "@angular/common";
import {
  AssistanceServiceStationTypeConfig,
  AssistanceServiceTypeConfig,
  Button,
  Collation,
  Column,
  DropdownItem,
  NumberField,
  SingleSelectField
} from "@models";
import { GridComponent } from "../../../../../components/grid/grid.component";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { DataResolver } from "@services/modals/modal-form/modal-form-data-resolver";
import { LocalizationService } from "@services/localization/localization.service";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";

@Component({
  selector: 'app-assistance-service-station',
  imports: [
    PageHeaderComponent,
    ContainerComponent,
    TranslatePipe,
    AsyncPipe,
    GridComponent
  ],
  templateUrl: './assistance-service-station.component.html',
})
export class AssistanceServiceStationComponent {

  private readonly _assistanceServiceStationService = inject(AssistanceServiceStationService);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _localizationService = inject(LocalizationService);

  readonly stationId$ = new BehaviorSubject<number>(0);
  readonly assistanceServiceStation$ = this._activatedRoute.params
    .pipe(
      takeUntilDestroyed(),
      map(params => parseId(params['id'])),
      tap(id => this.stationId$.next(id)),
      switchMap(id => this._assistanceServiceStationService.get(id)),
    );

  readonly typeConfigsCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly typeConfigsPage$ = this.typeConfigsCollation$.pipe(
    switchMap(collation => this._assistanceServiceStationService.getTypeConfigsPage(this.stationId$.value, collation)),
  );
  readonly typeConfigsColumn: Column<AssistanceServiceStationTypeConfig>[] = [
    new Column({
      header: 'LABEL.TYPE_NAME.SINGULAR',
      valueResolver: item => item.TypeConfig.TypeName,
      show: () => true,
    }),
    new Column({
      header: 'LABEL.ASSEMBLY_NAME.SINGULAR',
      valueResolver: item => item.TypeConfig.AssemblyName,
      show: () => true,
    }),
    new Column({
      valueResolver: item => item.Capacity.toString(),
      header: 'LABEL.CAPACITY.SINGULAR',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.ACTION.PLURAL',
      show: () => true,
      actions: [
        {
          type: 'button',
          label: this._localizationService.translate('ACTION.EDIT.TITLE'),
          onClick: stationTypeConfig => this._modalFormService.open({
            title: 'ACTION.EDIT.TITLE',
            buttons: {
              submit: {variant: 'primary', label: 'ACTION.SAVE.TITLE'},
            },
            valueResolver: new ValueResolver(stationTypeConfig, this._assistanceServiceStationService.getTypeConfig(stationTypeConfig.Id)),
            fields: (updatedStationTypeConfig) => {
              const c = updatedStationTypeConfig ?? stationTypeConfig;
              return [
                new SingleSelectField({
                  defaultItemLabel: `${c.TypeConfig.TypeName} - ${c.TypeConfig.AssemblyName}`,
                  size: 'normal',
                  variant: 'secondary',
                  key: 'typeConfigId',
                  label: 'Assistance service',
                  required: true,
                  readonly: true,
                  items: () => [],
                }),
                new NumberField({
                  key: 'capacity',
                  label: 'LABEL.CAPACITY.SINGULAR',
                  required: true,
                  value: c.Capacity,
                }),
              ];
            }
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(res => this._assistanceServiceStationService.editTypeConfig({
                Id: stationTypeConfig.Id,
                Capacity: res.value['capacity'],
              })),
              this._modalService.handleResponse()
            ).subscribe(() => this.typeConfigsCollation$.next(this.typeConfigsCollation$.value))
        },
        {
          type: 'button',
          label: 'ACTION.DELETE.TITLE',
          onClick: stationTypeConfig => {
            this._modalConfirmService.open({
              title: 'LABEL.ASSISTANCE_SERVICE.STATION.TYPE_CONFIG.DELETE.TITLE',
              description: this._localizationService.translate('LABEL.ASSISTANCE_SERVICE.STATION.TYPE_CONFIG.DELETE.DESCRIPTION'),
              buttons: {
                submit: {variant: 'danger', label: 'ACTION.DELETE.TITLE'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(() => this._assistanceServiceStationService.deleteTypeConfig(stationTypeConfig.Id)),
                this._modalService.handleResponse()
              ).subscribe(() => this.typeConfigsCollation$.next(this.typeConfigsCollation$.value))
          }
        },
      ]
    })
  ];
  readonly typeConfigsButtons: Button[] = [
    {
      type: 'button',
      label: 'LABEL.ASSISTANCE_SERVICE.STATION.TYPE_CONFIG.ADD.TITLE',
      show: () => true,
      variant: 'secondary',
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'LABEL.ASSISTANCE_SERVICE.STATION.TYPE_CONFIG.ADD.TITLE',
        buttons: {
          submit: {variant: 'primary', label: 'ACTION.ADD.TITLE'},
        },
        dataResolver: new DataResolver([] as AssistanceServiceTypeConfig[], this._assistanceServiceStationService.getAvailableStationTypeConfigs(this.stationId$.value)),
        fields: (_, assistanceServiceTypeConfigs) => [
          new SingleSelectField({
            defaultItemLabel: "",
            size: 'normal',
            variant: 'secondary',
            key: 'typeConfigId',
            label: 'LABEL.ASSISTANCE_SERVICE.TITLE.SINGULAR',
            required: true,
            items: () => (assistanceServiceTypeConfigs ?? []).map(x => ({
              key: x.Id,
              label: `${x.TypeName} - ${x.AssemblyName}`
            } as DropdownItem))
          }),
          new NumberField({
            key: 'capacity',
            label: 'LABEL.CAPACITY.SINGULAR',
            required: true,
          }),
        ]
      }).result
        .pipe(
          filter(res => res.success),
          tap(x => console.log(x)),
          switchMap(res => this._assistanceServiceStationService.addTypeConfig({
            StationId: this.stationId$.value,
            TypeConfigId: res.value['typeConfigId'],
            Capacity: res.value['capacity'],
          })),
          this._modalService.handleResponse(),
        )
        .subscribe(() => this.typeConfigsCollation$.next(this.typeConfigsCollation$.value))
    }
  ];

}
