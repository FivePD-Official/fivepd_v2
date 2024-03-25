import { Component } from '@angular/core';
import { DevtoolsService } from '@services/devtools/devtools.service';
import { filter } from 'rxjs';
import { Button, Column, DeleteEntityType, Page } from '@models';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { CoordinateWithDate } from '@store/app.reducer';
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { NotificationService } from "@services/notification/notification.service";
import { GridComponent } from '../../../../components/grid/grid.component';
import { ButtonComponent } from '../../../../components/button/button.component';
import { ContainerComponent } from '../../../../components/container/container.component';
import { AppActions } from "@store/app.actions";

@Component({
  selector: 'app-devtools-general',
  templateUrl: './devtools-general.component.html',
  styleUrls: ['./devtools-general.component.sass'],
  standalone: true,
  imports: [ContainerComponent, ButtonComponent, GridComponent]
})
export class DevtoolsGeneralComponent {

  coordinates?: Page<CoordinateWithDate>;
  coordinatesButtons: Button[] = [
    {
      type: 'button',
      label: 'Copy all Vector3',
      variant: 'tertiary-bg',
      show: () => true,
      disabled: () => (this.coordinates?.Items ?? []).length === 0,
      onClick: () => {
        const text = this.coordinates!.Items.map(item => `new Vector3(${item.X}f, ${item.Y}f, ${item.Z}f)`).join(',\n');
        this.copyToClipboard(text);
      }
    },
    {
      type: 'button',
      label: 'Clear',
      variant: 'tertiary-bg',
      show: () => true,
      disabled: () => (this.coordinates?.Items ?? []).length === 0,
      onClick: () => {
        this._modalConfirmService.open({
          title: 'Clear coordinates',
          description: 'Are you sure you want to clear all coordinates?',
          buttons: {
            submit: {
              label: 'Clear',
              variant: 'danger',
            },
          },
        }).result
          .pipe(
            filter(res => res.success),
            this._modalService.close(),
          )
          .subscribe(() => this.store.dispatch(AppActions.clearDevToolsCoordinates()));
      }
    },
    {
      type: 'button',
      label: 'Get current coordinate',
      show: () => true,
      disabled: () => false,
      onClick: () => {
        this.devtools.getCurrentPlayerPosition().subscribe(res => {
          const coordinate = {...res, date: new Date()};
          this.store.dispatch(AppActions.addDevToolsCoordinate({coordinate}))
        });
      }
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
          onClick: item => this.copyToClipboard(`${item.X}f, ${item.Y}f, ${item.Z}f`),
        },
        {
          type: 'button',
          label: 'Copy Vector3',
          show: () => true,
          disabled: () => false,
          onClick: item => this.copyToClipboard(`new Vector3(${item.X}f, ${item.Y}f, ${item.Z}f)`),
        },
      ]
    }
  ];

  constructor(
    private readonly devtools: DevtoolsService,
    private readonly store: Store<GlobalState>,
    private readonly _modalService: ModalService,
    private readonly _modalConfirmService: ModalConfirmService,
    private readonly _notificationService: NotificationService,
  ) {
    this.store.select('app', 'devTools', 'coordinates').subscribe(coordinates => this.coordinates = coordinates);
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

    this._notificationService.show({
      type: 'success',
      title: 'Copied to clipboard',
      description: 'Copied the selected item to your clipboard.',
    });
  }

}
