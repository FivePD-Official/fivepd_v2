import { Component } from '@angular/core';
import { Tab, TabsComponent } from "../../../components/tabs/tabs.component";
import { RouterOutlet } from "@angular/router";
import { PageHeaderComponent } from "../../../components/page-header/page-header.component";
import { TranslatePipe } from "../../../pipes/translate.pipe";

@Component({
  selector: 'app-scene-management',
  imports: [TabsComponent, RouterOutlet, PageHeaderComponent, TranslatePipe],
  templateUrl: './scene-management.component.html'
})
export class SceneManagementComponent {

  protected readonly tabs: Tab[] = [
    {
      label: 'Speed zones',
      routerLink: '/mdt/scene-management'
    },
    {
      label: 'Objects',
      routerLink: '/mdt/scene-management/objects',
      disabled: () => true,
    },
  ];
}
