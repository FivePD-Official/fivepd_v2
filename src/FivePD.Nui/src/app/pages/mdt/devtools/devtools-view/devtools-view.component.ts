import { Component } from '@angular/core';
import { Tab } from '../../../../components/tabs/tabs.component';

@Component({
  selector: 'app-devtools-view',
  templateUrl: './devtools-view.component.html',
  styleUrls: ['./devtools-view.component.sass']
})
export class DevtoolsViewComponent {

  tabs: Tab[] = [
    {
      label: 'General',
      routerLink: '/mdt/devtools'
    },
    {
      label: 'Callouts',
      routerLink: '/mdt/devtools/callouts'
    },
    {
      label: 'Plugins',
      routerLink: '/mdt/devtools/plugins'
    }
  ];
}
