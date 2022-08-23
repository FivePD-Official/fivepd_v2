import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from '@models';

@Component({
  selector: 'app-button',
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.sass']
})
export class ButtonComponent implements OnInit {

  @Input() label: Button['label'] = '';
  @Input() theme: Button['theme'] = 'primary';
  @Input() type?: Button['type'] = 'button';
  @Input() show: boolean = true;
  @Input() disabled: Button['disabled'] = false;
  @Input() link?: Button['link'];
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
