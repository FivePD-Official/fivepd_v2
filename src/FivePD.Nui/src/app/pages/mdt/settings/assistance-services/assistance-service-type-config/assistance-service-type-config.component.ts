import { Component, inject } from '@angular/core';
import { AssistanceServiceService } from "@services/assistance-service/assistance-service.service";
import { ActivatedRoute } from "@angular/router";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { map, switchMap } from "rxjs";
import { parseId } from "../../../../../helpers/parse-id";

@Component({
  selector: 'app-assistance-service-type-config',
  imports: [],
  templateUrl: './assistance-service-type-config.component.html',
})
export class AssistanceServiceTypeConfigComponent {

  private readonly _assistanceServiceService = inject(AssistanceServiceService);
  private readonly _activatedRoute = inject(ActivatedRoute);

  readonly assistanceServiceTypeConfig$ = this._activatedRoute.params
    .pipe(
      takeUntilDestroyed(),
      map(params => parseId(params['id'])),
      switchMap(id => this._assistanceServiceService.get(id)),
    );
}
