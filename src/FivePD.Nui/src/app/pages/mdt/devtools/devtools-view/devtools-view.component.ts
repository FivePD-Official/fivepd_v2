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
      label: 'UI.LABEL.GENERAL',
      routerLink: '/mdt/devtools'
    },
    {
      label: 'UI.LABEL.CALLOUT.PLURAL',
      routerLink: '/mdt/devtools/callouts'
    },
  ];
}
