import { MappedLooseDataEntity, Page } from '@models';
import { formatDate } from '@helpers';

export function mapPageEntities(page: Page<MappedLooseDataEntity>): Page<MappedLooseDataEntity> {
  page.Items = page.Items.map(entity => {
    for (const [key, value] of Object.entries(entity)) {
      if (value instanceof Date) {
        entity[key] = formatDate(new Date(value));
      }
    }
    return entity;
  });

  return page;
}
