import { Component, Input } from '@angular/core';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { RouterLink, RouterLinkActive } from '@angular/router';

export interface Tab {
  label: string;
  routerLink: string;
  disabled?: () => boolean;
}

@Component({
    selector: 'app-tabs',
    templateUrl: './tabs.component.html',
    styleUrls: ['./tabs.component.sass'],
    imports: [RouterLinkActive, RouterLink, TranslatePipe]
})
export class TabsComponent {
  @Input({required: true}) tabs: Tab[] = [];
}
