import { Component, inject, OnInit } from '@angular/core';
import { Button, Collation, Column, LooseDataAttribute, Page, SingleSelectField } from "@models";
import { CoreSetId } from "@enums";
import { LooseDataService } from "@services/loose-data/loose-data.service";
import { AttributeGridOptions } from "../_service/attribute-grid-options";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { GridComponent } from '../../../../components/grid/grid.component';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import {
  SingleSelectFieldComponent
} from "../../../../components/form-inputs/single-select-field/single-select-field.component";
import { LocalizationService } from "@services/localization/localization.service";

@Component({
  selector: 'app-attributes',
  templateUrl: './attributes.component.html',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, SingleSelectFieldComponent]
})
export class AttributesComponent implements OnInit {

  private readonly _looseData = inject(LooseDataService);
  private readonly _attributeGridOptions = inject(AttributeGridOptions);
  private readonly _localizationService = inject(LocalizationService);

  readonly setAttributeDropdown = new SingleSelectField({
    key: '#',
    value: CoreSetId.ArrestReport as any,
    defaultItemLabel: '',
    addDefaultItem: false,
    items: [
      {key: CoreSetId.ArrestReport, label: this._localizationService.translate('UI.LABEL.LOOSE_DATA_SET.TYPE.ARREST_REPORTS.TITLE')},
      {key: CoreSetId.IncidentReport, label: this._localizationService.translate('UI.LABEL.LOOSE_DATA_SET.TYPE.INCIDENT_REPORTS.TITLE')},
      {key: CoreSetId.PedTicket, label: this._localizationService.translate('UI.LABEL.LOOSE_DATA_SET.TYPE.PED_TICKETS.TITLE')},
      {key: CoreSetId.PedWarning, label: this._localizationService.translate('UI.LABEL.LOOSE_DATA_SET.TYPE.PED_WARNINGS.TITLE')},
    ],
    size: 'normal',
    height: 'base',
    variant: 'secondary',
    required: false,
  });
  selectedSet: string = CoreSetId.ArrestReport;

  page?: Page<LooseDataAttribute>;
  collation: Collation = {
    Page: 1,
    Filters: {},
  };
  columns: Column<LooseDataAttribute>[] = [];
  attributesButtons: Button[] = [];

  ngOnInit(): void {
    this.getAttributes();
    this.setAttributesGrid();
  }

  setAttributesGrid(): void {
    this.collation.Page = 1;
    this.getAttributes();

    this.columns = this._attributeGridOptions.createColumnList(
      this.selectedSet,
      () => this.getAttributes(),
      () => {
        this.collation.Page = 1;
        this.getAttributes();
      }
    );
    this.attributesButtons = this._attributeGridOptions.createButtons(
      this.selectedSet,
      () => {
        this.collation.Page = 1;
        this.getAttributes();
      }
    );
  }

  onTableCellActionSelect(selectedItemKey: any): void {
    this.selectedSet = selectedItemKey;
    this.setAttributesGrid();
  }

  getAttributes(): void {
    this._looseData.getSetAttributesPage(this.selectedSet, this.collation).subscribe(res => {
      this.page = res;
    });
  }

  onAttributesCollationChange(collation: Collation): void {
    this.collation = collation;
    this.getAttributes();
  }
}
