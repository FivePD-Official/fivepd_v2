import { Component, Input, OnInit } from '@angular/core';
import { IconType } from '@models';
import { NgClass } from '@angular/common';

@Component({
    selector: '[app-icon]',
    templateUrl: './icon.component.html',
    styleUrls: ['./icon.component.sass'],
    imports: [NgClass]
})
export class IconComponent implements OnInit {

  @Input() name?: IconType | keyof typeof IconType;
  @Input() className: string = '';
  @Input() ngClassName: object = {};
  internalName?: string;

  constructor() {
  }

  ngOnInit(): void {
    if (typeof this.name === 'string') {
      const indexOf = Object.values(IconType).indexOf(this.name as unknown as IconType);
      this.internalName = Object.keys(IconType)[indexOf];
    }
  }

  get IconType() {
    return IconType;
  }

}
