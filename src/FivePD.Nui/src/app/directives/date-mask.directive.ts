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
    this.valueChange$ = this.model.control?.valueChanges.subscribe(value => this.setValue(value));
  }

  ngOnDestroy(): void {
    this.valueChange$?.unsubscribe();
  }

  @HostListener('input')
  inputChange(): void {
    this.setValue();
  }

  setValue(value?: any): void {
    let newValue = value || this.model.value;

    if (newValue instanceof Date) {
      const year = newValue.getFullYear();
      const month = newValue.getMonth();
      const date = newValue.getDate();

      const formattedMonth = month < 10 ? `0${month}` : `${month}`;
      const formattedDate = date < 10 ? `0${date}` : `${date}`;

      newValue = `${formattedMonth}/${formattedDate}/${year}`;
    } else {
      const size = newValue.length;

      if ((size == 2 && newValue > 31) || (size == 5 && Number(newValue.split('/')[1]) > 12) || (size == 10 && Number(newValue.split('/')[2]) > 2022)) {
        this.model.control?.setErrors({ 'invalidDate': true });
        this.appDateMask?.updateValueAndValidity();
        return;
      }

      if ((size == 2 && newValue < 32) || (size == 5 && Number(newValue.split('/')[1]) < 13)) {
        newValue += '/';
      }
    }

    this.model.control?.setErrors({ 'invalidDate': false });
    this.model.control?.setValue(new Date(newValue));
    this.model!.valueAccessor?.writeValue(newValue);
    this.appDateMask?.updateValueAndValidity();
  }

}
