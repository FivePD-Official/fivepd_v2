import { Directive, HostListener, Input, OnDestroy, OnInit } from '@angular/core';
import { FormGroup, NgControl } from '@angular/forms';
import { Subscription } from 'rxjs';

@Directive({
  selector: '[appDateMask]'
})
export class DateMaskDirective implements OnInit, OnDestroy {

  @Input() appDateMask?: FormGroup;
  valueChange$?: Subscription;

  constructor(private model: NgControl) { }

  ngOnInit(): void {
    this.setValue();
    // this.valueChange$ = this.model.control?.valueChanges.subscribe(value => this.setValue(value));
  }

  ngOnDestroy(): void {
    this.valueChange$?.unsubscribe();
  }

  @HostListener('input', ['$event'])
  inputChange(e: InputEvent): void {
    this.setValue(e);
  }

  setValue(e?: InputEvent): void {
    let value = this.model.value;
    if (!value) {
      return;
    }

    if (value instanceof Date) {
      const year = value.getFullYear();
      const month = value.getMonth();
      const date = value.getDate();

      const formattedMonth = month < 10 ? `0${month + 1}` : `${month + 1}`;
      const formattedDate = date < 10 ? `0${date}` : `${date}`;

      value = `${formattedMonth}-${formattedDate}-${year}`;
    } else {
      const size = value.length;
      if (e?.inputType !== 'deleteContentBackward' && e?.inputType !== 'deleteContentForward') {
        if (size === 2 || size === 5) {
          value += '-';
        }

        const notNumber = !/^-?\d+$/.test(e?.data ?? '');
        if (size === 11 || notNumber) {
          value = value.slice(0, -1);
        }
      }
    }

    const split = value.split('-');
    const year = parseInt(split[2]);
    const month = parseInt(split[0]) - 1;
    const day = parseInt(split[1]);
    this.model.control?.setValue(new Date(Date.UTC(year, month, day)));

    this.model.valueAccessor?.writeValue(value);
    this.appDateMask?.updateValueAndValidity();
  }

}
