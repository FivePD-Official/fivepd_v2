import { Component, Input } from '@angular/core';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgClass, NgFor } from '@angular/common';

export interface Tab {
  label: string;
  routerLink: string;
  disabled?: () => boolean;
}

@Component({
    selector: 'app-tabs',
    templateUrl: './tabs.component.html',
    styleUrls: ['./tabs.component.sass'],
    imports: [NgFor, RouterLinkActive, RouterLink, TranslatePipe, NgClass]
})
export class TabsComponent {
  @Input({required: true}) tabs: Tab[] = [];
}
