import { Component } from '@angular/core';
import { DevtoolsService } from '@services/devtools/devtools.service';
import { Observable } from 'rxjs';
import { formatDate } from '@helpers';
import { Button, Column, Page } from '@models';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { CoordinateWithDate } from '@store/app.reducer';
import { addDevToolsCoordinate, clearDevToolsCoordinates } from '@store/app.actions';
import { DeleteEntityType } from '@models';

@Component({
  selector: 'app-devtools-general',
  templateUrl: './devtools-general.component.html',
  styleUrls: ['./devtools-general.component.sass']
})
export class DevtoolsGeneralComponent {

  coordinates$: Observable<Page<CoordinateWithDate>>;
  coordinatesButtons: Button[] = [
    {
      type: 'button',
      label: 'Get current coordinate',
      show: () => true,
      disabled: () => false,
      onClick: () => {
        this.devtools.getCurrentPlayerCoordinate().subscribe(res => {
          const coordinate = {...res, date: new Date()};
          this.store.dispatch(addDevToolsCoordinate({payload: coordinate}))
        });
      }
    },
    {
      type: 'button',
      label: 'Clear',
      show: () => true,
      disabled: () => false,
      onClick: () => this.store.dispatch(clearDevToolsCoordinates()),
    },
  ];
  coordinatesColumns: Column<CoordinateWithDate>[] = [
    {
      header: 'Date',
      accessorKey: 'date',
      show: () => true,
    },
    {
      header: 'Coordinate',
      valueResolver: item => `${item.X}, ${item.Y}, ${item.Z}`,
      show: () => true,
    },
    {
      header: 'Actions',
      show: () => true,
      actions: [
        {
          type: 'button',
          label: 'Copy',
          show: () => true,
          disabled: () => false,
          onClick: item => this.copyToClipboard(`${item.X}, ${item.Y}, ${item.Z}`),
        },
        {
          type: 'button',
          label: 'Copy Vector3',
          show: () => true,
          disabled: () => false,
          onClick: item => this.copyToClipboard(`new Vector3(${item.X}, ${item.Y}, ${item.Z})`),
        },
      ]
    }
  ];

  constructor(
    private readonly devtools: DevtoolsService,
    private readonly store: Store<GlobalState>,
  ) {
    this.coordinates$ = this.store.select('app', 'devTools', 'coordinates');
  }

  get DeleteEntityType() {
    return DeleteEntityType;
  }

  deleteEntities(type: DeleteEntityType): void {
    this.devtools.deleteEntities(type).subscribe();
  }

  copyToClipboard(text: string): void {
    const textArea = document.createElement('textarea');
    textArea.value = text;

    textArea.style.top = '0';
    textArea.style.left = '0';
    textArea.style.position = 'fixed';

    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
      document.execCommand('copy');
    } catch (err) {
    }

    document.body.removeChild(textArea);
  }

}
