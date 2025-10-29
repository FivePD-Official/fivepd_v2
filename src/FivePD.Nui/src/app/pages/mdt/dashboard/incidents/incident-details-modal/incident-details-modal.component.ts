import { Component, inject, InjectionToken } from '@angular/core';
import { IncidentDTO } from "@models";
import { IncidentService } from "@services/incident/incident.service";
import { AsyncPipe } from "@angular/common";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { FIncidentType } from "@enums";
import { BadgeComponent } from "../../../../../components/badge/badge.component";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { BehaviorSubject, combineLatest, map, of, shareReplay, switchMap, tap } from "rxjs";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { ButtonComponent } from "../../../../../components/button/button.component";
import { ModalRef } from "@services/modals/modal/modal-ref";
import { getPriorityBadgeColor, getPriorityLabel } from "../../../../../helpers/fpriority-helpers";
import { NotificationService } from "@services/notification/notification.service";
import { LocalizationService } from "@services/localization/localization.service";

export const INCIDENT = new InjectionToken<IncidentDTO>('Incident');

@Component({
    selector: 'app-incident-details-modal',
    imports: [
        AsyncPipe,
        TranslatePipe,
        BadgeComponent,
        ButtonComponent
    ],
    templateUrl: './incident-details-modal.component.html',
    styleUrl: './incident-details-modal.component.sass'
})
export class IncidentDetailsModalComponent {
  protected readonly FIncidentType = FIncidentType;

  readonly incident = inject(INCIDENT);
  readonly incidentService = inject(IncidentService);
  readonly store = inject<Store<GlobalState>>(Store);
  readonly modalRef = inject(ModalRef);
  readonly localizationService = inject(LocalizationService);
  readonly notificationService = inject(NotificationService);

  readonly refetch$ = new BehaviorSubject<null>(null);
  readonly incident$ = combineLatest([of(this.incident), this.refetch$]).pipe(
    switchMap(() => this.incidentService.get(this.incident.Id)),
    tap(incident => {
      if (incident) {
        return;
      }

      this.modalRef.close({value: {close: true}, success: true});
      this.notificationService.show({
        type: 'error',
        title: this.localizationService.translate('LABEL.INCIDENT.ERROR.UNAVAILABLE.TITLE'),
        description: this.localizationService.translate('LABEL.INCIDENT.ERROR.UNAVAILABLE.DESCRIPTION'),
      });
    }),
    shareReplay(),
  );
  readonly user$ = this.store.select('user', 'details').pipe(takeUntilDestroyed());

  readonly isUnitAssignedToIncident$ = combineLatest([this.incident$, this.user$]).pipe(
    map(([incident, user]) => {
      return !!user?.UnitCallsign && incident.Units.some(x => x === user?.UnitCallsign);
    }),
  );

  onSelfAssignClick(): void {
    this.incidentService.selfAssign(this.incident).subscribe(res => {
      if (res.Status) {
        this.refetch$.next(null);
        this.modalRef.close({value: {close: false}, success: true});
      }
    });
  }

  onSelfUnassignClick(): void {
    this.incidentService.selfUnassign(this.incident).subscribe(res => {
      if (res.Status) {
        this.refetch$.next(null);
        this.modalRef.close({value: {close: true}, success: true});
      }
    });
  }

  onCompleteClick(): void {
    this.incidentService.complete(this.incident).subscribe(res => {
      if (res.Status) {
        this.modalRef.close({value: {close: true}, success: true});
      }
    });
  }

  protected readonly getPriorityLabel = getPriorityLabel;
  protected readonly getPriorityBadgeColor = getPriorityBadgeColor;
}
