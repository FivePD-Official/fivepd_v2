import { Component, inject, InjectionToken } from '@angular/core';
import { IncidentDTO } from "@models";
import { IncidentService } from "@services/incident/incident.service";
import { AsyncPipe } from "@angular/common";
import { Observable } from "rxjs";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { FIncidentType } from "@enums";
import { BadgeComponent } from "../../../../../components/badge/badge.component";

export const INCIDENT = new InjectionToken<IncidentDTO>('Incident');

@Component({
  selector: 'app-incident-details-modal',
  standalone: true,
  imports: [
    AsyncPipe,
    TranslatePipe,
    BadgeComponent
  ],
  templateUrl: './incident-details-modal.component.html',
  styleUrl: './incident-details-modal.component.sass'
})
export class IncidentDetailsModalComponent {
  readonly incident = inject(INCIDENT);
  readonly incidentService = inject(IncidentService);

  readonly incident$: Observable<IncidentDTO> = this.incidentService.get(this.incident.Id);
  protected readonly FIncidentType = FIncidentType;
}
