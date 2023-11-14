import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ActivatedRoute, Params, Router } from "@angular/router";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { PedHistory, PedLicense, PedLicenseStatus } from "@models";
import { filter, of, switchMap, tap } from "rxjs";
import { CoreSetId, FGender, PedLicenseType, SetId } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { IconComponent } from '../../../../components/icon/icon.component';
import { DatePipe, NgFor, NgForOf, NgIf } from '@angular/common';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { AppActions } from "@store/app.actions";
import { EntityValue, EntityValueLink, LooseDataService } from "@services/loose-data/loose-data.service";
import { mapAttributesToFields } from "@helpers";
import { ButtonComponent } from "../../../../components/button/button.component";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";

@Component({
  selector: 'entity-view',
  template: `
    <div class="flex flex-col gap-4">
      <div class="w-full flex flex-row items-center justify-between">
        <p class="common-title">
          {{ title | translate }}
        </p>
        <app-button *ngIf="showCreateButton"
                    [label]="'UI.ACTION.CREATE' | translate"
                    (onClick)="createButtonClick.emit()"
                    variant="secondary"/>
      </div>
      <div class="flex flex-col gap-4">
        <div *ngFor="let ticket of entities" class="bg-zinc-800 rounded-md p-4 flex flex-col gap-2">
          <div *ngFor="let item of ticket" class="grid grid-rows-1 grid-cols-2 gap-4">
            <p class="text-zinc-400 font-medium">{{ item.name }}</p>
            <ng-container *ngIf="typeof(item.value) === 'string'; else links">
              <p class="text-zinc-400">{{ item.value }}</p>
            </ng-container>
            <ng-template #links>
              <div class="flex flex-col gap-2.5">
                <button *ngFor="let link of $any(item.value)"
                        (click)="onLinkClick(link)"
                        class="group flex flex-row items-center text-zinc-300 hover:text-white">
                  {{ link.label }}
                  <div app-icon name="Link"
                       className="ml-2.5 h-[20px] text-zinc-300 group-hover:text-white"></div>
                </button>
              </div>
            </ng-template>
          </div>
        </div>
      </div>
      <p *ngIf="!entities?.length"
         class="text-zinc-400 block">
        {{ 'UI.LABEL.NO_RESULTS' | translate }}
      </p>
    </div>`,
  standalone: true,
  imports: [
    TranslatePipe,
    IconComponent,
    ButtonComponent,
    NgIf,
    NgForOf
  ]
})
class EntityViewComponent {
  readonly router = inject(Router);

  @Input() title: string = '';
  @Input() entities: EntityValue[][] = [];
  @Input() showCreateButton = false;
  @Output() createButtonClick = new EventEmitter<void>();

  typeof(value: any): string {
    return typeof value;
  }

  onLinkClick(link: EntityValueLink): void {
    void this.router.navigate(link.link);
  }
}

@Component({
  selector: 'app-citizen-database-id',
  templateUrl: './citizen-database-id.component.html',
  styleUrls: ['./citizen-database-id.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, NgIf, IconComponent, NgFor, DatePipe, TranslatePipe, ButtonComponent, EntityViewComponent]
})
export class CitizenDatabaseIdComponent {

  protected readonly CoreSetId = CoreSetId;
  protected readonly FGender = FGender;
  protected readonly PedLicenseStatus = PedLicenseStatus;

  pedHistory?: PedHistory;
  tickets: EntityValue[][] = [];
  warnings: EntityValue[][] = [];
  arrestReports: EntityValue[][] = [];

  private readonly filter = {
    PedHistoryId: 0,
  };
  private readonly filterPlural = {
    PedHistoryIds: [] as number[],
  };

  constructor(private readonly _activatedRoute: ActivatedRoute,
              private readonly _pedHistoryService: PedHistoryService,
              private readonly _store: Store<GlobalState>,
              private readonly _looseDataService: LooseDataService,
              private readonly _modalService: ModalService,
              private readonly _modalFormService: ModalFormService) {
    _activatedRoute.params
      .pipe(
        takeUntilDestroyed(),
      )
      .subscribe(params => {
        const id = parseInt(params['id']);
        this.filter.PedHistoryId = id;
        this.filterPlural.PedHistoryIds = [id];
        this.getPedHistory(id);
      });
  }

  getPedHistory(id: number): void {
    this._pedHistoryService.get(id).subscribe(pedHistory => {
      this.pedHistory = pedHistory;
      this._store.dispatch(AppActions.addPedToRecentlyViewed({ped: pedHistory}));
    });
    this.getTickets();
    this.getWarnings();
    this.getArrestReports();
  }

  getTickets(): void {
    this._looseDataService.getEntities(CoreSetId.PedTicket, this.filter).subscribe(entities => {
      this.tickets = entities.map(x => this._looseDataService.mapEntityValues({
        entity: x,
        attributes: this._looseDataService.getSetAttributes(CoreSetId.PedTicket),
      }));
    });
  }

  getWarnings(): void {
    this._looseDataService.getEntities(CoreSetId.PedWarning, this.filter).subscribe(entities => {
      this.warnings = entities.map(x => this._looseDataService.mapEntityValues({
        entity: x,
        attributes: this._looseDataService.getSetAttributes(CoreSetId.PedWarning),
      }));
    });
  }

  getArrestReports(): void {
    this._looseDataService.getEntities(CoreSetId.ArrestReport, this.filterPlural).subscribe(entities => {
      this.arrestReports = entities.map(x => this._looseDataService.mapEntityValues({
        entity: x,
        attributes: this._looseDataService.getSetAttributes(CoreSetId.ArrestReport),
      }));
    });
  }

  licenseTypeI18nKey(licenseType: PedLicenseType): string {
    switch (licenseType) {
      case PedLicenseType.Driving:
        return 'Driving';
      case PedLicenseType.Fishing:
        return 'Fishing';
      case PedLicenseType.Hunting:
        return 'Hunting';
      case PedLicenseType.Ccw:
        return 'Ccw';
    }
  }

  join(array: any[] = []): string {
    return array.join(', ');
  }

  getLicenseStatusLabel(license: PedLicense): string {
    switch (license.Status) {
      case PedLicenseStatus.Valid:
        return 'LABEL.VALID';
      case PedLicenseStatus.Expired:
        return 'LABEL.EXPIRED';
      case PedLicenseStatus.Suspended:
        return 'LABEL.SUSPENDED';
      case PedLicenseStatus.Revoked:
        return 'LABEL.REVOKED';
    }
  }

  tryCreateLooseDataEntity(setId: SetId): void {
    of(this._looseDataService.getSetAttributes(setId))
      .pipe(
        switchMap(attributes => {
          if (attributes) {
            return of(attributes);
          }

          return this._looseDataService.updateSetAttributes(setId);
        }),
        switchMap(attributes => {
          return this._modalFormService.open({
            title: 'UI.ACTION.CREATE',
            buttons: {
              submit: {variant: 'primary', label: 'UI.ACTION.CREATE'},
            },
            fields: () => mapAttributesToFields(attributes),
          }).result;
        }),
        filter(res => res.success),
        switchMap(res => this._looseDataService.addEntity(setId, res.value)),
        this._modalService.handleResponse()
      )
      .subscribe(() => {
        if (setId === CoreSetId.PedTicket) {
          this.getTickets();
        }
        if (setId === CoreSetId.PedWarning) {
          this.getWarnings();
        }
      });
  }
}
