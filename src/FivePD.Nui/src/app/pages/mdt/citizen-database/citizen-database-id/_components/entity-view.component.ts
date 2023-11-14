import { Component, EventEmitter, inject, Input, Output } from "@angular/core";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { IconComponent } from "../../../../../components/icon/icon.component";
import { ButtonComponent } from "../../../../../components/button/button.component";
import { NgFor, NgIf } from "@angular/common";
import { Router } from "@angular/router";
import { EntityValue, EntityValueLink } from "@services/loose-data/loose-data.service";

@Component({
  selector: 'entity-view',
  template: `
    <div class="flex flex-col gap-4">
      <div class="w-full flex flex-row items-center justify-between">
        <p class="common-title">
          {{ title | translate }}
        </p>
        <app-button *ngIf="showCreateButton"
                    [label]="'UI.ACTION.CREATE' | translate"
                    (onClick)="createButtonClick.emit()"
                    variant="secondary"/>
      </div>
      <div class="flex flex-col gap-4">
        <div *ngFor="let ticket of entities" class="bg-zinc-800 rounded-md p-4 flex flex-col gap-2">
          <div *ngFor="let item of ticket" class="grid grid-rows-1 grid-cols-2 gap-4">
            <p class="text-zinc-400 font-medium">{{ item.name }}</p>
            <ng-container *ngIf="typeof(item.value) === 'string'; else links">
              <p class="text-zinc-400">{{ item.value }}</p>
            </ng-container>
            <ng-template #links>
              <div class="flex flex-col gap-2.5">
                <button *ngFor="let link of $any(item.value)"
                        (click)="onLinkClick(link)"
                        class="group flex flex-row items-center text-zinc-300 hover:text-white">
                  {{ link.label }}
                  <div app-icon name="Link"
                       className="ml-2.5 h-[20px] text-zinc-300 group-hover:text-white"></div>
                </button>
              </div>
            </ng-template>
          </div>
        </div>
      </div>
      <p *ngIf="!entities?.length"
         class="text-zinc-400 block">
        {{ 'UI.LABEL.NO_RESULTS' | translate }}
      </p>
    </div>`,
  standalone: true,
  imports: [
    TranslatePipe,
    IconComponent,
    ButtonComponent,
    NgIf,
    NgFor,
  ]
})
export class EntityViewComponent {

  private readonly _router = inject(Router);

  @Input() title: string = '';
  @Input() entities: EntityValue[][] = [];
  @Input() showCreateButton = false;
  @Output() createButtonClick = new EventEmitter<void>();

  typeof(value: any): string {
    return typeof value;
  }

  onLinkClick(link: EntityValueLink): void {
    void this._router.navigate(link.link);
  }
}
