import { Component, OnInit } from '@angular/core';
import { UserService } from "@services/user/user.service";
import { UnitService } from "@services/unit/unit.service";
import { DepartmentService } from "@services/department/department.service";
import { Department, FormInputBase, FormSubmitEvent, TextInput } from "@models";
import { CustomValidators } from "../../../../../helpers/validators";
import { filter, switchMap, tap } from "rxjs";

@Component({
  selector: 'app-user-settings',
  templateUrl: './user-settings.component.html',
  styleUrls: ['./user-settings.component.sass']
})
export class UserSettingsComponent implements OnInit {

  department?: Department;
  userDetailsFields: FormInputBase<any>[] = [];

  constructor(private readonly _userService: UserService,
              private readonly _unitService: UnitService,
              private readonly _departmentService: DepartmentService) {
  }

  ngOnInit(): void {
    this.getDepartment();
  }

  getDepartment(): void {
    const user = this._userService.user?.User;
    if (!user) {
      return;
    }

    this._departmentService.getDepartment(user.DepartmentId!).subscribe(department => {
      this.department = department;
      this.userDetailsFields = [
        new TextInput({
          key: 'Callsign',
          value: user.Callsign,
          label: 'Callsign',
          helpLabel: `Format: ${department.CallsignFormat ?? 'Unknown'}`,
          required: true,
          validators: [CustomValidators.callsignFormat()],
        }),
      ];
    });
  }

  onUserDetailsSubmit(event: FormSubmitEvent): void {
    const user = this._userService.user?.User;
    if (!user) {
      return;
    }

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

}
