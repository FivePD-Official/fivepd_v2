import { Component, Inject, OnInit } from '@angular/core';
import { ATTRIBUTES, ENTITY } from './entity-token';
import { LooseDataAttribute, LooseDataEntityWrapper, MappedLooseDataEntity } from '@models';
import { EntityValue, EntityValueLink, LooseDataService } from '@services/loose-data/loose-data.service';
import { tap } from 'rxjs';
import { ModalRef } from '@services/modals/modal/modal-ref';
import { Router } from "@angular/router";
import { IconComponent } from '../../icon/icon.component';
import { NgStyle } from '@angular/common';

@Component({
  selector: 'app-modal-entity-view',
  templateUrl: './modal-entity-view.component.html',
  styleUrls: ['./modal-entity-view.component.sass'],
  standalone: true,
  imports: [NgStyle, IconComponent]
})
export class ModalEntityViewComponent implements OnInit {

  entityValueColumns: EntityValue[][] = [];
  columnsGridStyle: string = '';

  constructor(
    private modalRef: ModalRef,
    @Inject(ENTITY) private mappedEntity: MappedLooseDataEntity,
    @Inject(ATTRIBUTES) private attributes: LooseDataAttribute[],
    private _looseData: LooseDataService,
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

  typeof(value: any): string {
    return typeof value;
  }

  onLinkClick(link: EntityValueLink): void {
    this.modalRef.close({value: {}, success: true});
    this._router.navigate(link.link);
  }

  private getEntity(): void {
    this.modalRef.startLoading('Updating...');
    this._looseData.getEntity(this.mappedEntity.SetId, this.mappedEntity.Id)
      .pipe(
        tap(() => this.modalRef.stopLoading())
      )
      .subscribe(res => {
        this.mapEntityValues(res);
      });
  }

  private mapEntityValues(entityWrapper: LooseDataEntityWrapper): void {
    const values = this._looseData.mapEntityValues(entityWrapper);
    const columns = [];
    for (let i = this.modalRef.options?.columns ?? 1; i > 0; i--) {
      columns.push(values.splice(0, Math.ceil(values.length / i)));
    }
    this.entityValueColumns = columns;
  }

}
