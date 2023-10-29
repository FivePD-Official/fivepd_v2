import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { CalloutDetailsRulesetType, CalloutDetailsWithDepartments, SimpleDepartment } from "@models";
import { DepartmentService } from "@services/department/department.service";
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { zip } from "rxjs";
import deepEqual from "fast-deep-equal/es6";
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { ButtonComponent } from '../../../../components/button/button.component';
import { IconComponent } from '../../../../components/icon/icon.component';
import { NgClass, NgFor, NgIf } from '@angular/common';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';

interface Selectable<T> {
  item: T;
  selected: boolean;
}

@Component({
  selector: 'app-callout-details',
  templateUrl: './callout-details.component.html',
  styleUrls: ['./callout-details.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, NgIf, ReactiveFormsModule, NgClass, NgFor, IconComponent, ButtonComponent, TranslatePipe]
})
export class CalloutDetailsComponent implements OnInit {

  readonly CalloutDetailsRulesetType = CalloutDetailsRulesetType;

  calloutDetails?: CalloutDetailsWithDepartments;

  departments: SimpleDepartment[] = [];
  availableDepartments: Selectable<SimpleDepartment>[] = [];
  selectedDepartments: Selectable<SimpleDepartment>[] = [];

  form!: FormGroup;
  originalFormValue?: object;

  constructor(private readonly _route: ActivatedRoute,
              private readonly _calloutDetailService: CalloutDetailsService,
              private readonly _departmentService: DepartmentService) {
  }

  ngOnInit(): void {
    this.load();
  }

  get rulesetType() {
    return this.form.get('rulesetType') as FormControl;
  }

  get departmentsControl() {
    return this.form.get('departments') as FormControl;
  }

  load(): void {
    zip(
      this._calloutDetailService.get(this._route.snapshot.params['id']),
      this._departmentService.getAllSimple(),
    ).subscribe(([calloutDetails, departments]) => {
      this.departments = departments;
      this.calloutDetails = calloutDetails;

      this.selectedDepartments = departments
        .filter(x => calloutDetails.CalloutDetailsDepartments.some(y => y.DepartmentId == x.Id))
        .map(x => ({item: x, selected: false}));
      this.availableDepartments = departments.filter(x => !this.selectedDepartments.some(y => y.item.Id == x.Id))
        .map(x => ({item: x, selected: false}));

      this.form = new FormGroup({
        rulesetType: new FormControl(calloutDetails.RulesetType),
        departments: new FormControl([...this.selectedDepartments.map(x => x.item.Id)]),
      });
      this.originalFormValue = this.form.value;
      this.form.valueChanges.subscribe(() => {
        const equal = deepEqual(this.originalFormValue, this.form.value);
        if (equal) {
          this.form.markAsPristine();
        } else {
          this.form.markAsDirty();
        }
      });
    });
  }

  toLeft(): void {
    const selected = this.selectedDepartments.filter(x => x.selected);
    this.selectedDepartments = this.selectedDepartments.filter(x => !x.selected);
    for (const item of selected) {
      item.selected = false;
    }
    this.availableDepartments.push(...selected);

    this.departmentsControl.setValue([...this.selectedDepartments.map(x => x.item.Id)]);
  }

  toRight(): void {
    const selected = this.availableDepartments.filter(x => x.selected);
    this.availableDepartments = this.availableDepartments.filter(x => !x.selected);
    for (const item of selected) {
      item.selected = false;
    }
    this.selectedDepartments.push(...selected);

    this.departmentsControl.setValue([...this.selectedDepartments.map(x => x.item.Id)]);
  }

  onSubmit(): void {
    this._calloutDetailService.updateRuleset({
      Id: this.calloutDetails!.Id,
      RulesetType: this.rulesetType.value,
      Departments: this.departmentsControl.value,
    }).subscribe(() => {
      this.originalFormValue = this.form.value;
      this.form.markAsPristine();
    });
  }

  // revert(): void {
  //   if (!this.originalFormValue) {
  //     return;
  //   }
  //
  //   this.form!.setValue(this.originalFormValue);
  //   this.form!.markAsPristine();
  // }
}
