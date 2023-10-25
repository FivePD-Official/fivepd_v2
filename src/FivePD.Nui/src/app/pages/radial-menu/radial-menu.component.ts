import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs';
import { MenuShowEvent, RadialMenu, RadialMenuControlEvent } from '@models';
import { EventService } from '@services/event/event.service';
import { ActivatedRoute } from '@angular/router';
import { MenuService } from '@services/menu/menu.service';
import { HttpClient } from '@angular/common/http';

interface MousePosition {
  X: number;
  Y: number;
}

@Component({
  selector: 'app-radial-menu',
  templateUrl: './radial-menu.component.html',
  styleUrls: ['./radial-menu.component.sass'],
  standalone: true
})
export class RadialMenuComponent implements OnInit, OnDestroy {

  @ViewChild('canvas') canvas!: ElementRef<HTMLCanvasElement>;

  menuShowEvent$?: Subscription;
  menuControlEvent$?: Subscription;

  selectedMenu: RadialMenu | null = null;
  selectedItemIndex: number = -1;
  currentMousePosition: MousePosition = {X: 0, Y: 0};
  lastMousePosition: MousePosition = {X: 0, Y: 0};
  lastSelectAngle: number = -1;

  constructor(
    private _event: EventService,
    private _route: ActivatedRoute,
    private _menu: MenuService,
    private _http: HttpClient,
  ) {
  }

  ngOnInit(): void {
    this.findSelectedMenu(this._route.snapshot.params['id']);
    this.drawCanvas();

    this.menuShowEvent$ = this._event.onEvent<MenuShowEvent>('fivepd:ui:view:radialMenu').subscribe(event => {
      this.resetValues();
      this.findSelectedMenu(event.MenuId);
      this.drawCanvas();
    });

    this.menuControlEvent$ = this._event.onEvent<RadialMenuControlEvent>('fivepd:radialMenu:control').subscribe(event => {
      const {X, Y, Control} = event;
      if (Control === 'up' || Control === 'down') {
        if (X && Y) {
          this.currentMousePosition = {X: X, Y: Y};
          this.onMouseMove();
        }
      } else if (Control === 'select') {
        if (this.selectedItemIndex !== -1) {
          this._http.post(`fivepd:radialMenu:trigger_item_${this.selectedMenu?.Items[this.selectedItemIndex].Hashcode}`, {}).subscribe();
          this.resetValues();
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.menuShowEvent$?.unsubscribe();
    this.menuControlEvent$?.unsubscribe();
  }

  resetValues(): void {
    this.lastSelectAngle = -1;
    this.selectedItemIndex = -1;
    this.currentMousePosition = {X: 0, Y: 0};
    this.lastMousePosition = {X: 0, Y: 0};
  }

  findSelectedMenu(id: string): void {
    this.selectedMenu = this._menu.getRadialMenuById(id);
  }

  onMouseMove(): void {
    // Compute the change from last time around
    const deltaX = this.currentMousePosition.X - this.lastMousePosition.X;
    const deltaY = this.currentMousePosition.Y - this.lastMousePosition.Y;

    // Take the inverse tangent of the movement
    let angle = Math.atan2(deltaY, deltaX);

    // atan2 returns on -pi through pi, but we want 0 through 2pi. Convert!
    if (angle < 0) {
      angle += (2 * Math.PI);
    }

    // Check if this was a deliberate movement. Small changes can be ignored for now
    if (deltaX > 20 || deltaX < -20 || deltaY > 20 || deltaY < -20) {
      // Update the angles and redraw!
      this.lastMousePosition = this.currentMousePosition;
      this.lastSelectAngle = angle;
      this.drawCanvas();
    }
  }

  drawCanvas(): void {
    if (!this.selectedMenu || !this.canvas) return;

    const menu: RadialMenu = this.selectedMenu;

    // All of the configuration stuff for canvas (2d context)
    const ctx: CanvasRenderingContext2D = this.canvas.nativeElement.getContext('2d')!;
    ctx.resetTransform();
    ctx.clearRect(0, 0, this.canvas.nativeElement.width, this.canvas.nativeElement.height);
    ctx.textAlign = 'center';
    ctx.font = '16px Arial';

    // Configuration
    const TOTAL_RADIUS = this.canvas.nativeElement.width / 2;
    const SECTION_DEPTH = 100; // How deep should each section appear?

    // Normally the origin is in the top left. Move it to the middle.
    ctx.translate(this.canvas.nativeElement.width / 2, this.canvas.nativeElement.height / 2);

    // Calculate the arc length of each section
    const arcLength = 2 * Math.PI / menu.Items.length;

    // Calculate the average radius of each section
    const midRadius = TOTAL_RADIUS - (SECTION_DEPTH / 2);

    // Increment through each section
    for (let i = 0; i < menu.Items.length; i++) {

      // Calculate angle for start and stop
      const start = arcLength * i;
      const stop = arcLength * (i + 1);

      // Uncomment for aim assist!
      ctx.beginPath();
      ctx.moveTo(0, 0);
      ctx.lineTo(50 * Math.cos(this.lastSelectAngle), 50 * Math.sin(this.lastSelectAngle));
      ctx.stroke();

      // Draw arcs
      ctx.beginPath();
      ctx.arc(0, 0, TOTAL_RADIUS, start, stop);
      ctx.arc(0, 0, TOTAL_RADIUS - SECTION_DEPTH, stop, start, true);
      ctx.stroke();

      // Calculate angle of section middle
      const middle = arcLength * (i + 0.5);

      // Show the selected item as being special
      if (this.lastSelectAngle >= start && this.lastSelectAngle < stop) {
        ctx.fillStyle = 'red';
        this.selectedItemIndex = i;
      } else {
        ctx.fillStyle = 'black';
      }

      // Draw demo text in middle(ish) of each box
      ctx.fillText(menu.Items[i].Title, midRadius * Math.cos(middle), midRadius * Math.sin(middle));
    }
  }

}
