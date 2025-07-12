import { Component, inject, OnInit } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { InformationUpdateDTO } from "@models";
import { FDutyStatus, FIncidentType, FUnitAvailability } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../pipes/translate.pipe';
import { getPriorityLabel } from "../../helpers/fpriority-helpers";
import { UserActions } from "@store/user.actions";
import { injectDestroy } from "../../helpers/destroy-ref";
import { takeUntil } from "rxjs";

@Component({
  selector: 'app-information',
  templateUrl: './information.component.html',
  styleUrls: ['./information.component.sass'],
  imports: [TranslatePipe]
})
export class InformationComponent implements OnInit {

  public information: InformationUpdateDTO = {
    PanicMode: false,
    Event: '',
    Status: '-',
    DutyStatus: FDutyStatus.OffDuty,
    Availability: FUnitAvailability.NotReceiving,
  };

  private readonly destroy$ = injectDestroy();
  private readonly _eventService = inject(EventService);
  private readonly _store = inject<Store<GlobalState>>(Store);

  ngOnInit(): void {
    this._eventService.onEvent<InformationUpdateDTO>('fivepd:information:update')
      .pipe(takeUntil(this.destroy$))
      .subscribe(information => {
        this._store.dispatch(UserActions.setInformation({information}));
      });

    this._store.select('user', 'information')
      .pipe(takeUntil(this.destroy$))
      .subscribe(information => {
        this.information = information;
      });
  }

  get FDutyStatus() {
    return FDutyStatus;
  }

  get FPlayerAvailability() {
    return FUnitAvailability;
  }

  get priorityLabel(): string {
    if (!this.information.IncidentWrapper) {
      return '';
    }

    return getPriorityLabel(this.information.IncidentWrapper.Incident.Priority);
  }

  protected readonly FIncidentType = FIncidentType;
}
