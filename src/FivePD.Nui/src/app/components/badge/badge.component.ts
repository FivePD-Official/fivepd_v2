import { Component, Input } from '@angular/core';
import { NgClass } from "@angular/common";

export type BadgeVariants = 'gray' | 'red' | 'green' | 'amber' | 'blue';

@Component({
    selector: 'app-badge',
    imports: [
        NgClass
    ],
    templateUrl: './badge.component.html',
    styleUrl: './badge.component.sass'
})
export class BadgeComponent {
  @Input({required: true}) variant!: BadgeVariants;
  @Input({required: true}) label!: string;
}
