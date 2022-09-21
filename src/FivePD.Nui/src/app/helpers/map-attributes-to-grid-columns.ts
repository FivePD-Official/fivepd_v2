import { Column, LooseDataEntity, SetAttribute } from '@models';

export interface InsertableItem<T> {
  index: number;
  item: T;
}

export function mapAttributesToGridColumns(attributes: SetAttribute[], itemsToInsert: InsertableItem<Column<LooseDataEntity>>[] = []): Column<LooseDataEntity>[] {
  const attributesWithoutOrder = [];
  const attributesWithOrder = [];
  for (const attr of attributes.filter(attr => attr.ShowInGrid)) {
    if (attr.Order) {
      attributesWithOrder.push(attr)
    } else {
      attributesWithoutOrder.push(attr);
    }
  }

  const columns: Column<LooseDataEntity>[] = attributesWithOrder
    .sort((a,b) => {
      if (!a.Order || !b.Order) return -1;
      if (a.Order < b.Order) return -1;
      if (a.Order > b.Order) return 1;
      return 0;
    })
    .map(attr => {
      return new Column<LooseDataEntity>({
        header: attr.Label!,
        accessorKey: attr.Name,
        show: () => true,
      })
    });

  for (const attr of attributesWithoutOrder) {
    columns.push(new Column<LooseDataEntity>({
      header: attr.Label!,
      accessorKey: attr.Name,
      show: () => true,
    }))
  }

  for (const item of itemsToInsert) {
    columns.splice(item.index, 0, item.item);
  }

  return columns;
}
