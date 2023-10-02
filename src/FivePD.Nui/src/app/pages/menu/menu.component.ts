import {
  AfterViewInit,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnInit,
  QueryList,
  SimpleChanges,
  ViewChildren
} from '@angular/core';
import { MenuItemType } from '@enums';
import { BaseNuiEvent, Menu, MenuCheckboxItem, MenuItem, MenuListItem } from '@models';
import { EventService } from '@services/event/event.service';
import { takeUntil } from 'rxjs';
import { injectDestroy } from "../../helpers/destroy-ref";

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.sass']
})
export class MenuComponent implements OnInit, AfterViewInit, OnChanges {

  destroy$ = injectDestroy();

  @Input() menu!: Menu;

  currentItemDescription: string = '';
  menuContainer: HTMLElement | null = null;
  selectorNode: HTMLElement | null = null;
  menuItems: MenuItem[] = [];

  @ViewChildren('item') items!: QueryList<ElementRef<HTMLDivElement>>;

  constructor(
    private _event: EventService,
  ) {
    this._event.onEvent<BaseNuiEvent>('fivepd:menu:control_up')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuUp());
    this._event.onEvent<BaseNuiEvent>('fivepd:menu:control_down')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuDown());
    this._event.onEvent<BaseNuiEvent>('fivepd:menu:control_left')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuLeft());
    this._event.onEvent<BaseNuiEvent>('fivepd:menu:control_right')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuRight());
    this._event.onEvent<BaseNuiEvent>('fivepd:menu:control_select')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuSelect());
  }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    const menu = changes['menu']?.currentValue;
    if (!menu) {
      return;
    }

    this.menuItems = menu.items ?? [];
    this.menu.currentItemIndex$.next(0);
    this.currentItemDescription = '';

    if (this.selectorNode) {
      this.selectorNode.style.top = '0px';
    }

    if (this.items) {
      const currentItemNode = this.items.get(this.menu.currentItemIndex$.value);
      currentItemNode?.nativeElement.scrollIntoView({behavior: 'auto', block: 'nearest'});
    }

    this.menu.currentItemIndex$.subscribe(index => {
      if (index === 0 && this.selectorNode) {
        this.selectorNode.style.top = '0px';
      }
    });
  }

  ngAfterViewInit(): void {
    this.menuContainer = document.querySelector('.menu-items');
    this.selectorNode = document.querySelector('#selector');
  }

  public get MenuItemType() {
    return MenuItemType;
  }

  onMenuUp(): void {
    if (!this.selectorNode || !this.items?.length || !this.menuItems) {
      return;
    }

    const previousIndex = this.menu.currentItemIndex$.value;
    if (this.menu.currentItemIndex$.value > 0) this.menu.currentItemIndex$.next(this.menu.currentItemIndex$.value - 1);
    else this.menu.currentItemIndex$.next(this.menuItems.length - 1);

    const currentItem = this.menuItems[this.menu.currentItemIndex$.value];
    if (!currentItem.Enabled) {
      this.onMenuUp();
      return;
    }

    this.currentItemDescription = currentItem.Description;

    const currentItemNode = this.items.get(this.menu.currentItemIndex$.value);
    this.selectorNode.style.top = `${currentItemNode?.nativeElement.offsetTop}px`;
    currentItemNode?.nativeElement.scrollIntoView({
      behavior: previousIndex === 0 ? 'auto' : 'smooth',
      block: 'nearest'
    });
  }

  onMenuDown(): void {
    if (!this.selectorNode || !this.items?.length || !this.menuItems) {
      return;
    }

    const previousIndex = this.menu.currentItemIndex$.value;
    if (this.menu.currentItemIndex$.value < this.menuItems.length - 1) {
      this.menu.currentItemIndex$.next(this.menu.currentItemIndex$.value + 1);
    } else {
      this.menu.currentItemIndex$.next(0);
    }

    const currentItem = this.menuItems[this.menu.currentItemIndex$.value];
    if (!currentItem.Enabled) {
      this.onMenuDown();
      return;
    }

    this.currentItemDescription = currentItem.Description;

    const currentItemNode = this.items.get(this.menu.currentItemIndex$.value);
    this.selectorNode.style.top = `${currentItemNode?.nativeElement.offsetTop}px`;
    currentItemNode?.nativeElement.scrollIntoView({
      behavior: previousIndex > this.menu.currentItemIndex$.value ? 'auto' : 'smooth',
      block: 'nearest'
    });
  }

  onMenuLeft(): void {
    this.onListSlide('left');
  }

  onMenuRight(): void {
    this.onListSlide('right');
  }

  onMenuSelect() {
    if (!this.selectorNode || !this.items?.length || !this.menuItems) {
      return;
    }

    const currentItem = this.menuItems[this.menu.currentItemIndex$.value];
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

    if (currentItem.type === MenuItemType.List) {
      const item = currentItem as MenuListItem;
      if (!item.items[item.currentIndex].Enabled) {
        return;
      }
    }

    currentItem.onPress();
  }

  getMarqueeClass(item: MenuListItem): Record<string, boolean> {
    if (item.items.length <= item.currentIndex) {
      return {};
    }

    const className = item.items[item.currentIndex].Title.length > 10 ? 'marquee' : '';

    return {
      [className]: true,
    }
  }

  onListSlide(dir: 'left' | 'right'): void {
    if (!this.selectorNode || !this.items?.length || !this.menuItems) {
      return;
    }

    const currentItem: MenuListItem = this.menuItems[this.menu.currentItemIndex$.value] as MenuListItem;
    if (currentItem.type !== MenuItemType.List) {
      return;
    }

    const currentItemNode = this.items.get(this.menu.currentItemIndex$.value);
    const leftArrow = currentItemNode?.nativeElement.querySelector('.left-arrow');
    const rightArrow = currentItemNode?.nativeElement.querySelector('.right-arrow');

    const arrow = dir === 'left' ? leftArrow : rightArrow;
    arrow!.classList.add('on-press');
    setTimeout(() => {
      arrow!.classList.remove('on-press');
    }, 100);

    if (dir === 'left') {
      currentItem.prev();
    } else {
      currentItem.next();
    }
  }

  protected readonly MenuListItem = MenuListItem;
  protected readonly MenuCheckboxItem = MenuCheckboxItem;
}
