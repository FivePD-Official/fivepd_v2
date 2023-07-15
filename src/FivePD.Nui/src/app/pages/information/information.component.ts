import { Component, OnInit } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { NuiEventType } from "@enums";
import { InformationUpdateDTO } from "@models";
import { FDutyStatus, FIncidentPriority, FPlayerAvailability } from "@enums";

@Component({
  selector: 'app-information',
  templateUrl: './information.component.html',
  styleUrls: ['./information.component.sass']
})
export class InformationComponent implements OnInit {

  public information: InformationUpdateDTO = {
    type: '',
    Status: '-',
    DutyStatus: FDutyStatus.OffDuty,
    Availability: FPlayerAvailability.NotReceiving,
  };

  constructor(private readonly _eventService: EventService) {
  }

  ngOnInit(): void {
    this._eventService.onEvent<InformationUpdateDTO>(NuiEventType.InformationUpdate).subscribe(information => {
      this.information = information;
    });
  }

  get FDutyStatus() {
    return FDutyStatus;
  }

  get FPlayerAvailability() {
    return FPlayerAvailability;
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
