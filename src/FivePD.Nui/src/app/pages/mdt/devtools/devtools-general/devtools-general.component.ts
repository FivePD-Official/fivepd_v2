import { Component } from '@angular/core';
import { DeleteEntityType, DevtoolsService } from '@services/devtools/devtools.service';
import { Dispatch } from '@ngxs-labs/dispatch-decorator';
import { AddDevToolsCoordinate, ClearDevToolsCoordinates } from '@store/app.actions';
import { AppState, Coordinate, CoordinateWithDate } from '@store/app.state';
import { Select } from '@ngxs/store';
import { Observable } from 'rxjs';
import { formatDate } from '@helpers';
import { Button, Column, Page } from '@models';

@Component({
  selector: 'app-devtools-general',
  templateUrl: './devtools-general.component.html',
  styleUrls: ['./devtools-general.component.sass']
})
export class DevtoolsGeneralComponent {

  @Select(AppState.devToolsCoordinates) coordinates$!: Observable<Page<CoordinateWithDate>>;
  @Dispatch() addCoordinate = (coordinate: CoordinateWithDate) => new AddDevToolsCoordinate(coordinate);
  @Dispatch() clearCoordinates = () => new ClearDevToolsCoordinates();

  coordinatesButtons: Button[] = [
    {
      type: 'button',
      label: 'Get current coordinate',
      show: () => true,
      disabled: () => false,
      onClick: () => {
        this.devtools.getCurrentPlayerCoordinate().subscribe(res => {
          this.addCoordinate({...res, date: new Date()});
        });
      }
    },
    {
      type: 'button',
      label: 'Clear',
      show: () => true,
      disabled: () => false,
      onClick: () => this.clearCoordinates()
    },
  ];
  coordinatesColumns: Column<CoordinateWithDate>[] = [
    {
      header: 'Date',
      accessorKey: 'date',
      show: () => true,
      valueResolver: item => formatDate(item.date, 'medium'),
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
    private readonly devtools: DevtoolsService
  ) {
  }

  deleteEntities(type: DeleteEntityType): void {
    this.devtools.deleteEntities(type).subscribe();
  }

  copyToClipboard(text: string): void {
    const textArea = document.createElement("textarea");
    textArea.value = text;

    textArea.style.top = "0";
    textArea.style.left = "0";
    textArea.style.position = "fixed";

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
