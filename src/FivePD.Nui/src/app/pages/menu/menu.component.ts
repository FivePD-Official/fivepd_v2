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
    imports: [NgClass, TranslatePipe, AsPipe]
})
export class MenuComponent implements OnInit, AfterViewInit, OnChanges {

  protected readonly MenuListItem = MenuListItem;
  protected readonly MenuCheckboxItem = MenuCheckboxItem;
  protected readonly MenuItemType = MenuItemType;

  protected currentItemDescription: string = '';
  protected items: MenuItem[] = [];
  protected nonDividerItems: MenuItem[] = [];

  private readonly destroy$ = injectDestroy();
  private readonly _eventService = inject(EventService);

  private menuWidth = '';

  @Input({required: true}) menu!: Menu;

  @ViewChildren('element') elements!: QueryList<ElementRef<HTMLDivElement>>;
  @ViewChildren('dividerElement') dividerElements!: QueryList<ElementRef<HTMLDivElement>>;
  @ViewChildren('itemElement') itemElements!: QueryList<ElementRef<HTMLDivElement>>;

  @ViewChild('selector') selectorElement!: ElementRef<HTMLDivElement>;

  @ViewChild('itemsContainer') itemsContainerElement!: ElementRef<HTMLDivElement>;
  @ViewChildren('descriptionContainer') descriptionContainerElement!: QueryList<ElementRef<HTMLDivElement>>;

  ngOnInit(): void {
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_up')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onVerticalControl('up'));
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_down')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onVerticalControl('down'));
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_left')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onHorizontalControl('left'));
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_right')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onHorizontalControl('right'));
    this._eventService.onEvent<BaseNuiEvent>('fivepd:menu:control_select')
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.onSelect());
  }

  ngAfterViewInit(): void {
    if (this.selectorElement?.nativeElement) {
      const firstItem = this.nonDividerItems[0];
      if (firstItem) {
        const index = this.items.findIndex(x => x.hashcode === firstItem.hashcode);
        const item = this.elements.get(index);
        this.selectorElement.nativeElement.style.top = item ? item.nativeElement.offsetTop + 'px' : '0px';
      }
    }

    if (this.itemsContainerElement?.nativeElement) {
      const width = Math.round(this.itemsContainerElement.nativeElement.getBoundingClientRect().width) + 20;
      this.menuWidth = `${width}px`
      this.itemsContainerElement.nativeElement.style.width = this.menuWidth;
    }

    this.descriptionContainerElement.changes
      .pipe(takeUntil(this.destroy$))
      .subscribe((queryList: QueryList<ElementRef<HTMLDivElement>>) => {
        if (this.menuWidth && queryList.first?.nativeElement) {
          queryList.first.nativeElement.style.width = this.menuWidth;
          queryList.first.nativeElement.style.maxWidth = this.menuWidth;
        }
      })
  }

  ngOnChanges(changes: SimpleChanges): void {
    const menu = changes['menu']?.currentValue;
    if (!menu) {
      return;
    }

    this.items = menu.items ?? [];
    this.nonDividerItems = this.items.filter(x => !x.divider);
    this.menu.currentItemIndex$.next(0);
    this.currentItemDescription = '';

    if (this.elements) {
      const currentItemNode = this.elements.get(this.menu.currentItemIndex$.value);
      currentItemNode?.nativeElement.scrollIntoView({behavior: 'auto', block: 'nearest'});
    }
  }

  getMarqueeClass(item: MenuListItem): Record<string, boolean> {
    if (item.Options.length <= item.currentIndex) {
      return {};
    }

    const className = item.Options[item.currentIndex].Title.length > 10 ? 'marquee' : '';

    return {
      [className]: true,
    }
  }

  get menuItemsHeight(): string {
    if (this.items.length >= 10) {
      return '400px';
    }

    return this.items.length * 40 + 'px';
  }

  private onSelect(): void {
    if (!this.selectorElement?.nativeElement || !this.elements?.length) {
      return;
    }

    const currentItem = this.nonDividerItems[this.menu.currentItemIndex$.value];
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
      if (!item.Options[item.currentIndex].Enabled) {
        return;
      }
    }

    currentItem.onPress();
  }

  private onVerticalControl(dir: 'up' | 'down'): void {
    if (!this.selectorElement?.nativeElement || !this.itemElements?.length) {
      return;
    }

    const previousIndex = this.menu.currentItemIndex$.value;
    let newIndex: number;
    if (dir === 'up') {
      if (previousIndex > 0) {
        newIndex = previousIndex - 1;
      } else {
        newIndex = this.nonDividerItems.length - 1;
      }
    } else {
      if (previousIndex < this.nonDividerItems.length - 1) {
        newIndex = previousIndex + 1;
      } else {
        newIndex = 0;
      }
    }
    this.menu.currentItemIndex$.next(newIndex);

    const currentItem = this.nonDividerItems[this.menu.currentItemIndex$.value];

    this.currentItemDescription = currentItem.Description;

    const currentItemNode = this.itemElements.get(this.menu.currentItemIndex$.value);
    this.selectorElement.nativeElement.style.top = `${currentItemNode?.nativeElement.offsetTop}px`;

    const itemIndex = this.items.findIndex(x => x.hashcode === currentItem.hashcode);
    const previousItem = this.items[itemIndex - 1];
    let closestDividerToCurrentItemNode: ElementRef<HTMLDivElement> | undefined;
    if (
      dir === 'up' && (previousItem?.divider)
      ||
      dir === 'down' && (previousItem?.divider && previousIndex > this.menu.currentItemIndex$.value)
    ) {
      closestDividerToCurrentItemNode = this.elements.get(itemIndex - 1);
    } else {
      closestDividerToCurrentItemNode = currentItemNode;
    }
    closestDividerToCurrentItemNode?.nativeElement.scrollIntoView({
      behavior: 'auto',
      block: 'nearest'
    });
  }

  private onHorizontalControl(dir: 'left' | 'right'): void {
    if (!this.elements?.length) {
      return;
    }

    const currentItem: MenuListItem = this.nonDividerItems[this.menu.currentItemIndex$.value] as MenuListItem;
    if (currentItem.type !== MenuItemType.List) {
      return;
    }

    const currentItemNode = this.itemElements.get(this.menu.currentItemIndex$.value);
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
