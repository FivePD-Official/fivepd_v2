import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-button',
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.sass']
})
export class ButtonComponent implements OnInit {

  @Input() label: string = '';
  @Input() theme: 'primary' | 'secondary' | 'small' | 'danger' = 'primary';
  @Input() type?: 'button' | 'submit' = 'button';
  @Input() show: boolean = true;
  @Input() disabled: boolean = false;
  @Input() link?: string[];
  @Output() onClick = new EventEmitter();

  constructor(
    private _router: Router,
  ) { }

  ngOnInit(): void {
  }

  get className() {
    if (this.disabled) {
      return {
        'button--disabled': true
      };
    }

    return {
      'button--primary': this.theme === 'primary',
      'button--secondary': this.theme === 'secondary',
      'button--small': this.theme === 'small',
      'button--danger': this.theme === 'danger',
    };
  }

  internalOnClick(): void {
    if (this.link) {
      this._router.navigate([...this.link]);
    } else {
      this.onClick.emit();
    }
  }

}
