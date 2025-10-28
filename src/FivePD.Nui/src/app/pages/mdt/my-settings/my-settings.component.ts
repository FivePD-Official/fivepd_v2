import { Component, OnInit } from '@angular/core';
import { UserService } from "@services/user/user.service";
import { DepartmentService } from "@services/department/department.service";
import {
  Department,
  FormInputBase,
  FormSubmitEvent,
  GeneralConfig, NumberField,
  PlayerDetailsDTO,
  SingleSelectField, User
} from "@models";
import { filter, of, switchMap, tap, zip } from "rxjs";
import { TranslatePipe } from '../../../pipes/translate.pipe';
import { DynamicFormComponent } from '../../../components/dynamic-form/dynamic-form.component';
import { ContainerComponent } from '../../../components/container/container.component';
import { PageHeaderComponent } from '../../../components/page-header/page-header.component';
import { LocalizationService } from "@services/localization/localization.service";
import { ConfigService } from "@services/config/config.service";
import { ButtonComponent } from "../../../components/button/button.component";
import { ModalService } from "@services/modals/modal/modal.service";
import { CallsignEditorComponent } from "./_components/user-callsign-editor/callsign-editor.component";
import { CALLSIGN_FORMAT_VALUES } from "./_components/user-callsign-editor/token";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import {
  CALLSIGN_FORMAT_ITEMS
} from "../departments/departments-view/_components/department-callsign-format-editor/token";
import { Validators } from "@angular/forms";

@Component({
    selector: 'app-my-settings',
    templateUrl: './my-settings.component.html',
    imports: [PageHeaderComponent, ContainerComponent, DynamicFormComponent, TranslatePipe, ButtonComponent]
})
export class MySettingsComponent implements OnInit {

  user?: PlayerDetailsDTO;
  generalConfig?: GeneralConfig;
  department?: Department;
  userDetailsFields: FormInputBase<any>[] = [];
  availableLocalizations: Record<string, string> = {};

  constructor(private readonly _userService: UserService,
              private readonly _departmentService: DepartmentService,
              private readonly _localizationService: LocalizationService,
              private readonly _configService: ConfigService,
              private readonly _modalService: ModalService,
              private readonly _store: Store<GlobalState>) {
    _store.select('user', 'details').subscribe(user => this.user = user);
  }

  ngOnInit(): void {
    const user = this._userService.user?.User;
    if (!user) {
      return;
    }

    zip(
      this._configService.getGeneralConfig(),
      this.loadAvailableLocales(),
      user.DepartmentId ? this._departmentService.getDepartment(user.DepartmentId) : of(undefined),
    ).subscribe(([generalConfig, availableLocalizations, department]) => {
      this.department = department;
      this.generalConfig = generalConfig;

      const fields = [...this.userDetailsFields];

      const defaultLanguageIndex = Object.keys(availableLocalizations).findIndex(locale => locale === generalConfig.config.DefaultLanguage);
      const preferredLanguageIndex = Object.keys(availableLocalizations).findIndex(locale => locale === user.PreferredLanguage);
      fields.push(new SingleSelectField({
        key: 'PreferredLanguage',
        value: preferredLanguageIndex !== -1 ? preferredLanguageIndex : defaultLanguageIndex,
        label: 'UI.LABEL.LANGUAGE.SINGULAR',
        required: true,
        addDefaultItem: false,
        height: 'base',
        defaultItemLabel: this._localizationService.translate('UI.LABEL.LANGUAGE.SINGULAR'),
        variant: 'secondary',
        size: 'normal',
        items: () => Object.values(availableLocalizations).map((title, index) => ({
          key: index,
          label: title,
        })),
      }));

      fields.push(new NumberField({
        key: 'AudioVolume',
        value: user.AudioVolume,
        label: 'UI.LABEL.MY_SETTINGS.AUDIO_VOLUME.TITLE',
        required: true,
        helpLabel: 'UI.LABEL.MY_SETTINGS.AUDIO_VOLUME.HELP_LABEL',
        validators: [Validators.min(0), Validators.max(1)],
        options: {
          min: 0,
          max: 1,
        }
      }));

      this.userDetailsFields = fields;
    });
  }

  onUserDetailsSubmit(event: FormSubmitEvent): void {
    const user = this._userService.user?.User;
    if (!user) {
      return;
    }

    // Override the preferred language index with the actual locale
    event.value['PreferredLanguage'] = Object.keys(this.availableLocalizations)[event.value['PreferredLanguage']];

    this._userService.updateUserDetails({
      Id: user.Id,
      ...event.value,
    })
      .pipe(
        filter(res => res.Status),
        tap(() => event.markAsPristine()),
      )
      .subscribe(() => {
        this._userService.refreshData()
      });
  }

  tryEditCallsign(): void {
    this._modalService.open(CallsignEditorComponent, {
        title: 'ACTION.EDIT.CALLSIGN.TITLE'
      }, new Map()
        .set(CALLSIGN_FORMAT_ITEMS, this.department?.CallsignFormat)
        .set(CALLSIGN_FORMAT_VALUES, this.user?.User?.CallsignFormat.Values)
    ).result
      .pipe(
        filter(res => res.success),
        switchMap(res => this._userService.updateUserDetails({
          Id: this.user?.User?.Id,
          CallsignFormatValues: res.value['CallsignFormatValues']
        })),
        this._modalService.handleResponse()
      )
      .subscribe(() => this._userService.refreshData());
  }

  private loadAvailableLocales() {
    return this._localizationService.getAvailableLocalizations()
      .pipe(
        tap(locales => this.availableLocalizations = locales),
      );
  }

}
