import { AfterViewInit, Component, EventEmitter, inject, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { FormInputBase, FormSubmitEvent, ModalFormOptions } from '@models';
import { DynamicFormComponent } from '../../dynamic-form/dynamic-form.component';
import { forkJoin, map, Observable, of, Subject, switchMap, takeUntil, tap, zip } from 'rxjs';
import { ModalRef } from '@services/modals/modal/modal-ref';
import { NgIf } from '@angular/common';
import { IncidentHistoryService } from "@services/incident-history/incident-history.service";
import { CoreAttributes } from "../../../helpers/core-attributes";
import { PedHistoryService } from "@services/ped-history/ped-history.service";
import { FGender } from "@enums";
import { formatDate } from "@helpers";
import { DepartmentService } from "@services/department/department.service";
import { UserService } from "@services/user/user.service";

@Component({
  selector: 'app-modal-form',
  templateUrl: './modal-form.component.html',
  standalone: true,
  imports: [NgIf, DynamicFormComponent],
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
    const {fields, valueResolver} = this.modalRef.options as ModalFormOptions<any>;
    if (!fields) {
      return;
    }

    this.fields = fields(undefined);

    if (valueResolver) {
      valueResolver.value
        .pipe(
          takeUntil(this.destroy$),
          map(v => fields(v)),
          switchMap(fields => {
            const observables = this.getPredefinedAttributeObservables(fields);
            return zip(of(fields), forkJoin(observables))
          }),
        )
        .subscribe(([fields]) => {
          this.fields = [...fields];
        });
    } else {
      const observables = this.getPredefinedAttributeObservables(this.fields);
      zip(of(this.fields), forkJoin(observables)).subscribe(([fields]) => {
        this.fields = [...fields];
      });
    }
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

  private getPredefinedAttributeObservables(fields: FormInputBase<any>[]): Observable<any>[] {
    const observables: Observable<any>[] = [];

    const [singleUser, multiUser] = [
      this.fields.find(x => x.key === CoreAttributes.UserId),
      this.fields.find(x => x.key === CoreAttributes.UserIds),
    ];
    if (singleUser || multiUser) {
      observables.push(this._userService.getAllSimple().pipe(tap(res => {
          const mapped = res.map(item => ({
            key: item.Id,
            label: item.Name,
          }));
          if (singleUser) {
            singleUser.dropdownOptions!.items = mapped;
          }
          if (multiUser) {
            multiUser.dropdownOptions!.items = mapped;
          }
        })),
      );
    }

    const [singleDepartment, multiDepartment] = [
      this.fields.find(x => x.key === CoreAttributes.DepartmentId),
      this.fields.find(x => x.key === CoreAttributes.DepartmentIds),
    ];
    if (singleDepartment || multiDepartment) {
      observables.push(this._departmentService.getAllSimple().pipe(tap(res => {
          const mapped = res.map(item => ({
            key: item.Id,
            label: item.Name,
          }));
          if (singleDepartment) {
            singleDepartment.dropdownOptions!.items = mapped;
          }
          if (multiDepartment) {
            multiDepartment.dropdownOptions!.items = mapped;
          }
        }))
      );
    }

    const [singleIncidentHistory, multiIncidentHistory] = [
      fields.find(x => x.key === CoreAttributes.IncidentHistoryId),
      fields.find(x => x.key === CoreAttributes.IncidentHistoryIds),
    ];
    if (singleIncidentHistory || multiIncidentHistory) {
      observables.push(this._incidentHistoryService.getAllOfUser().pipe(tap(res => {
          const mapped = res.map(item => ({
            key: item.Id,
            label: `${item.Title} (${item.GeneratedAt})`,
          }));
          if (singleIncidentHistory) {
            singleIncidentHistory.dropdownOptions!.items = mapped;
          }
          if (multiIncidentHistory) {
            multiIncidentHistory.dropdownOptions!.items = mapped;
          }
        })),
      );
    }

    const [singlePedHistory, multiPedHistory] = [
      this.fields.find(x => x.key === CoreAttributes.PedHistoryId),
      this.fields.find(x => x.key === CoreAttributes.PedHistoryIds),
    ];
    if (singlePedHistory || multiPedHistory) {
      observables.push(this._pedHistoryService.getAllSimple().pipe(tap(res => {
          const mapped = res.map(item => ({
            key: item.Id,
            label: `${item.Firstname} ${item.Lastname} - ${item.Gender === FGender.Male ? 'Male' : 'Female'} - ${formatDate(new Date(item.Birthdate), 'none')}`,
          }));
          if (singlePedHistory) {
            singlePedHistory.dropdownOptions!.items = mapped;
          }
          if (multiPedHistory) {
            multiPedHistory.dropdownOptions!.items = mapped;
          }
        }))
      );
    }

    if (!observables.length) {
      observables.push(of(null));
    }

    return observables;
  }

}
