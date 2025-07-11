import { Component, Inject, OnInit } from '@angular/core';
import { ATTRIBUTES, ENTITY } from './entity-token';
import { VariableDataAttribute, VariableDataEntityWrapper, MappedVariableDataEntity } from '@models';
import { tap } from 'rxjs';
import { ModalRef } from '@services/modals/modal/modal-ref';
import { Router } from "@angular/router";
import { IconComponent } from '../../icon/icon.component';
import { NgStyle } from '@angular/common';
import { EntityValue, EntityValueLink, VariableDataService } from "@services/variable-data/variable-data.service";

@Component({
    selector: 'app-modal-entity-view',
    templateUrl: './modal-entity-view.component.html',
    styleUrls: ['./modal-entity-view.component.sass'],
    imports: [NgStyle, IconComponent]
})
export class ModalEntityViewComponent implements OnInit {

  entityValueColumns: EntityValue[][] = [];
  columnsGridStyle: string = '';

  constructor(
    private modalRef: ModalRef,
    @Inject(ENTITY) private mappedEntity: MappedVariableDataEntity,
    @Inject(ATTRIBUTES) private attributes: VariableDataAttribute[],
    private _variableDataService: VariableDataService,
    private readonly _router: Router,
  ) {
  }

  ngOnInit(): void {
    this.mapEntityValues({
      entity: this.mappedEntity,
      attributes: this.attributes
    });
    this.getEntity();
    this.columnsGridStyle = `1fr / repeat(${this.modalRef.options?.columns ?? 1}, 1fr)`;
  }

  isArray(value: any): value is EntityValueLink[] {
    return Array.isArray(value);
  }

  onLinkClick(link: EntityValueLink): void {
    this.modalRef.close({value: {}, success: true});
    this._router.navigate(link.link);
  }

  private getEntity(): void {
    this.modalRef.startLoading('Updating...');
    this._variableDataService.getEntity(this.mappedEntity.SetId, this.mappedEntity.Id)
      .pipe(
        tap(() => this.modalRef.stopLoading())
      )
      .subscribe(res => {
        this.mapEntityValues(res);
      });
  }

  private mapEntityValues(entityWrapper: VariableDataEntityWrapper): void {
    const values = this._variableDataService.mapEntityValues(entityWrapper);
    const columns = [];
    for (let i = this.modalRef.options?.columns ?? 1; i > 0; i--) {
      columns.push(values.splice(0, Math.ceil(values.length / i)));
    }
    this.entityValueColumns = columns;
  }
}
