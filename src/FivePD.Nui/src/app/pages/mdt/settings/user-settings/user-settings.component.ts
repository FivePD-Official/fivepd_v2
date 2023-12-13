import { Component, OnInit } from '@angular/core';
import { UserService } from "@services/user/user.service";
import { DepartmentService } from "@services/department/department.service";
import {
  Department,
  FormInputBase,
  FormSubmitEvent,
  GeneralConfig,
  PlayerDetailsDTO,
  SingleSelectField
} from "@models";
import { filter, switchMap, tap, zip } from "rxjs";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { DynamicFormComponent } from '../../../../components/dynamic-form/dynamic-form.component';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { LocalizationService } from "@services/localization/localization.service";
import { ConfigService } from "@services/config/config.service";
import { ButtonComponent } from "../../../../components/button/button.component";
import { ModalService } from "@services/modals/modal/modal.service";
import { CallsignEditorComponent } from "./_components/user-callsign-editor/callsign-editor.component";
import {
  CALLSIGN_FORMAT_ITEMS
} from "../../departments/departments-view/_components/department-callsign-format-editor/token";
import { CALLSIGN } from "./_components/user-callsign-editor/token";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, DynamicFormComponent, TranslatePipe, ButtonComponent]
})
export class UserSettingsComponent implements OnInit {

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
      this._departmentService.getDepartment(user.DepartmentId!),
    ).subscribe(([generalConfig, availableLocalizations, department]) => {
      this.department = department;
      this.generalConfig = generalConfig;

      const fields = [...this.userDetailsFields];

      const defaultLanguageIndex = Object.keys(this.availableLocalizations).findIndex(locale => locale === generalConfig.config.DefaultLanguage);
      const preferredLanguageIndex = Object.keys(this.availableLocalizations).findIndex(locale => locale === user.PreferredLanguage);
      fields.push(new SingleSelectField({
        key: 'PreferredLanguage',
        value: preferredLanguageIndex !== -1 ? preferredLanguageIndex : defaultLanguageIndex,
        label: 'Language',
        required: true,
        addDefaultItem: false,
        height: 'base',
        defaultItemLabel: 'Language',
        variant: 'secondary',
        size: 'normal',
        items: Object.values(this.availableLocalizations).map((title, index) => ({
          key: index,
          label: title,
        })),
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
        title: 'Edit callsign'
      }, new Map()
        .set(CALLSIGN_FORMAT_ITEMS, this.department?.CallsignFormat)
        .set(CALLSIGN, this.user?.User?.Callsign)
    ).result
      .pipe(
        filter(res => res.success),
        switchMap(res => this._userService.updateUserDetails({
          Id: this.user?.User?.Id,
          Callsign: res.value['Callsign']
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
