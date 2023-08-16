import { Component, OnInit } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { InformationUpdateDTO } from "@models";
import { FDutyStatus, FIncidentPriority, FUnitAvailability } from "@enums";

@Component({
  selector: 'app-information',
  templateUrl: './information.component.html',
  styleUrls: ['./information.component.sass']
})
export class InformationComponent implements OnInit {

  public information: InformationUpdateDTO = {
    Event: '',
    Status: '-',
    DutyStatus: FDutyStatus.OffDuty,
    Availability: FUnitAvailability.NotReceiving,
  };

  constructor(private readonly _eventService: EventService) {
  }

  ngOnInit(): void {
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
    if (!this.information.CalloutWrapper) {
      return '';
    }

    switch (this.information.CalloutWrapper.Callout.Priority) {
      case FIncidentPriority.Code1:
        return 'LABEL.PRIORITY.CODE1';
      case FIncidentPriority.Code2:
        return 'LABEL.PRIORITY.CODE2';
      case FIncidentPriority.Code3:
        return 'LABEL.PRIORITY.CODE3';
    }

    return '-';
  }

}
