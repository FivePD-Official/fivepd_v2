import { Component, Input } from '@angular/core';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NgFor } from '@angular/common';

export interface Tab {
  label: string;
  routerLink: string;
}

@Component({
  selector: 'app-tabs',
  templateUrl: './tabs.component.html',
  styleUrls: ['./tabs.component.sass'],
  standalone: true,
  imports: [NgFor, RouterLinkActive, RouterLink, TranslatePipe]
})
export class TabsComponent {

  @Input() tabs: Tab[] = [];

}
