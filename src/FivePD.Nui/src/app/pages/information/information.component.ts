import { Component, OnInit } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { NuiEventType } from "@enums";
import { FDutyStatus, FPlayerAvailability, InformationUpdateDTO } from "@models";

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

}
