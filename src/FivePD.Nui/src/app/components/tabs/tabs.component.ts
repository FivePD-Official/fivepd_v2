import { Component, Input } from '@angular/core';

export interface Tab {
  label: string;
  routerLink: string;
}

@Component({
  selector: 'app-tabs',
  templateUrl: './tabs.component.html',
  styleUrls: ['./tabs.component.sass']
})
export class TabsComponent {

  @Input() tabs: Tab[] = [];

}
