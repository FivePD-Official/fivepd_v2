import { Component, EventEmitter, inject, Input, Output } from "@angular/core";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { IconComponent } from "../../../../../components/icon/icon.component";
import { ButtonComponent } from "../../../../../components/button/button.component";
import { Router } from "@angular/router";
import { EntityValue, EntityValueLink } from "@services/variable-data/variable-data.service";

@Component({
  selector: 'entity-view',
  template: `
    <div class="flex flex-col gap-4">
      <div class="w-full flex flex-row items-center justify-between">
        <p class="common-title">{{ title | translate }}</p>
        @if (showCreateButton) {
          <app-button [label]="'UI.ACTION.CREATE' | translate"
                      (onClick)="createButtonClick.emit()"
                      variant="secondary"/>
        }
      </div>
      <div class="flex flex-col gap-4">
        @for (entity of entities;track $index) {
          <div class="bg-zinc-800 rounded-md p-4 flex flex-col gap-2">
            @for (attribute of entity;track $index) {
              <div class="grid grid-rows-1 grid-cols-2 gap-4">
                <p class="text-zinc-400 font-medium">{{ attribute.name }}</p>
                @if (isArray(attribute.value)) {
                  <div class="flex flex-col gap-2.5">
                    @for (link of $any(attribute.value);track $index) {
                      <button (click)="onLinkClick(link)"
                              class="group flex flex-row items-center text-zinc-300 hover:text-white">
                        {{ link.label }}
                        <div app-icon name="Link"
                             className="ml-2.5 h-[20px] text-zinc-300 group-hover:text-white"></div>
                      </button>
                    }
                  </div>
                } @else {
                  <p class="text-zinc-400">{{ attribute.value }}</p>
                }
              </div>
            }
          </div>
        } @empty {
          <p class="text-zinc-400 block">{{ 'UI.LABEL.NO_RESULTS' | translate }}</p>
        }
      </div>
    </div>`,
  standalone: true,
  imports: [
    TranslatePipe,
    IconComponent,
    ButtonComponent,
  ]
})
export class EntityViewComponent {

  private readonly _router = inject(Router);

  @Input() title: string = '';
  @Input() entities: EntityValue[][] = [];
  @Input() showCreateButton = false;
  @Output() createButtonClick = new EventEmitter<void>();

  isArray(value: any): boolean {
    return Array.isArray(value);
  }

  onLinkClick(link: EntityValueLink): void {
    void this._router.navigate(link.link);
  }
}
