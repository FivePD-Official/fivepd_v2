import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, QueryList, ViewChildren } from '@angular/core';
import { MenuItemType, NuiEventType } from '@enums';
import { BaseNuiEvent, Menu, MenuCheckboxItem, MenuItem, MenuListItem } from '@models';
import { ActivatedRoute } from '@angular/router';
import { MenuService } from '@services/menu/menu.service';
import { EventService } from '@services/event/event.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.sass']
})
export class MenuComponent implements OnInit, AfterViewInit, OnDestroy {

  menu: Menu | null = null;

  currentItemIndex: number = 0;
  currentItemDescription: string = '';
  menuContainer: HTMLElement | null = null;
  selectorNode: HTMLElement | null = null;
  menuItems: MenuItem[] = [];
  controlEvent$: Subscription | null = null;

  @ViewChildren('item') items!: QueryList<ElementRef<HTMLDivElement>>;

  constructor(
    private _route: ActivatedRoute,
    private _menuService: MenuService,
    private _event: EventService,
  ) {
  }

  ngOnInit(): void {
    this._route.params.subscribe(params => {
      this.menu = this._menuService.getMenuById(params['id']);
      this.menuItems = this.menu?.items ?? [];
      this.currentItemIndex = 0;
      this.currentItemDescription = '';

      if (this.selectorNode) {
        this.selectorNode.style.top = '0px';
      }

      if (this.items) {
        const currentItemNode = this.items.get(this.currentItemIndex);
        currentItemNode?.nativeElement.scrollIntoView({ behavior: 'auto', block: 'nearest' });
      }
    });
    this.controlEvent$ = this._event.onEvent<BaseNuiEvent>(NuiEventType.MenuUp, NuiEventType.MenuDown, NuiEventType.MenuLeft, NuiEventType.MenuRight, NuiEventType.MenuSelect).subscribe(event => {
      switch (event.type) {
        case NuiEventType.MenuUp: {
          this.onMenuUp();
          break;
        }
        case NuiEventType.MenuDown: {
          this.onMenuDown();
          break;
        }
        case NuiEventType.MenuLeft: {
          this.onMenuLeft();
          break;
        }
        case NuiEventType.MenuRight: {
          this.onMenuRight();
          break;
        }
        case NuiEventType.MenuSelect: {
          this.onMenuSelect();
          break;
        }
      }
    });
  }

  ngAfterViewInit(): void {
    this.menuContainer = document.querySelector('.menu-items');
    this.selectorNode = document.querySelector('#selector');
  }

  ngOnDestroy(): void {
    this.controlEvent$?.unsubscribe();
  }

  public get MenuItemType() {
    return MenuItemType;
  }

  onMenuUp(): void {
    if (!this.selectorNode || !this.items?.length || !this.menuItems) {
      return;
    }

    const previousIndex = this.currentItemIndex;
    if (this.currentItemIndex > 0) this.currentItemIndex = this.currentItemIndex - 1;
    else this.currentItemIndex = this.menuItems.length - 1;

    const currentItem = this.menuItems[this.currentItemIndex];
    if (!currentItem.Enabled) {
      this.onMenuUp();
      return;
    }

    this.currentItemDescription = currentItem.Description;

    const currentItemNode = this.items.get(this.currentItemIndex);
    this.selectorNode.style.top = `${currentItemNode?.nativeElement.offsetTop}px`;
    currentItemNode?.nativeElement.scrollIntoView({ behavior: previousIndex === 0 ? 'auto' : 'smooth', block: 'nearest' });
  }

  onMenuDown(): void {
    if (!this.selectorNode || !this.items?.length || !this.menuItems) {
      return;
    }

    const previousIndex = this.currentItemIndex;
    if (this.currentItemIndex < this.menuItems.length - 1) {
      this.currentItemIndex = this.currentItemIndex + 1;
    } else {
      this.currentItemIndex = 0;
    }

    const currentItem = this.menuItems[this.currentItemIndex];
    if (!currentItem.Enabled) {
      this.onMenuDown();
      return;
    }

    this.currentItemDescription = currentItem.Description;

    const currentItemNode = this.items.get(this.currentItemIndex);
    this.selectorNode.style.top = `${currentItemNode?.nativeElement.offsetTop}px`;
    currentItemNode?.nativeElement.scrollIntoView({ behavior: previousIndex > this.currentItemIndex ? 'auto' : 'smooth', block: 'nearest' });
  }

  onMenuLeft(): void {
    this.onListSlide(NuiEventType.MenuLeft);
  }

  onMenuRight(): void {
    this.onListSlide(NuiEventType.MenuRight);
  }

  onMenuSelect() {
    if (!this.selectorNode || !this.items?.length|| !this.menuItems) {
      return;
    }

    const currentItem = this.menuItems[this.currentItemIndex];
    if (!currentItem.Enabled) {
      return;
    }

    // Change DOM state
    this.selectorNode.classList.add('selector--on-select');
    setTimeout(() => {
      this.selectorNode!.classList.remove('selector--on-select');
    }, 100);

    // Change actual item state
    if (currentItem.type === MenuItemType.Toggle) {
      (currentItem as MenuCheckboxItem).toggle();
    }
    currentItem.onPress();
  }

  getMarqueeClass(item: MenuListItem): Record<string, boolean> {
    if (item.items.length <= item.currentIndex) {
      return {};
    }

    const className = item.items[item.currentIndex].length > 10 ? 'marquee' : '';

    return {
      [className]: true,
    }
  }

  onListSlide(dir: string): void {
    if (!this.selectorNode || !this.items?.length || !this.menuItems) {
      return;
    }

    const currentItem: MenuListItem = this.menuItems[this.currentItemIndex] as MenuListItem;
    if (currentItem.type !== MenuItemType.List) {
      return;
    }

    const currentItemNode = this.items.get(this.currentItemIndex);
    const leftArrow = currentItemNode?.nativeElement.querySelector('.left-arrow');
    const rightArrow = currentItemNode?.nativeElement.querySelector('.right-arrow');

    const arrow = dir === NuiEventType.MenuLeft ? leftArrow : rightArrow;
    arrow!.classList.add('on-press');
    setTimeout(() => {
      arrow!.classList.remove('on-press');
    }, 100);

    if (dir === NuiEventType.MenuLeft) {
      currentItem.prev();
    } else {
      currentItem.next();
    }
  }
}
