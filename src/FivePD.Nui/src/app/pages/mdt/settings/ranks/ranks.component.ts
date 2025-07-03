import { Component, inject, OnInit } from '@angular/core';
import { Button, Collation, Column } from "@models";
import { BehaviorSubject, map, switchMap } from "rxjs";
import { LocalizationService } from "@services/localization/localization.service";
import { ConfigService } from "@services/config/config.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { AsyncPipe } from "@angular/common";
import { GridComponent } from "../../../../components/grid/grid.component";
import { RankService } from "@services/rank/rank.service";
import { FormattedRank, RankOptions } from "../../../_service/rank-options";

@Component({
    selector: 'app-ranks',
    imports: [
        AsyncPipe,
        GridComponent
    ],
    templateUrl: './ranks.component.html'
})
export class RanksComponent implements OnInit {

  private readonly _configService = inject(ConfigService);
  private readonly _modalService = inject(ModalService);
  private readonly _rankService = inject(RankService);
  private readonly _rankOptions = inject(RankOptions);

  columns: Column<FormattedRank>[] = [];
  buttons: Button[] = [];
  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly ranks$ = this.collation$.pipe(
    switchMap(collation => this._rankService.getPage({...collation, Filters: {...collation.Filters, Global: true}})),
    map(page => this._rankOptions.mapRanksToFormattedRanks(page)),
  );

  ngOnInit(): void {
    this._configService.getGeneralConfig().subscribe(generalConfig => {

      this.columns = this._rankOptions.createColumns({
        generalConfig,
        showActionsColumn: true,
        onEditSuccess: (res, rank) => {
          this._rankService.editGlobal({
            RankId: rank.Id,
            Name: res.value!['Name'],
            Permission: {
              DepartmentManager: res.value['DepartmentManager'],
              Kick: res.value['Kick'],
              SetRank: res.value['SetRank'],
              SetDivision: res.value['SetDivision'],
            },
            RequiredXp: res.value!['RequiredXp'],
          }).pipe(
            this._modalService.handleResponse()
          ).subscribe(() => this.collation$.next(this.collation$.value))
        },
        onDeleteSuccess: () => this.collation$.next(this.collation$.value),
      });

      this.buttons = this._rankOptions.createButtons({
        generalConfig,
        showCreateButton: true,
        onCreateSuccess: res => {
          this._rankService.createGlobal({
            Name: res.value['Name'],
            Permission: {
              DepartmentManager: res.value['DepartmentManager'],
              Kick: res.value['Kick'],
              SetRank: res.value['SetRank'],
              SetDivision: res.value['SetDivision'],
            }
          }).pipe(
            this._modalService.handleResponse()
          ).subscribe(() => {
            this.collation$.next(this.collation$.value);
          });
        }
      });

    });
  }
}
