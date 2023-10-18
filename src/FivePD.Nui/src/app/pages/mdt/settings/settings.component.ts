import { Component, OnInit } from '@angular/core';
import { Button, CalloutDetails, Collation, Column, Page, SetAttribute, User } from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { first } from 'rxjs';
import { LocalizationService } from '@services/localization/localization.service';
import { DefaultSetId } from '@enums';
import { Store } from '@ngrx/store';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { GlobalState } from '@store';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalService } from '@services/modals/modal/modal.service';
import { UserRole, UserService } from '@services/user/user.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EventService } from "@services/event/event.service";
import { AttributeGridOptions } from "./_service/attribute-grid-options";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { ActivatedRoute, Router } from "@angular/router";

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.sass']
})
export class SettingsComponent implements OnInit {

  protected readonly UserRole = UserRole;

  user?: User;

  arrestReportAttributes?: Page<SetAttribute>;
  arrestReportAttributesCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  arrestReportAttributesColumns: Column<SetAttribute>[] = this._attributeGridOptions.createColumnList(
    DefaultSetId.ArrestReport,
    () => this.getArrestReportAttributes(),
    () => {
      this.arrestReportAttributesCollation.Page = 1;
      this.getArrestReportAttributes();
    }
  );
  arrestReportAttributesButtons: Button[] = this._attributeGridOptions.createButtons(
    DefaultSetId.ArrestReport,
    () => {
      this.arrestReportAttributesCollation.Page = 1;
      this.getArrestReportAttributes();
    }
  );

  incidentReportAttributes?: Page<SetAttribute>;
  incidentReportAttributesCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  incidentReportAttributesColumns: Column<SetAttribute>[] = this._attributeGridOptions.createColumnList(
    DefaultSetId.IncidentReport,
    () => this.getIncidentReportAttributes(),
    () => {
      this.incidentReportAttributesCollation.Page = 1;
      this.getIncidentReportAttributes();
    }
  );
  incidentReportAttributesButtons: Button[] = this._attributeGridOptions.createButtons(
    DefaultSetId.IncidentReport,
    () => {
      this.incidentReportAttributesCollation.Page = 1;
      this.getIncidentReportAttributes();
    }
  );

  users?: Page<User>;
  usersCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  usersColumns: Column<User>[] = [
    new Column<User>({
      header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
      show: () => true,
      accessorKey: 'Name',
    }),
    new Column<User>({
      header: this._localization.translate('UI.LABEL.ROLE.SINGULAR'),
      show: () => this.userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'select',
          label: this._localization.translate('UI.LABEL.ROLE.NO'),
          selectedItemsResolver: (user, dropdownItemKey) => this.userService.userHasRole(dropdownItemKey),
          items: () => {
            return [
              {key: UserRole.Admin, label: this._localization.translate('UI.LABEL.ROLE.ADMIN')},
              {key: UserRole.Developer, label: this._localization.translate('UI.LABEL.ROLE.DEVELOPER')},
            ];
          },
          resetWhenSelectedId: UserRole.None,
          onSelectMulti: (user, selectedIds) => {
            this.userService.changeRole(user, selectedIds).subscribe();
          }
        }
      ]
    }),
  ];

  calloutDetails?: Page<CalloutDetails>;
  calloutDetailsCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  calloutDetailsColumns: Column<CalloutDetails>[] = [
    new Column({
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: 'TypeName',
    }),
    new Column({
      header: 'UI.LABEL.ASSEMBLY.NAME.SINGULAR',
      accessorKey: 'AssemblyName',
    }),
    new Column({
      header: 'Active',
      valueResolver: item => item.Active ? 'Yes' : 'No',
      badgeResolver: item => item.Active ? 'green' : 'red'
    }),
    new Column({
      header: 'Actions',
      actions: [
        {
          type: 'button',
          label: 'Edit',
          show: () => this.userService.userHasRole(UserRole.Admin),
          onClick: item => this._router.navigate(['mdt', 'settings', 'callout-details', item.Id]),
        },
      ],
    })
  ];

  constructor(
    public readonly userService: UserService,
    private readonly _looseData: LooseDataService,
    private readonly _modalService: ModalService,
    private readonly _modalFormService: ModalFormService,
    private readonly _modalConfirmService: ModalConfirmService,
    private readonly _localization: LocalizationService,
    private readonly _store: Store<GlobalState>,
    private readonly _eventService: EventService,
    private readonly _attributeGridOptions: AttributeGridOptions,
    private readonly _calloutDetailService: CalloutDetailsService,
    private readonly _router: Router,
    private readonly _a: ActivatedRoute
  ) {
    _store.pipe(takeUntilDestroyed(), first()).subscribe(state => {
      this.user = state.user.details.User;
    });
  }

  ngOnInit(): void {
    this.getSettings();
  }

  private getSettings(): void {
    if (!this.user || !this.userService.userHasRole(UserRole.Admin)) {
      return;
    }

    this.getArrestReportAttributes();
    this.getIncidentReportAttributes();
    this.getUsers();
    this.getCalloutDetails();
  }

  getArrestReportAttributes(): void {
    this._looseData.getSetAttributesPage(DefaultSetId.ArrestReport, this.arrestReportAttributesCollation).subscribe(res => {
      this.arrestReportAttributes = res;
    });
  }

  onArrestReportAttributesCollationChange(collation: Collation): void {
    this.arrestReportAttributesCollation = collation;
    this.getArrestReportAttributes();
  }

  getIncidentReportAttributes(): void {
    this._looseData.getSetAttributesPage(DefaultSetId.IncidentReport, this.incidentReportAttributesCollation).subscribe(res => {
      this.incidentReportAttributes = res;
    });
  }

  onIncidentReportAttributesCollationChange(collation: Collation): void {
    this.incidentReportAttributesCollation = collation;
    this.getIncidentReportAttributes();
  }

  getUsers(): void {
    this.userService.getAllPageable(this.arrestReportAttributesCollation).subscribe(res => {
      this.users = res;
    });
  }

  onUsersCollationChange(collation: Collation): void {
    this.usersCollation = collation;
    this.getUsers();
  }

  getCalloutDetails(): void {
    this._calloutDetailService.getAll(this.calloutDetailsCollation).subscribe(res => {
      this.calloutDetails = res;
    });
  }

  onCalloutDetailsCollationChange(collation: Collation): void {
    this.calloutDetailsCollation = collation;
    this.getCalloutDetails();
  }
}
