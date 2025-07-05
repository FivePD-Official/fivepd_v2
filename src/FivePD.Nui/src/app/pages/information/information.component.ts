import { Component } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { InformationUpdateDTO } from "@models";
import { FDutyStatus, FIncidentType, FUnitAvailability } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../pipes/translate.pipe';
import { getPriorityLabel } from "../../helpers/fpriority-helpers";

@Component({
    selector: 'app-information',
    templateUrl: './information.component.html',
    styleUrls: ['./information.component.sass'],
    imports: [TranslatePipe]
})
export class InformationComponent {

  public information: InformationUpdateDTO = {
    PanicMode: false,
    Event: '',
    Status: '-',
    DutyStatus: FDutyStatus.OffDuty,
    Availability: FUnitAvailability.NotReceiving,
  };

  constructor(private readonly _eventService: EventService,
              private readonly _store: Store<GlobalState>) {
    this._eventService.onEvent<InformationUpdateDTO>('fivepd:ui:information:update').subscribe(information => {
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
