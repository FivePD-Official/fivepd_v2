import { Id } from "@models";

export function parseId(id: Id): number {
  if (typeof id === 'string') {
    id = parseInt(id);
  }
  return id;
}
