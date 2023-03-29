import { Component } from '@angular/core';
import { DeleteEntityType, DevtoolsService } from '@services/devtools/devtools.service';

@Component({
  selector: 'app-devtools-general',
  templateUrl: './devtools-general.component.html',
  styleUrls: ['./devtools-general.component.sass']
})
export class DevtoolsGeneralComponent {

  constructor(
    private readonly devtools: DevtoolsService
  ) {
  }

  deleteEntities(type: DeleteEntityType): void {
    this.devtools.deleteEntities(type).subscribe();
  }

}
