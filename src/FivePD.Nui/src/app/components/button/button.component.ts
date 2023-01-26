import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { Button, ButtonSizes, ButtonVariants } from '@models';

const variants: Record<ButtonVariants, string> = {
  'primary': `text-white flex flex-col justify-center items-center
              border border-transparent
              before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
              before:bg-primary
              disabled:cursor-not-allowed disabled:opacity-50`,
  'secondary': `text-zinc-900 dark:text-white flex flex-col justify-center items-center
              border border-zinc-600 rounded-md
              before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
              before:bg-transparent
              disabled:cursor-not-allowed disabled:opacity-50`,
  'tertiary': `text-black dark:text-white flex flex-col justify-center items-center
              border border-transparent
              before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
              before:bg-zinc-800
              disabled:cursor-not-allowed disabled:opacity-50`,
  'tertiary-bg': `text-black dark:text-white flex flex-col justify-center items-center
              border border-transparent
              before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
              before:bg-zinc-700
              disabled:cursor-not-allowed disabled:opacity-50`,
  'danger': `text-white flex flex-col justify-center items-center
              border border-transparent
              before:absolute before:inset-0 before:w-full before:h-full before:rounded-md
              before:bg-red-600
              disabled:cursor-not-allowed disabled:opacity-50`,
};

const buttonVariants: Record<ButtonVariants, string> = {
  'primary': 'enabled:hover:before:opacity-75',
  'secondary': 'enabled:hover:border-zinc-900 enabled:dark:hover:border-white',
  'tertiary': 'enabled:hover:before:bg-zinc-200 enabled:hover:before:dark:bg-zinc-700',
  'tertiary-bg': 'enabled:hover:before:bg-zinc-300 enabled:hover:before:dark:bg-zinc-600',
  'danger': 'enabled:hover:before:opacity-75',
};

const sizeVariants: Record<ButtonSizes, string> = {
  'base': 'text-base',
  'sm': 'text-sm font-medium',
};

@Component({
  selector: 'app-button',
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.sass']
})
export class ButtonComponent implements OnInit, OnChanges {

  @Input() label: Button['label'] = '';
  @Input() variant: ButtonVariants = 'primary';
  @Input() size: ButtonSizes = 'base';
  @Input() type: Button['type'] = 'button';
  @Input() show: boolean = true;
  @Input() disabled: boolean = false;
  @Input() link?: Button['link'];
  @Output() onClick = new EventEmitter();

  className: string[] = [];

  constructor(
    private _router: Router,
  ) { }

  ngOnInit(): void {
    this.setClassName();
  }

  ngOnChanges(): void {
    this.setClassName();
  }

  private setClassName(): void {
    // if (this.disabled) {
    //   this.className = {
    //     'button--disabled': true
    //   };
    // }

    this.className = [
      variants[this.variant!],
      buttonVariants[this.variant!],
      sizeVariants[this.size!],
      // width === 'max' ? 'w-max' : 'w-full',
      'h-[40px] px-4 relative',
    ];
  }

  internalOnClick(): void {
    if (this.link) {
      this._router.navigate([...this.link]);
    } else {
      this.onClick.emit();
    }
  }

}
