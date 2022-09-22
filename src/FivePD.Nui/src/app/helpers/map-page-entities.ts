import { MappedLooseDataEntity, Page } from '@models';

export function mapPageEntities(page: Page<MappedLooseDataEntity>): Page<MappedLooseDataEntity> {
  page.Items = page.Items.map(entity => {
    for (const [key, value] of Object.entries(entity)) {
      if (value instanceof Date) {
        const date = new Date(value);
        entity[key] = new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(date);
      }
    }
    return entity;
  });

  return page;
}
