import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { TextField, Vector3 } from "@models";
import { BaseFieldComponent } from "../base-field/base-field.component";
import { createDefaultFormGroup } from "../_helpers/field";

@Component({
  selector: 'app-position-field',
  imports: [ReactiveFormsModule, TranslatePipe, BaseFieldComponent],
  templateUrl: './position-field.component.html',
  styleUrl: './position-field.component.sass'
})
export class PositionFieldComponent implements OnInit {
  @Input({required: false}) form!: FormGroup;
  @Input({required: true}) field!: TextField;

  @Output() onChange = new EventEmitter<Vector3>();

  private pseudoFormControl = new FormControl<Vector3 | undefined>(undefined);

  readonly coordinateX = new FormControl<number | undefined>(undefined);
  readonly coordinateY = new FormControl<number | undefined>(undefined);
  readonly coordinateZ = new FormControl<number | undefined>(undefined);

  ngOnInit(): void {
    if (!this.form) {
      this.form = createDefaultFormGroup(this.field, '');
    }

    if (!this.control) {
      throw new Error('Form control is undefined.');
    }

    if (this.control.value) {
      const {X, Y, Z} = this.control.value;
      if (X !== undefined && Y !== undefined && Z !== undefined) {
        this.coordinateX.setValue(X);
        this.coordinateY.setValue(Y);
        this.coordinateZ.setValue(Z);
      }
    }

    this.control.valueChanges.subscribe(v => {
      this.onChange.emit(v);
    });

    this.coordinateX.valueChanges.subscribe(v => {
      this.control.setValue({X: v!, Y: this.coordinateY.value!, Z: this.coordinateZ.value!});
    });

    this.coordinateY.valueChanges.subscribe(v => {
      this.control.setValue({X: this.coordinateX.value!, Y: v!, Z: this.coordinateZ.value!});
    });

    this.coordinateZ.valueChanges.subscribe(v => {
      this.control.setValue({X: this.coordinateX.value!, Y: this.coordinateY.value!, Z: v!});
    });
  }

  get control() {
    return (this.form?.controls[this.field.key] ?? this.pseudoFormControl) as FormControl<Vector3>;
  }
}
