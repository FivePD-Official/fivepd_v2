import { Component } from '@angular/core';
import { Tab, TabsComponent } from '../../../../components/tabs/tabs.component';
import { TranslatePipe } from '../../../../pipes/translate.pipe';
import { RouterOutlet } from '@angular/router';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';

@Component({
    selector: 'app-devtools-view',
    templateUrl: './devtools-view.component.html',
    styleUrls: ['./devtools-view.component.sass'],
    imports: [PageHeaderComponent, TabsComponent, RouterOutlet, TranslatePipe]
})
export class DevtoolsViewComponent {

  tabs: Tab[] = [
    {
      label: 'LABEL.GENERAL',
      routerLink: '/mdt/devtools'
    },
    {
      label: 'LABEL.CALLOUT.PLURAL',
      routerLink: '/mdt/devtools/callouts'
    },
  ];
}
