import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  Input,
  OnChanges,
  OnInit,
  QueryList,
  SimpleChanges,
  ViewChild,
  ViewChildren
} from '@angular/core';
import { MenuItemType } from '@enums';
import { BaseNuiEvent, Menu, MenuCheckboxItem, MenuItem, MenuListItem } from '@models';
import { EventService } from '@services/event/event.service';
import { takeUntil } from 'rxjs';
import { injectDestroy } from "../../helpers/destroy-ref";
import { AsPipe } from '../../pipes/as.pipe';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.sass'],
  standalone: true,
  imports: [NgClass, TranslatePipe, AsPipe]
})
export class MenuComponent implements OnInit, AfterViewInit, OnChanges {

  protected readonly MenuListItem = MenuListItem;
  protected readonly MenuCheckboxItem = MenuCheckboxItem;
  protected readonly MenuItemType = MenuItemType;

  protected currentItemDescription: string = '';
  protected menuItems: MenuItem[] = [];

  private readonly destroy$ = injectDestroy();
  private readonly _eventService = inject(EventService);

  @Input({required: true}) menu!: Menu;

  @ViewChildren('element') elements!: QueryList<ElementRef<HTMLDivElement>>;
  @ViewChildren('dividerElement') dividerElements!: QueryList<ElementRef<HTMLDivElement>>;
  @ViewChildren('itemElement') itemElements!: QueryList<ElementRef<HTMLDivElement>>;

  @ViewChild('selector') selectorElement!: ElementRef<HTMLDivElement>;

  ngOnInit(): void {
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_up')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuUp());
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_down')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuDown());
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_left')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuLeft());
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_right')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuRight());
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_select')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onMenuSelect());
  }

  ngAfterViewInit() {
    if (this.selectorElement?.nativeElement) {
      const first = this.nonDividerMenuItems[0];
      const index = this.menuItems.findIndex(x => x.hashcode === first.hashcode);
      const item = this.elements.get(index);
      this.selectorElement.nativeElement.style.transition = 'none'; // remove if we'll decide to add back transitions
      this.selectorElement.nativeElement.style.top = item ? item.nativeElement.offsetTop + 'px' : '0px';
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    const menu = changes['menu']?.currentValue;
    if (!menu) {
      return;
    }

    this.menuItems = menu.items ?? [];
    this.menu.currentItemIndex$.next(0);
    this.currentItemDescription = '';

    if (this.elements) {
      const currentItemNode = this.elements.get(this.menu.currentItemIndex$.value);
      currentItemNode?.nativeElement.scrollIntoView({behavior: 'auto', block: 'nearest'});
    }
  }

  get nonDividerMenuItems() {
    return this.menuItems.filter(x => !x.divider);
  }

  get dividerMenuItems() {
    return this.menuItems.filter(x => x.divider);
  }

  get menuItemsHeight() {
    if (this.menuItems.length >= 5) {
      return '200px';
    }

    const itemsHeight = this.menuItems.length * 40;
    const dividerItemsHeight = this.dividerMenuItems.length * 40;

    return (itemsHeight + dividerItemsHeight) + 'px';
  }

  onMenuUp(): void {
    if (!this.selectorElement.nativeElement || !this.itemElements?.length || !this.nonDividerMenuItems) {
      return;
    }

    const previousIndex = this.menu.currentItemIndex$.value;
    if (this.menu.currentItemIndex$.value > 0) this.menu.currentItemIndex$.next(this.menu.currentItemIndex$.value - 1);
    else this.menu.currentItemIndex$.next(this.nonDividerMenuItems.length - 1);

    const currentItem = this.nonDividerMenuItems[this.menu.currentItemIndex$.value];
    if (!currentItem.Enabled) {
      this.onMenuUp();
      return;
    }

    this.currentItemDescription = currentItem.Description;

    const currentItemNode = this.itemElements.get(this.menu.currentItemIndex$.value);
    this.selectorElement.nativeElement.style.top = `${currentItemNode?.nativeElement.offsetTop}px`;

    const index = this.menuItems.findIndex(x => x.hashcode === currentItem.hashcode);
    let closestDividerToCurrentItemNode: ElementRef<HTMLDivElement> | undefined;
    if (this.menuItems[index - 1]?.divider) {
      closestDividerToCurrentItemNode = this.elements.get(index - 1);
    } else {
      closestDividerToCurrentItemNode = currentItemNode;
    }
    closestDividerToCurrentItemNode?.nativeElement.scrollIntoView({
      // uncomment if we'll decide to add back transitions - behavior: previousIndex === 0 ? 'auto' : 'smooth',
      behavior: 'auto',
      block: 'nearest'
    });
  }

  onMenuDown(): void {
    if (!this.selectorElement.nativeElement || !this.itemElements?.length || !this.nonDividerMenuItems) {
      return;
    }

    const previousIndex = this.menu.currentItemIndex$.value;
    if (this.menu.currentItemIndex$.value < this.nonDividerMenuItems.length - 1) {
      this.menu.currentItemIndex$.next(this.menu.currentItemIndex$.value + 1);
    } else {
      this.menu.currentItemIndex$.next(0);
    }

    const currentItem = this.nonDividerMenuItems[this.menu.currentItemIndex$.value];
    if (!currentItem.Enabled) {
      this.onMenuDown();
      return;
    }

    this.currentItemDescription = currentItem.Description;

    const currentItemNode = this.itemElements.get(this.menu.currentItemIndex$.value);
    this.selectorElement.nativeElement.style.top = `${currentItemNode?.nativeElement.offsetTop}px`;

    const index = this.menuItems.findIndex(x => x.hashcode === currentItem.hashcode);
    let closestDividerToCurrentItemNode: ElementRef<HTMLDivElement> | undefined;
    if (this.menuItems[index - 1]?.divider && previousIndex > this.menu.currentItemIndex$.value) {
      closestDividerToCurrentItemNode = this.elements.get(index - 1);
    } else {
      closestDividerToCurrentItemNode = currentItemNode;
    }
    closestDividerToCurrentItemNode?.nativeElement.scrollIntoView({
      // uncomment if we'll decide to add back transitions - behavior: previousIndex > this.menu.currentItemIndex$.value ? 'auto' : 'smooth',
      behavior: 'auto',
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
    if (!this.selectorElement.nativeElement || !this.elements?.length || !this.menuItems) {
      return;
    }

    const currentItem = this.menuItems[this.menu.currentItemIndex$.value];
    if (!currentItem.Enabled) {
      return;
    }

    // Change DOM state
    this.selectorElement.nativeElement.classList.add('selector--on-select');
    setTimeout(() => {
      this.selectorElement.nativeElement!.classList.remove('selector--on-select');
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
    if (!this.elements?.length || !this.menuItems) {
      return;
    }

    const currentItem: MenuListItem = this.menuItems[this.menu.currentItemIndex$.value] as MenuListItem;
    if (currentItem.type !== MenuItemType.List) {
      return;
    }

    const currentItemNode = this.elements.get(this.menu.currentItemIndex$.value);
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
}
