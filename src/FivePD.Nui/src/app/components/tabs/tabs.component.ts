import { AfterContentInit, Component, ContentChildren, inject, Input, OnInit, QueryList } from '@angular/core';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { Router } from '@angular/router';
import { NgClass } from "@angular/common";
import { TabComponent } from './tab/tab.component';

export interface Tab {
  label: string;
  routerLink: string;
  disabled?: () => boolean;
}

@Component({
  selector: 'app-tabs',
  templateUrl: './tabs.component.html',
  styleUrls: ['./tabs.component.sass'],
  imports: [TranslatePipe, NgClass]
})
export class TabsComponent implements OnInit, AfterContentInit {
  @Input({required: true}) tabs: Tab[] = [];
  @Input({required: false}) displayChildren = false;

  @ContentChildren(TabComponent) tabComponents!: QueryList<TabComponent>;

  activeTab: Tab | undefined = undefined;

  readonly router = inject(Router);

  ngOnInit(): void {
    const firstTab = this.tabs && this.tabs[0];
    if (firstTab) {
      this.onTabSelected(firstTab);
    }
  }

  ngAfterContentInit(): void {
    if (this.displayChildren) {
      this.tabComponents.forEach(tab => tab.hidden = true);
      this.setTabVisibility();
    }
  }

  onTabSelected(tab: Tab): void {
    this.activeTab = tab;

    if (this.displayChildren) {
      this.setTabVisibility();
    } else {
      void this.router.navigateByUrl(tab.routerLink);
    }
  }

  private setTabVisibility(): void {
    const selectedTab = this.tabComponents.toArray().find(t => t.title === this.activeTab?.label);
    if (selectedTab) {
      this.tabComponents.forEach(tab => tab.hidden = true);
      selectedTab.hidden = false;
    }
  }
}
