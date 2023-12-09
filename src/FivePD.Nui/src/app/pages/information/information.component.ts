import { Component } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { InformationUpdateDTO } from "@models";
import { FDutyStatus, FIncidentPriority, FIncidentType, FUnitAvailability } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-information',
  templateUrl: './information.component.html',
  styleUrls: ['./information.component.sass'],
  standalone: true,
  imports: [TranslatePipe]
})
export class InformationComponent {

  public information: InformationUpdateDTO = {
    Event: '',
    Status: '-',
    DutyStatus: FDutyStatus.OffDuty,
    Availability: FUnitAvailability.NotReceiving,
  };

  constructor(private readonly _eventService: EventService,
              private readonly _store: Store<GlobalState>) {
    _store.select('user', 'information').subscribe(information => {
      this.information = information
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

    switch (this.information.IncidentWrapper.Incident.Priority) {
      case FIncidentPriority.Code1:
        return 'LABEL.PRIORITY.CODE1';
      case FIncidentPriority.Code2:
        return 'LABEL.PRIORITY.CODE2';
      case FIncidentPriority.Code3:
        return 'LABEL.PRIORITY.CODE3';
    }

    return '-';
  }

  protected readonly FIncidentType = FIncidentType;
}
