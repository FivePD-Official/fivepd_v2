import { Component, inject } from '@angular/core';
import {
  AssistanceServiceTypeConfigService
} from "@services/assistance-service-type-config/assistance-service-type-config.service";
import { ActivatedRoute } from "@angular/router";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { filter, map, switchMap, tap } from "rxjs";
import { parseId } from "../../../../../helpers/parse-id";
import { ContainerComponent } from "../../../../../components/container/container.component";
import { PageHeaderComponent } from "../../../../../components/page-header/page-header.component";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { AsyncPipe, NgClass } from "@angular/common";
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from "@angular/forms";
import deepEqual from "fast-deep-equal/es6";
import {
  AssistanceServiceTypeConfig,
  AssistanceServiceTypeConfigSpawnTypes,
  CheckboxField,
  PositionField
} from "@models";
import { CheckboxFieldComponent } from "../../../../../components/form-inputs/checkbox-field/checkbox-field.component";
import { ButtonComponent } from "../../../../../components/button/button.component";
import { DevtoolsService } from "@services/devtools/devtools.service";
import { IconComponent } from "../../../../../components/icon/icon.component";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { CustomValidators } from "../../../../../helpers/validators";

@Component({
  selector: 'app-assistance-service-type-config',
  imports: [
    ContainerComponent,
    PageHeaderComponent,
    TranslatePipe,
    AsyncPipe,
    FormsModule,
    ReactiveFormsModule,
    NgClass,
    CheckboxFieldComponent,
    ButtonComponent,
    IconComponent
  ],
  templateUrl: './assistance-service-type-config.component.html',
})
export class AssistanceServiceTypeConfigComponent {

  private readonly _assistanceServiceService = inject(AssistanceServiceTypeConfigService);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _devtoolsService = inject(DevtoolsService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);

  readonly assistanceServiceTypeConfig$ = this._activatedRoute.params
    .pipe(
      takeUntilDestroyed(),
      map(params => parseId(params['id'])),
      switchMap(id => this._assistanceServiceService.get(id)),
      tap(v => this.createForm(v))
    );

  readonly spawnTypes = [
    {
      value: AssistanceServiceTypeConfigSpawnTypes.AUTO,
      label: 'LABEL.ASSISTANCE_SERVICE.SPAWN_TYPE.TYPE.AUTO.TITLE',
      helpLabel: 'LABEL.ASSISTANCE_SERVICE.SPAWN_TYPE.TYPE.AUTO.HELP_LABEL',
    },
    {
      value: AssistanceServiceTypeConfigSpawnTypes.MANUAL,
      label: 'LABEL.ASSISTANCE_SERVICE.SPAWN_TYPE.TYPE.MANUAL.TITLE',
      helpLabel: 'LABEL.ASSISTANCE_SERVICE.SPAWN_TYPE.TYPE.MANUAL.HELP_LABEL',
    },
    {
      value: AssistanceServiceTypeConfigSpawnTypes.STATION,
      label: 'LABEL.ASSISTANCE_SERVICE.SPAWN_TYPE.TYPE.STATION.TITLE',
      helpLabel: 'LABEL.ASSISTANCE_SERVICE.SPAWN_TYPE.TYPE.STATION.HELP_LABEL',
    },
  ]

  form!: FormGroup;
  originalFormValue?: object;
  enabledCheckboxField!: CheckboxField;

  get enabled() {
    return this.form.get('enabled') as FormControl;
  }

  get spawnType() {
    return this.form.get('spawnType') as FormControl;
  }

  get manualSpawnLocations() {
    return this.form.get('manualSpawnLocations') as FormControl;
  }

  get activeSpawnTypeHelpLabel() {
    return this.spawnTypes.find(x => x.value === this.spawnType.value)?.helpLabel ?? '';
  }

  createForm(assistanceServiceTypeConfig: AssistanceServiceTypeConfig): void {
    this.form = new FormGroup({
      enabled: new FormControl(assistanceServiceTypeConfig.Enabled),
      spawnType: new FormControl(assistanceServiceTypeConfig.SpawnType),
      manualSpawnLocations: new FormControl(assistanceServiceTypeConfig.ManualSpawnLocations),
    });
    this.enabledCheckboxField = new CheckboxField({
      key: 'enabled',
      value: assistanceServiceTypeConfig.Enabled,
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
  }

  onSubmit(assistanceServiceTypeConfig: AssistanceServiceTypeConfig): void {
    this._assistanceServiceService.edit({
      Id: assistanceServiceTypeConfig.Id,
      Enabled: this.enabled.value,
      SpawnType: this.spawnType.value,
      ManualSpawnLocations: this.manualSpawnLocations.value,
    }).subscribe(() => {
      this.originalFormValue = this.form.value;
      this.form.markAsPristine();
    });
  }

  onAddPositionManually(): void {
    this._modalFormService.open({
      title: 'Add position',
      buttons: {
        submit: {variant: 'primary', label: 'ACTION.ADD.TITLE'},
      },
      fields: () => [new PositionField({
        key: 'position',
        required: true,
        validators: [CustomValidators.positionComponentsRequired()],
        label: 'Position',
      })]
    }).result
      .pipe(
        filter(res => res.success),
        tap(res => this.manualSpawnLocations.setValue([...this.manualSpawnLocations.value, res.value['position']])),
        this._modalService.close(),
      )
      .subscribe()
  }

  onAddCurrentPosition(): void {
    this._devtoolsService.getCurrentPlayerPosition().subscribe(position => {
      this.manualSpawnLocations.setValue([...this.manualSpawnLocations.value, position]);
    });
  }

  onRemoveManualSpawnPosition(index: number): void {
    const positions = [...this.manualSpawnLocations.value];
    positions.splice(index, 1);
    this.manualSpawnLocations.setValue(positions);
  }

  protected readonly AssistanceServiceTypeConfigSpawnTypes = AssistanceServiceTypeConfigSpawnTypes;
}
