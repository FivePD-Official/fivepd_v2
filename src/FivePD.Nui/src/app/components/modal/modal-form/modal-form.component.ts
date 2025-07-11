import { AfterViewInit, Component, EventEmitter, inject, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { DropdownItem, FormInputBase, FormSubmitEvent, ModalFormOptions } from '@models';
import { DynamicFormComponent } from '../../dynamic-form/dynamic-form.component';
import { map, Observable, of, Subject, switchMap, takeUntil, tap, zip } from 'rxjs';
import { ModalRef } from '@services/modals/modal/modal-ref';
import { IncidentHistoryService } from "@services/incident-history/incident-history.service";
import { CoreAttributes } from "../../../helpers/core-attributes";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { FGender } from "@enums";
import { CoreAttributeDropdownItems, formatDate } from "@helpers";
import { DepartmentService } from "@services/department/department.service";
import { UserService } from "@services/user/user.service";

interface CoreAttributeDropdownItemObservables {
  userIds: Observable<DropdownItem[]>;
  departmentIds: Observable<DropdownItem[]>;
  pedHistoryIds: Observable<DropdownItem[]>;
  incidentHistoryIds: Observable<DropdownItem[]>;
}

@Component({
  selector: 'app-modal-form',
  templateUrl: './modal-form.component.html',
  imports: [DynamicFormComponent]
})
export class ModalFormComponent implements OnInit, AfterViewInit, OnDestroy {

  public readonly modalRef = inject(ModalRef);
  private readonly _userService = inject(UserService);
  private readonly _departmentService = inject(DepartmentService);
  private readonly _incidentHistoryService = inject(IncidentHistoryService);
  private readonly _pedHistoryService = inject(PedHistoryService);

  destroy$ = new Subject<void>();
  fields: FormInputBase<any>[] = [];
  @ViewChild(DynamicFormComponent) dynamicFormComponent?: DynamicFormComponent;
  @Output() form = new EventEmitter();

  ngOnInit(): void {
    this.populateForm();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngAfterViewInit(): void {
    this.form.emit(this.dynamicFormComponent);
  }

  populateForm(): void {
    const {fields, valueResolver, dataResolver} = this.modalRef.options as ModalFormOptions<any>;
    if (!fields) {
      return;
    }

    let entity: any;
    let additionalData: any;

    const a = this.getPredefinedAttributeObservables(fields());
    const coreAttributeDropdownItems: CoreAttributeDropdownItems = {
      userIds: [],
      departmentIds: [],
      pedHistoryIds: [],
      incidentHistoryIds: [],
    };

    zip([
      a.userIds,
      a.departmentIds,
      a.pedHistoryIds,
      a.incidentHistoryIds,
    ]).subscribe(([u, d, p, i]) => {
      coreAttributeDropdownItems.userIds = u;
      coreAttributeDropdownItems.departmentIds = d;
      coreAttributeDropdownItems.pedHistoryIds = p;
      coreAttributeDropdownItems.incidentHistoryIds = i;

      if (valueResolver) {
        valueResolver.value.pipe(
          takeUntil(this.destroy$),
          tap(val => entity = val),
          switchMap(() => {
            if (dataResolver) {
              return dataResolver.value;
            } else {
              return of(undefined);
            }
          }),
          takeUntil(this.destroy$),
        ).subscribe(val => {
          additionalData = val;
          this.updateFields(fields, entity, additionalData, coreAttributeDropdownItems);
        });
      } else if (dataResolver) {
        dataResolver.value.pipe(
          takeUntil(this.destroy$),
        ).subscribe(val => {
          additionalData = val;
          this.updateFields(fields, entity, additionalData, coreAttributeDropdownItems);
        });
      } else {
        this.updateFields(fields, undefined, undefined, coreAttributeDropdownItems);
      }
    });


  }

  private updateFields(fieldsFunction: (entity?: any, additionalData?: any, coreAttributeDropdownItems?: CoreAttributeDropdownItems) => FormInputBase<any>[], entity?: any, additionalData?: any, coreAttributeDropdownItems?: CoreAttributeDropdownItems): void {
    this.fields = fieldsFunction(entity, additionalData, coreAttributeDropdownItems);
  }

  submit(event?: FormSubmitEvent): void {
    this.modalRef.startLoading('Loading...');
    this.modalRef.close({
      success: true,
      value: event?.value ?? {},
    });
  }

  close(): void {
    this.modalRef.close({
      success: false,
      value: {}
    });
  }

  // TODO: why is this not invoked anywhere? current impl is not dynamic, example: https://github.com/FivePD-Official/fivepd/commit/406cd77e70140f3195919be2cbb77e6fbf7332d5#diff-ff2ce897f5b65b89e71e53125f4efaaa10cc90f0dfaf685acade83150e3d01f7R64
  private getPredefinedAttributeObservables(fields: FormInputBase<any>[]): CoreAttributeDropdownItemObservables {
    const observables: CoreAttributeDropdownItemObservables = {
      userIds: of([]),
      departmentIds: of([]),
      pedHistoryIds: of([]),
      incidentHistoryIds: of([]),
    };

    // const observables: Observable<any>[] = [];

    const [singleUser, multiUser] = [
      fields.find(x => x.key === CoreAttributes.UserId),
      fields.find(x => x.key === CoreAttributes.UserIds),
    ];
    if (singleUser || multiUser) {
      observables.userIds = this._userService.getAllSimple().pipe(map(res => {
        return res.map(item => ({
          key: item.Id,
          label: item.Name,
        }))
      }));
    }

    const [singleDepartment, multiDepartment] = [
      fields.find(x => x.key === CoreAttributes.DepartmentId),
      fields.find(x => x.key === CoreAttributes.DepartmentIds),
    ];
    if (singleDepartment || multiDepartment) {
      observables.departmentIds = this._departmentService.getAllSimple().pipe(map(res => {
        return res.map(item => ({
          key: item.Id,
          label: item.Name,
        }))
      }))
    }

    const [singleIncidentHistory, multiIncidentHistory] = [
      fields.find(x => x.key === CoreAttributes.IncidentHistoryId),
      fields.find(x => x.key === CoreAttributes.IncidentHistoryIds),
    ];
    if (singleIncidentHistory || multiIncidentHistory) {
      observables.incidentHistoryIds = this._incidentHistoryService.getAllOfUser().pipe(map(res => {
        return res.map(item => ({
          key: item.Id,
          label: `${item.Title} (${item.GeneratedAt})`,
        }))
      }))
    }

    const [singlePedHistory, multiPedHistory] = [
      fields.find(x => x.key === CoreAttributes.PedHistoryId),
      fields.find(x => x.key === CoreAttributes.PedHistoryIds),
    ];
    if (singlePedHistory || multiPedHistory) {
      observables.pedHistoryIds = this._pedHistoryService.getAllSimple().pipe(map(res => {
        return res.map(item => ({
          key: item.Id,
          label: `${item.Firstname} ${item.Lastname} - ${item.Gender === FGender.Male ? 'Male' : 'Female'} - ${formatDate(new Date(item.Birthdate), 'none')}`,
        }))
      }))
    }

    return observables;
  }

}
