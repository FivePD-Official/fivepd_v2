import { Component, OnInit } from '@angular/core';
import { UserService } from "@services/user/user.service";
import { DepartmentService } from "@services/department/department.service";
import { Department, Dropdown, FormInputBase, FormSubmitEvent, GeneralConfig, TextInput } from "@models";
import { CustomValidators } from "../../../../helpers/validators";
import { filter, tap, zip } from "rxjs";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { DynamicFormComponent } from '../../../../components/dynamic-form/dynamic-form.component';
import { NgIf } from '@angular/common';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { LocalizationService } from "@services/localization/localization.service";
import { ConfigService } from "@services/config/config.service";

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, NgIf, DynamicFormComponent, TranslatePipe]
})
export class UserSettingsComponent implements OnInit {

  generalConfig?: GeneralConfig;
  department?: Department;
  userDetailsFields: FormInputBase<any>[] = [];
  availableLocalizations: Record<string, string> = {};

  constructor(private readonly _userService: UserService,
              private readonly _departmentService: DepartmentService,
              private readonly _localizationService: LocalizationService,
              private readonly _configService: ConfigService) {
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
      this.generalConfig = generalConfig;

      const fields = [...this.userDetailsFields];

      const defaultLanguageIndex = Object.keys(this.availableLocalizations).findIndex(locale => locale === generalConfig.config.DefaultLanguage);
      const preferredLanguageIndex = Object.keys(this.availableLocalizations).findIndex(locale => locale === user.PreferredLanguage);
      fields.push(new Dropdown({
        key: 'PreferredLanguage',
        value: preferredLanguageIndex !== -1 ? preferredLanguageIndex : defaultLanguageIndex,
        label: 'Preferred language',
        required: true,
        dropdownOptions: {
          addDefaultItem: false,
          label: '',
          type: 'single',
          height: 'base',
          variant: 'secondary',
          size: 'normal',
          items: Object.values(this.availableLocalizations).map((title, index) => ({
            key: index,
            label: title,
          })),
        }
      }));

      if (department) {
        fields.push(new TextInput({
          key: 'Callsign',
          value: user.Callsign,
          label: 'Callsign',
          helpLabel: `Format: ${department.CallsignFormat ?? 'Unknown'}`,
          required: true,
          validators: [CustomValidators.callsignFormat()],
        }));
      }

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

  private loadAvailableLocales() {
    return this._localizationService.getAvailableLocalizations()
      .pipe(
        tap(locales => this.availableLocalizations = locales),
      );
  }

}
